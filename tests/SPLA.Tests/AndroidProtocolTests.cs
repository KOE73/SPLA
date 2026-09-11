using System.Buffers.Binary;
using System.IO.Compression;
using SPLA.Domain.Agent;
using SPLA.Domain.Models;
using SPLA.Domain.Security;
using SPLA.Plugins.Android;
using SPLA.Plugins.Android.Adb;
using SPLA.Plugins.Android.Gestures;
using SPLA.Plugins.Android.Scrcpy;
using SPLA.Plugins.Android.Session;
using SPLA.Plugins.Android.Tools;
using SPLA.Plugins.Android.Ui;
using SPLA.Plugins.Android.Video;

namespace SPLA.Tests;

public sealed class AndroidProtocolTests
{
    [Fact]
    public void Control_messages_match_wire_examples()
    {
        Assert.Equal(Convert.FromHexString("0200000000000000000100000064000000C804380780FFFF0000000000000000"), ControlMessages.Touch(0,1,100,200,1080,1920,1));
        Assert.Equal(Convert.FromHexString("0000000000040000000000000000"),ControlMessages.Key(0,4));
        Assert.Equal(Convert.FromHexString("01000000026869"),ControlMessages.Text("hi"));
        Assert.Equal(Convert.FromHexString("0900000000000000000100000002D18F"),ControlMessages.SetClipboard("я",true));
        Assert.Throws<ArgumentException>(() => ControlMessages.Text(new string('я',151)));
    }

    [Fact]
    public async Task Demux_merges_config_preserves_pts_and_detects_truncation()
    {
        using var stream = new MemoryStream();
        var session = new byte[12]; session[0] = 0x80;
        BinaryPrimitives.WriteInt32BigEndian(session.AsSpan(4),64); BinaryPrimitives.WriteInt32BigEndian(session.AsSpan(8),32); stream.Write(session);
        void Packet(ulong flags, byte[] bytes)
        {
            var header = new byte[12]; BinaryPrimitives.WriteUInt64BigEndian(header,flags); BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(8),bytes.Length);
            stream.Write(header); stream.Write(bytes);
        }
        Packet(1UL<<62,[1,2]); Packet((1UL<<61)|123,[3,4]);
        stream.Position = 0; List<DemuxItem> items = [];
        await foreach (var item in VideoDemuxer.ReadAsync(stream,default)) items.Add(item);
        Assert.Equal(new SessionInfo(64,32),items[0]);
        var packet = Assert.IsType<VideoPacket>(items[1]); Assert.Equal(new byte[]{1,2,3,4},packet.Data); Assert.Equal(123,packet.Pts); Assert.True(packet.KeyFrame);
        var truncated = stream.ToArray()[..^1];
        await Assert.ThrowsAsync<EndOfStreamException>(async () => { await foreach (var _ in VideoDemuxer.ReadAsync(new MemoryStream(truncated),default)) { } });
    }

    [Fact]
    public void Adb_arguments_do_not_use_host_shell_and_preserve_tokens()
    {
        var start = AdbRunner.BuildStartInfo(@"C:\runtime\adb.exe",5038,"serial with spaces",["shell","input","text","'a;b'"]);
        Assert.False(start.UseShellExecute); Assert.True(start.CreateNoWindow);
        Assert.Equal(new[]{"-s","serial with spaces","shell","input","text","'a;b'"}, start.ArgumentList);
        Assert.Equal("5038",start.Environment["ANDROID_ADB_SERVER_PORT"]);
        Assert.Equal("'a%s'\\'';$(id)'",AdbBackend.EscapeText("a ';$(id)"));
        Assert.Throws<NotSupportedException>(() => AdbBackend.EscapeText("Привет"));
        Assert.Throws<NotSupportedException>(() => AdbBackend.EscapeText("%s"));
    }

    [Fact]
    public void Device_parser_includes_unauthorized_and_no_permissions()
    {
        var devices = AdbDevices.Parse("List of devices attached\nUSB device model:Phone product:test transport_id:1\n192.0.2.1:5555 unauthorized\nOther offline\nNo no permissions (user in plugdev group)\n* daemon started successfully\n");
        Assert.Equal(4,devices.Count); Assert.Equal("Phone",devices[0].Model); Assert.Equal("unauthorized",devices[1].State); Assert.Equal("no permissions",devices[3].State);
    }

    [Fact]
    public void Gestures_validate_bounds_and_pinch_orders_both_fingers()
    {
        var gesture = GestureBuilders.Pinch(100,100,20,100,0,64);
        GestureBuilders.Validate(gesture,200,200);
        var timeline = GesturePlayer.Timeline(gesture);
        Assert.Equal(new[]{0,1},timeline.Where(e=>e.Action==0).Select(e=>e.Pointer));
        Assert.Equal(new[]{1,0},timeline.Where(e=>e.Action==1).Select(e=>e.Pointer));
        Assert.Equal(timeline.Select(e=>e.Time).Order(),timeline.Select(e=>e.Time));
        Assert.Throws<ArgumentException>(()=>GestureBuilders.Validate(GestureBuilders.Tap(200,0),200,200));
        Assert.Throws<ArgumentException>(()=>GestureBuilders.Validate(GestureBuilders.Swipe(0,0,1,1,60001),200,200));
    }

    [Fact]
    public async Task Cancelled_gesture_releases_pressed_fingers()
    {
        using var cancel = new CancellationTokenSource(); List<byte[]> sent = [];
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => GesturePlayer.PlayAsync(GestureBuilders.LongPress(10,10,500),100,100,
            (message,_) => { sent.Add(message); if (message[1]==0) cancel.Cancel(); return Task.CompletedTask; },cancel.Token));
        Assert.Equal(0,sent[0][1]); Assert.Equal(1,sent[^1][1]);
    }

    [Fact]
    public void Png_round_trips_sub_filter_and_yuv_neutrals()
    {
        byte[] pixels = [255,0,0,0,255,0,0,0,255,255,255,255,0,0,0,127,128,129];
        var png = PngEncoder.Encode(pixels,3,2);
        Assert.Equal(3,BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(16)));
        using var compressed = new MemoryStream();
        for (var offset=8;offset<png.Length;)
        {
            var length = BinaryPrimitives.ReadInt32BigEndian(png.AsSpan(offset));
            var type = System.Text.Encoding.ASCII.GetString(png,offset+4,4);
            var storedCrc = BinaryPrimitives.ReadUInt32BigEndian(png.AsSpan(offset+8+length));
            uint crc=uint.MaxValue;
            foreach (var value in png.AsSpan(offset+4,length+4)) { crc^=value; for(var bit=0;bit<8;bit++) crc=(crc>>1)^((crc&1)==1?0xEDB88320u:0); }
            Assert.Equal(crc^uint.MaxValue,storedCrc);
            if(type=="IDAT") compressed.Write(png,offset+8,length);
            offset+=length+12;
        }
        compressed.Position=0; using var zlib=new ZLibStream(compressed,CompressionMode.Decompress); using var raw=new MemoryStream(); zlib.CopyTo(raw);
        var data=raw.ToArray(); var decoded=new byte[18];
        for(var row=0;row<2;row++) { Assert.Equal(1,data[row*10]); for(var x=0;x<9;x++) decoded[row*9+x]=unchecked((byte)(data[row*10+x+1]+(x>=3?decoded[row*9+x-3]:0))); }
        Assert.Equal(pixels,decoded);
        Assert.All(YuvToRgb.ToRgb24(new(2,2,[235,235,235,235],[128],[128],2,1,false,0)),b=>Assert.Equal(255,b));
        Assert.All(YuvToRgb.ToRgb24(new(2,2,[16,16,16,16],[128,128],[],2,2,true,0)),b=>Assert.Equal(0,b));
    }

    [Fact]
    public async Task Latest_frame_waits_for_content_evidence_and_wakes_waiters()
    {
        var slot=new LatestFrame(); var pending=slot.WaitNextAsync(-1,TimeSpan.FromSeconds(1),default);
        YuvFrame Frame(byte luma)=>new(2,2,[luma,luma,luma,luma],[128],[128],2,1,false,0);
        slot.Publish(Frame(16)); Assert.Equal(1,(await pending).Sequence);
        Assert.False((await slot.WaitStableAsync(20,50,default)).Settled);
        var stable=slot.WaitStableAsync(20,1000,default);
        await Task.Delay(40); slot.Publish(Frame(16)); Assert.True((await stable).Settled);
        Assert.True((await slot.WaitStableAsync(20,50,default)).Settled);
        slot.InvalidateStability();
        Assert.False((await slot.WaitStableAsync(20,40,default)).Settled);
        var next=slot.WaitNextAsync(slot.Current!.Sequence,TimeSpan.FromSeconds(1),default);
        slot.Fail(new IOException("unplugged")); await Assert.ThrowsAsync<IOException>(()=>next);
    }

    [Fact]
    public void Ui_dump_scales_refs_and_rejects_dtd()
    {
        var xml="""<hierarchy><node bounds="[0,0][1000,2000]"><node text="Отправить" class="android.Button" clickable="true" bounds="[400,1000][600,1200]" /></node></hierarchy>""";
        var dump=UiAutomatorDump.Parse(xml,500,1000);
        Assert.Equal(250,dump.Refs["e1"].X); Assert.Equal(550,dump.Refs["e1"].Y); Assert.Contains("Отправить",dump.Text);
        Assert.Empty(UiAutomatorDump.Parse(xml,500,1000,"missing").Refs);
        Assert.Throws<System.Xml.XmlException>(()=>UiAutomatorDump.Parse("<!DOCTYPE foo [<!ENTITY x SYSTEM 'file:///secret'>]><hierarchy/>",100,100));
        Assert.True(DataOrigin.Device("phone").RaisesDoubt);
    }

    [Fact]
    public async Task Screenshot_is_an_image_and_observes_origin()
    {
        var owner=new AgentSession(null!,null!,null!,chatId:"test");
        var session=new DeviceSession("phone",owner,new FakeBackend(),null,new());
        var result=await AndroidToolBase.ScreenshotResult(session,"Test",false,default);
        Assert.True(owner.Doubt.IsRaised);
        Assert.Single(result.Content.OfType<ToolImage>());
    }

    internal sealed class FakeBackend : IDeviceBackend
    {
        public bool Disposed { get; private set; }
        public string Kind=>"fake"; public (int Width,int Height) ScreenSize=>(2,2); public bool SupportsMultiTouch=>true;
        public ValueTask DisposeAsync() { Disposed=true; return ValueTask.CompletedTask; }
        public Task<Screenshot> CaptureAsync(bool waitStable,CancellationToken ct)=>Task.FromResult(new Screenshot(PngEncoder.Encode(new byte[12],2,2),2,2,Kind,false,0));
        public Task PlayAsync(Gesture gesture,CancellationToken ct)=>Task.CompletedTask;
        public Task KeyAsync(int keycode,bool longPress,CancellationToken ct)=>Task.CompletedTask;
        public Task<TextEntryResult> TypeTextAsync(string text,CancellationToken ct)=>Task.FromResult(new TextEntryResult("fake",false));
    }
}
