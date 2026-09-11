using SPLA.Domain.Agent;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Session;

namespace SPLA.Tests;

public sealed class AndroidSessionTests
{
    [Fact]
    public async Task Lease_refuses_other_chat_takeover_releases_and_idle_reaps()
    {
        var registry=new DeviceSessionRegistry();
        var first=new AgentSession(null!,null!,null!,chatId:"first"); var second=new AgentSession(null!,null!,null!,chatId:"second");
        List<AndroidProtocolTests.FakeBackend> backends=[];
        Task<IReadOnlyList<AdbDevice>> List(CancellationToken _) => Task.FromResult<IReadOnlyList<AdbDevice>>([new("phone","device",null,null,null)]);
        Task<(IDeviceBackend,string?)> Create(string serial,CancellationToken _) { var backend=new AndroidProtocolTests.FakeBackend(); backends.Add(backend); return Task.FromResult<(IDeviceBackend,string?)>((backend,null)); }
        Task<string> Use(IAgentSession owner,bool takeover=false)=>registry.UseAsync(owner,"phone",takeover,List,Create,new(),session=>Task.FromResult(session.OwnerLabel),default);
        Assert.Equal("first",await Use(first));
        var error=await Assert.ThrowsAsync<InvalidOperationException>(()=>Use(second)); Assert.Contains("first",error.Message);
        Assert.Equal("second",await Use(second,true)); Assert.True(backends[0].Disposed);
        await registry.ReleaseAsync(second,default); Assert.True(backends[1].Disposed);
        await Use(first); await registry.ReapAsync(DateTimeOffset.UtcNow.AddMinutes(16)); Assert.True(backends[2].Disposed);
    }
}
