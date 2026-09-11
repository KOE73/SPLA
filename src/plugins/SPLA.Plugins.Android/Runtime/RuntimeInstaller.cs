using System.Collections.Concurrent;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SPLA.Plugins.Android.Adb;

namespace SPLA.Plugins.Android.Runtime;

public sealed record InstallStatus(
    [property: JsonPropertyName("component")] string Component,
    [property: JsonPropertyName("folder")] string Folder,
    [property: JsonPropertyName("stage")] string Stage,
    [property: JsonPropertyName("bytesDone")] long BytesDone,
    [property: JsonPropertyName("bytesTotal")] long? BytesTotal,
    [property: JsonPropertyName("error")] string? Error);

public sealed class RuntimeInstaller
{
    public static RuntimeInstaller Instance { get; } = new(new HttpClient { Timeout = TimeSpan.FromMinutes(15) });
    private readonly HttpClient http;
    private readonly Func<RuntimeManifest> manifest;
    private readonly Func<string, CancellationToken, Task<string>> probeAdb;
    private readonly ConcurrentDictionary<string, InstallStatus> jobs = new(StringComparer.OrdinalIgnoreCase);
    private readonly object sync = new();

    internal RuntimeInstaller(HttpClient http, Func<RuntimeManifest>? manifest = null,
        Func<string, CancellationToken, Task<string>>? probeAdb = null)
    {
        this.http = http;
        this.manifest = manifest ?? (() => RuntimeManifest.Load());
        this.probeAdb = probeAdb ?? ProbeAdbAsync;
    }

    public InstallStatus? Get(string folder) => jobs.GetValueOrDefault(Path.GetFullPath(folder));

    public InstallStatus Start(string component, string channel, string folder, int adbServerPort = 0)
    {
        if (component is not ("adb" or "scrcpy")) throw new ArgumentException("Expected adb or scrcpy.");
        if (channel is not ("pinned" or "latest")) throw new ArgumentException("Expected pinned or latest.");
        folder = Path.TrimEndingDirectorySeparator(Path.GetFullPath(folder));
        if (Path.GetPathRoot(folder) == folder) throw new ArgumentException("Choose a dedicated runtime component folder, not a drive root.");
        lock (sync)
        {
            if (Get(folder) is { Stage: not ("done" or "failed") } active) return active;
            if (jobs.Values.Any(job => job.Stage is not ("done" or "failed") &&
                (job.Folder.StartsWith(folder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                 folder.StartsWith(job.Folder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))))
                throw new InvalidOperationException("An installation is already using a parent or child folder.");
            var initial = new InstallStatus(component, folder, "downloading", 0, null, null);
            jobs[folder] = initial;
            _ = Task.Run(() => InstallAsync(initial, channel, adbServerPort));
            return initial;
        }
    }

    private async Task InstallAsync(InstallStatus initial, string channel, int adbServerPort)
    {
        var folder = initial.Folder;
        var archive = folder + ".download-" + Guid.NewGuid().ToString("N") + ".zip";
        var candidate = folder + ".new-" + Guid.NewGuid().ToString("N");
        var previous = folder + ".old-" + Guid.NewGuid().ToString("N");
        using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(15));
        var ct = deadline.Token;
        FileStream? installLock = null;
        void Stage(string stage) => jobs[folder] = jobs[folder] with { Stage = stage };
        try
        {
            var entry = manifest().Component(initial.Component);
            if (channel == "latest") entry = await LatestAsync(initial.Component, ct);
            Directory.CreateDirectory(Path.GetDirectoryName(folder)!);
            installLock = new FileStream(folder + ".install-lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            using (var request = new HttpRequestMessage(HttpMethod.Get, entry.Url))
            {
                request.Headers.UserAgent.ParseAdd("SPLA");
                using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
                response.EnsureSuccessStatusCode();
                jobs[folder] = jobs[folder] with { BytesTotal = response.Content.Headers.ContentLength };
                await using var input = await response.Content.ReadAsStreamAsync(ct);
                await using var output = File.Create(archive);
                var buffer = new byte[81920];
                int count;
                while ((count = await input.ReadAsync(buffer, ct)) != 0)
                {
                    await output.WriteAsync(buffer.AsMemory(0, count), ct);
                    jobs[folder] = jobs[folder] with { BytesDone = jobs[folder].BytesDone + count };
                }
            }
            Stage("verifying");
            string hash;
            await using (var input = File.OpenRead(archive)) hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(input, ct));
            var verified = !string.IsNullOrWhiteSpace(entry.Sha256);
            if ((channel == "pinned" && !verified) || (verified && !hash.Equals(entry.Sha256, StringComparison.OrdinalIgnoreCase)))
                throw new IOException($"SHA-256 mismatch: expected {entry.Sha256}, received {hash}. Existing installation was not changed.");
            Stage("extracting");
            Extract(archive, candidate, initial.Component);
            Stage("probing");
            var version = entry.Version;
            if (initial.Component == "adb")
            {
                version = await probeAdb(RuntimePaths.AdbExe(candidate), ct);
                if (channel == "pinned" && version != entry.Version) throw new IOException($"Expected adb {entry.Version}, received {version}.");
            }
            else if (RuntimePaths.ScrcpyServer(candidate) is null || !Directory.EnumerateFiles(candidate, "avcodec-*.dll").Any() ||
                !Directory.EnumerateFiles(candidate, "avutil-*.dll").Any()) throw new IOException("scrcpy archive is missing server or FFmpeg libraries.");
            RuntimeReceipt.Write(candidate, new(initial.Component, version, channel, entry.Url, hash, verified, DateTimeOffset.UtcNow.ToString("O")));
            if (Directory.Exists(folder))
            {
                // Never replace an arbitrary manually configured directory and erase unrelated SDK files.
                if (RuntimeReceipt.TryRead(folder)?.Component != initial.Component && Directory.EnumerateFileSystemEntries(folder).Any())
                    throw new IOException("The target folder is not managed by SPLA. Choose an empty dedicated component folder.");
                try { Directory.Move(folder, previous); }
                catch (IOException) when (initial.Component == "adb" && File.Exists(RuntimePaths.AdbExe(folder)))
                {
                    await new AdbRunner(RuntimePaths.AdbExe(folder), adbServerPort).CheckedAsync(null, ["kill-server"], ct, 10000);
                    Directory.Move(folder, previous);
                }
            }
            try { Directory.Move(candidate, folder); }
            catch { if (Directory.Exists(previous)) Directory.Move(previous, folder); throw; }
            if (Directory.Exists(previous))
            {
                try { Directory.Delete(previous, true); }
                catch (IOException) when (initial.Component == "adb")
                {
                    await new AdbRunner(RuntimePaths.AdbExe(previous), adbServerPort).CheckedAsync(null, ["kill-server"], ct, 10000);
                    Directory.Delete(previous, true);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                { jobs[folder] = jobs[folder] with { Error = $"Installed; old runtime could not be removed at {previous}: {ex.Message}" }; }
            }
            Stage("done");
        }
        catch (Exception ex) { jobs[folder] = jobs[folder] with { Stage = "failed", Error = ex.Message }; }
        finally
        {
            try { File.Delete(archive); if (Directory.Exists(candidate)) Directory.Delete(candidate, true); }
            catch (Exception ex) { jobs[folder] = jobs[folder] with { Error = $"{jobs[folder].Error} Cleanup: {ex.Message}" }; }
            // Delete while holding the handle with DeleteOnClose would create a race on Windows;
            // the empty lock file is persistent and only its OS-held handle denotes ownership.
            installLock?.Dispose();
        }
    }

    internal static void Extract(string archive, string destination, string component)
    {
        Directory.CreateDirectory(destination);
        using var zip = ZipFile.OpenRead(archive);
        foreach (var entry in zip.Entries)
        {
            var name = entry.FullName.Replace('\\', '/');
            if (name.EndsWith('/')) continue;
            if (component == "adb")
            {
                if (name is not ("platform-tools/adb.exe" or "platform-tools/AdbWinApi.dll" or "platform-tools/AdbWinUsbApi.dll")) continue;
                name = name["platform-tools/".Length..];
            }
            else
            {
                var slash = name.IndexOf('/');
                if (slash < 0 || !name[..slash].StartsWith("scrcpy-win64-v", StringComparison.Ordinal))
                    throw new IOException($"Unexpected archive entry: {name}");
                name = name[(slash + 1)..];
            }
            var path = Path.GetFullPath(Path.Combine(destination, name));
            if (!path.StartsWith(Path.GetFullPath(destination) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || name.Contains(':'))
                throw new IOException("Archive entry escapes the component folder.");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            entry.ExtractToFile(path);
        }
        if (component == "adb" && new[] { "adb.exe", "AdbWinApi.dll", "AdbWinUsbApi.dll" }.Any(name => !File.Exists(Path.Combine(destination, name))))
            throw new IOException("ADB archive is missing required files.");
    }

    private async Task<RuntimeComponent> LatestAsync(string component, CancellationToken ct)
    {
        if (component == "adb") return new("unknown", "https://dl.google.com/android/repository/platform-tools-latest-windows.zip", "");
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/Genymobile/scrcpy/releases/latest");
        request.Headers.UserAgent.ParseAdd("SPLA");
        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var tag = json.RootElement.GetProperty("tag_name").GetString()!;
        var asset = json.RootElement.GetProperty("assets").EnumerateArray().Single(a => a.GetProperty("name").GetString() == $"scrcpy-win64-{tag}.zip");
        var digest = asset.TryGetProperty("digest", out var value) ? value.GetString() : null;
        return new(tag.TrimStart('v'), asset.GetProperty("browser_download_url").GetString()!, digest?.StartsWith("sha256:") == true ? digest[7..] : "");
    }

    private static async Task<string> ProbeAdbAsync(string executable, CancellationToken ct)
    {
        var output = await new AdbRunner(executable, 0).CheckedAsync(null, ["version"], ct, 10000);
        var match = Regex.Match(output, @"(?m)^Version (\d+\.\d+\.\d+)");
        return match.Success ? match.Groups[1].Value : throw new IOException("Cannot determine adb version.");
    }
}
