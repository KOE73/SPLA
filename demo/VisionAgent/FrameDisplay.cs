using OpenCvSharp;

namespace SPLA.Demo.Vision;

/// <summary>
/// Optional on-screen preview (vision.show_windows): two HighGUI windows on a dedicated thread —
/// "Live" shows the continuous source feed, "Sent" shows the exact frame that just went to the
/// model. All HighGUI calls (NamedWindow/ImShow/WaitKey) stay on the one owning thread; producers
/// only swap cloned Mats into slots, so neither the capture pump nor the agent turn ever blocks
/// on rendering.
/// </summary>
public sealed class FrameDisplay : IDisposable
{
    private const string LiveWindow = "SPLA Vision - Live";
    private const string SentWindow = "SPLA Vision - Sent to model";

    private readonly object _gate = new();
    private Mat? _live, _sent;
    private readonly Thread _thread;
    private volatile bool _stop;

    public FrameDisplay()
    {
        _thread = new Thread(Loop) { IsBackground = true, Name = "vision-display" };
        _thread.Start();
    }

    public void ShowLive(Mat frame) => Swap(ref _live, frame);

    public void ShowSent(Mat frame) => Swap(ref _sent, frame);

    private void Swap(ref Mat? slot, Mat frame)
    {
        if (_stop) return;
        var clone = frame.Clone();
        lock (_gate)
        {
            slot?.Dispose();   // предыдущий кадр не успел отрисоваться — заменяем
            slot = clone;
        }
    }

    private void Loop()
    {
        Cv2.NamedWindow(LiveWindow, WindowFlags.AutoSize);
        Cv2.NamedWindow(SentWindow, WindowFlags.AutoSize);
        while (!_stop)
        {
            Mat? live, sent;
            lock (_gate)
            {
                live = _live; _live = null;
                sent = _sent; _sent = null;
            }
            if (live != null) { Cv2.ImShow(LiveWindow, live); live.Dispose(); }
            if (sent != null) { Cv2.ImShow(SentWindow, sent); sent.Dispose(); }
            Cv2.WaitKey(30);   // качает message loop окон и задаёт темп ~33 fps
        }
        Cv2.DestroyAllWindows();
    }

    public void Dispose()
    {
        _stop = true;
        _thread.Join(1500);
        lock (_gate)
        {
            _live?.Dispose(); _live = null;
            _sent?.Dispose(); _sent = null;
        }
    }
}
