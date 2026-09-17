using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using SPLA.Plugins.Android.Runtime;

namespace SPLA.Tests;

public sealed class AndroidRuntimeInstallerTests
{
    private sealed class Handler(byte[] zip, bool fail=false) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
        {
            Assert.Contains("SPLA",request.Headers.UserAgent.ToString());
            if(fail) throw new HttpRequestException("connection interrupted");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new ByteArrayContent(zip)});
        }
    }
    private static byte[] Archive(bool traversal=false)
    {
        using var stream=new MemoryStream();
        using(var zip=new ZipArchive(stream,ZipArchiveMode.Create,true))
            foreach(var name in traversal ? new[]{"scrcpy-win64-v4.1/../escape"} : new[]{"scrcpy-server","avcodec-62.dll","avutil-60.dll"}.Select(name=>"scrcpy-win64-v4.1/"+name))
            { using var writer=new StreamWriter(zip.CreateEntry(name).Open()); writer.Write("test"); }
        return stream.ToArray();
    }
    private static async Task<InstallStatus> Wait(RuntimeInstaller installer,string folder)
    {
        using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while(installer.Get(folder) is { Stage: not ("done" or "failed") }) await Task.Delay(10,deadline.Token);
        return installer.Get(folder)!;
    }

    [Theory]
    [InlineData(false,false,"done")]
    [InlineData(true,false,"failed")]
    [InlineData(false,true,"failed")]
    public async Task Hash_verification_and_failure_preserve_the_existing_install(bool badHash,bool networkFailure,string stage)
    {
        var root=Path.Combine(Path.GetTempPath(),"spla-android-test-"+Guid.NewGuid().ToString("N"));
        var folder=Path.Combine(root,"runtime"); Directory.CreateDirectory(folder);
        var zip=Archive(); var hash=Convert.ToHexStringLower(SHA256.HashData(zip));
        var manifest=new RuntimeManifest { Scrcpy=new("4.1","https://example.test/runtime.zip",badHash?new string('0',64):hash) };
        RuntimeReceipt.Write(folder,new("scrcpy","old","pinned","","",true,"")); File.WriteAllText(Path.Combine(folder,"old.txt"),"preserve me");
        try
        {
            using var client=new HttpClient(new Handler(zip,networkFailure));
            var installer=new RuntimeInstaller(client,()=>manifest);
            installer.Start("scrcpy","pinned",folder);
            Assert.Equal(stage,(await Wait(installer,folder)).Stage);
            Assert.Equal(stage=="failed",File.Exists(Path.Combine(folder,"old.txt")));
            Assert.Single(Directory.EnumerateDirectories(root));
            Assert.DoesNotContain(Directory.EnumerateFiles(root), path => path.Contains(".download-"));
            if(stage=="done") { Assert.True(RuntimeReceipt.TryRead(folder)!.Verified); Assert.Equal(hash,RuntimeReceipt.TryRead(folder)!.Sha256); }
        }
        finally { Directory.Delete(root,true); }
    }

    [Fact]
    public async Task Unmanaged_folder_is_not_replaced_and_archive_cannot_escape()
    {
        var root=Path.Combine(Path.GetTempPath(),"spla-android-test-"+Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        try
        {
            var folder=Path.Combine(root,"manual"); Directory.CreateDirectory(folder); File.WriteAllText(Path.Combine(folder,"keep.txt"),"keep");
            var zip=Archive(); using var client=new HttpClient(new Handler(zip));
            var installer=new RuntimeInstaller(client,()=>new RuntimeManifest { Scrcpy=new("4.1","https://example.test/a.zip",Convert.ToHexStringLower(SHA256.HashData(zip))) });
            installer.Start("scrcpy","pinned",folder); Assert.Equal("failed",(await Wait(installer,folder)).Stage); Assert.True(File.Exists(Path.Combine(folder,"keep.txt")));
            var path=Path.Combine(root,"evil.zip"); File.WriteAllBytes(path,Archive(true));
            Assert.Throws<IOException>(()=>RuntimeInstaller.Extract(path,Path.Combine(root,"candidate"),"scrcpy"));
            Assert.False(File.Exists(Path.Combine(root,"escape")));
        }
        finally { Directory.Delete(root,true); }
    }
}
