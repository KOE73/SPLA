# SPLA.Plugins.Android — agent notes

Controls a personal Android phone: live screen for the model, touch (incl. multi-touch), APK install,
and a small toolbox around it. Why each choice was made: `docs/adr/ADR_20260910_plugins_android-device.md`.
Work order: `docs/plans/PLAN_20260910_plugins_android-device.md`. This file is the **wire-level
reference** — keep it matching the code.

Read `src/plugins/AGENTS.md` and `agents/plugins.md` first. Browser plugin (`SPLA.Plugins.Browser`) is
the reference for per-chat sessions, `ToolImage` screenshots and blob storage — copy its patterns.

## Invariants

- **Never use PATH, ANDROID_HOME, or global env.** `adb.exe` and `scrcpy-server` are resolved only via
  `RuntimePaths` (settings path → project-relative → `%LOCALAPPDATA%\SPLA\runtime\android\<component>\`).
  `ANDROID_ADB_SERVER_PORT` is set only on the child process's `ProcessStartInfo.Environment`.
- **Never build a host command line by string concatenation.** Use `ProcessStartInfo.ArgumentList`.
- **Device shell strings:** anything taken from the model that lands inside `adb shell ...` is
  validated (package names: `^[A-Za-z0-9._]+$`) or single-quote-escaped (paths: `'` → `'\''`).
  `android_shell` is the only tool that passes free text to the device shell — on purpose, declared High.
- **One coordinate space.** Every x/y a tool accepts is in the pixel space of the latest screenshot the
  backend produced (`IDeviceBackend.ScreenSize`). The scrcpy backend: video frame size (≤ `max_size`).
  The adb backend: native display size. Every screenshot result states that size.
- **No PNG per frame.** Frames are stored decoded (YUV). PNG is produced only when a tool returns an image.
- **Don't under-declare risk** (see table in the plan). Plugin tools never use `Scope=Agent/Skill`.
- **Never bypass FLAG_SECURE / black frames.**

## Layout

```
SPLA.Plugins.Android/
  AndroidPlugin.cs              ISplaPlugin + ISplaPluginAction + ISplaPluginSelfCheck
  AndroidSettings.cs            settings blob (FromBlob like BrowserSettings)
  runtime-manifest.json         pinned components (copied next to the dll)
  Runtime/
    RuntimeManifest.cs          model of runtime-manifest.json
    RuntimePaths.cs             folder resolution rules
    RuntimeReceipt.cs           spla-runtime.json inside each component folder
    RuntimeInstaller.cs         background install jobs (pinned | latest), status polling
  Adb/
    AdbRunner.cs                runs adb.exe (text or binary stdout, timeout, cancel)
    AdbDevices.cs               `adb devices -l` parser
  Session/
    IDeviceBackend.cs           screen + input contract
    AdbBackend.cs               screencap + `input`
    ScrcpyBackend.cs            server lifecycle + video + control
    DeviceSession.cs            backend + lease owner + last-used
    DeviceSessionRegistry.cs    serial → session; chat → serial; idle reaper
  Scrcpy/
    ScrcpyServerLauncher.cs     push, forward, app_process, socket handshake
    VideoDemuxer.cs             codec id, session packets, media packets, config merge
    ControlMessages.cs          serializers (pure, unit-tested)
    DeviceMessageDrain.cs       reads & discards device→client messages
  Video/
    FfmpegNative.cs             NativeLibrary loading + function pointers
    H264Decoder.cs              send_packet / receive_frame → YuvFrame
    YuvFrame.cs                 planes + width/height + sequence
    LatestFrame.cs              atomic slot + waiters + stability signature
    PngEncoder.cs               RGB24 → PNG (ZLibStream, CRC32)
    YuvToRgb.cs                 BT.601 limited range
  Gestures/
    Gesture.cs                  pointers × path points (x, y, t_ms)
    GestureBuilders.cs          tap, long press, swipe, pinch
    GesturePlayer.cs            plays a Gesture over a backend
  Ui/
    UiAutomatorDump.cs          dump → parse → compact list with refs
  Tools/                        one file per tool, AndroidToolBase.cs for shared helpers
  web/                          settings panel bundle (copy of ssh's structure)
```

## adb commands used

All run as `adb [-s SERIAL] ...` through `AdbRunner`.

| Purpose | Arguments | Notes |
|---|---|---|
| version | `version` | first line `Android Debug Bridge version 1.0.41`, second `Version 37.0.0-...` |
| devices | `devices -l` | skip header line; `SERIAL<ws>STATE key:value...`; states: `device`, `unauthorized`, `offline`, `no permissions` |
| tcp connect | `connect HOST:PORT` | output contains `connected to` or `already connected` on success |
| screenshot (fallback) | `exec-out screencap -p` | **binary** stdout = PNG. Never read as text |
| display size | `shell wm size` | `Physical size: 1080x2400` (+ optional `Override size:` — prefer override) |
| tap | `shell input tap X Y` | adb backend only |
| swipe / long press | `shell input swipe X1 Y1 X2 Y2 MS` | long press = same point, MS = hold |
| key | `shell input keyevent CODE` | |
| text (ASCII) | `shell input text ESCAPED` | space → `%s`; escape `\ ' " ( ) & < > ; | * $ ~ ?` with backslash; non-ASCII → refuse in adb mode |
| ui dump | `shell uiautomator dump /data/local/tmp/spla_ui.xml` then `exec-out cat /data/local/tmp/spla_ui.xml` then `shell rm -f /data/local/tmp/spla_ui.xml` | dump may fail with `could not get idle state` on animations → retry once after 500 ms |
| apps | `shell pm list packages -3` (user) / `shell pm list packages` (all) | lines `package:com.x.y` |
| start app | `shell monkey -p PKG -c android.intent.category.LAUNCHER 1` | |
| stop app | `shell am force-stop PKG` | |
| install | `install -r [-g] HOSTPATH` | success line `Success`; else `Failure [INSTALL_...]` |
| push / pull | `push HOST REMOTE` / `pull REMOTE HOST` | |
| pid | `shell pidof PKG` | empty = not running |
| logcat | `logcat -d -t N [--pid=PID] [*:LEVEL]` | `-d` = dump and exit |
| forward | `forward tcp:PORT localabstract:NAME` / `forward --remove tcp:PORT` | |
| push server | `push SCRCPY_SERVER_FILE /data/local/tmp/scrcpy-server.jar` | |

## scrcpy protocol (pinned: v4.1)

Source of truth: `https://github.com/Genymobile/scrcpy/tree/v4.1` — `doc/develop.md`,
`app/src/demuxer.c`, `app/src/control_msg.{h,c}`, `server/.../Options.java`,
`server/.../device/DesktopConnection.java`. **All integers are big-endian.**

### Launch (forward tunnel)

1. `scid` = random int in `[0, 0x7FFFFFFF]`, formatted `x8` (8 lowercase hex digits). The server parses
   `scid` as **hex**.
2. Socket name: `scrcpy_` + scid hex, e.g. `scrcpy_0a1b2c3d`.
3. `adb -s S push <scrcpy-server> /data/local/tmp/scrcpy-server.jar`
4. Pick a free local TCP port (bind `127.0.0.1:0`, read port, close).
5. `adb -s S forward tcp:PORT localabstract:scrcpy_<scid>`
6. Start a **long-running** process (keep it; its stdout/stderr go to a ring buffer for error messages):

   ```
   adb -s S shell CLASSPATH=/data/local/tmp/scrcpy-server.jar app_process / com.genymobile.scrcpy.Server
       <VERSION> scid=<scid> log_level=info tunnel_forward=true audio=false video=true control=true
       video_codec=h264 max_size=<max_size> max_fps=<max_fps> video_bit_rate=<bit_rate>
       stay_awake=<true|false> cleanup=true power_off_on_close=false clipboard_autosync=false
   ```

   Each token is a separate `ArgumentList` entry. `<VERSION>` is the version from the scrcpy
   **receipt** (e.g. `4.1`) — the server throws if it is not exactly its own version.
7. Connect **video** socket: TCP to `127.0.0.1:PORT`, then read exactly **1 dummy byte**. The adb forward
   accepts immediately even when the server is not up yet, so EOF / reset here means "not ready":
   close, wait 100 ms, retry (max 100 attempts ≈ 10 s). If the server process has exited — fail with
   its captured output.
8. Connect **control** socket: second TCP connection to the same port. No dummy byte on it.
9. Read **device meta** from the video socket: **64 bytes**, UTF-8, NUL-padded → device name.
10. Read **codec id** (u32) from the video socket. `0x68323634` = `"h264"` expected. `0` = video disabled
    on device, `1` = server-side configuration error → fail.
11. Teardown (any order, all best-effort): close sockets, kill the adb shell process,
    `adb forward --remove tcp:PORT`.

### Video packets (after codec id)

Every packet starts with a 12-byte header.

- `header[0] & 0x80` set → **session packet**: bytes 4..7 = width (u32), 8..11 = height (u32). Bit 0 of
  byte 3 = "client-resized" flag (ignore). No payload. Emitted at start and on every capture restart
  (rotation!) — update the backend's `ScreenSize`.
- Otherwise **media packet**: bytes 0..7 = u64 `ptsFlags`, bytes 8..11 = payload size (u32), then payload.
  - `CONFIG = 1UL << 62` — codec config (SPS/PPS). **Do not decode alone**: keep it and prepend it to the
    next non-config packet (this is scrcpy's "packet merger").
  - `KEY_FRAME = 1UL << 61`.
  - `PTS = ptsFlags & ((1UL << 61) - 1)`.

### Control messages (client → device, control socket)

Type byte values (enum order): `0 INJECT_KEYCODE, 1 INJECT_TEXT, 2 INJECT_TOUCH_EVENT, 3 INJECT_SCROLL_EVENT,
4 BACK_OR_SCREEN_ON, 5 EXPAND_NOTIFICATION_PANEL, 6 EXPAND_SETTINGS_PANEL, 7 COLLAPSE_PANELS,
8 GET_CLIPBOARD, 9 SET_CLIPBOARD, 10 SET_DISPLAY_POWER, 11 ROTATE_DEVICE, ... 16 START_APP, 17 RESET_VIDEO`.
We use 0, 1, 2, 9.

**INJECT_KEYCODE — 14 bytes**

| off | size | field |
|---|---|---|
| 0 | 1 | type = 0 |
| 1 | 1 | action: 0 = DOWN, 1 = UP |
| 2 | 4 | Android keycode |
| 6 | 4 | repeat (0) |
| 10 | 4 | metastate (0) |

A key press = DOWN then UP. Long press = DOWN, wait, UP.

**INJECT_TEXT — 5 + n bytes**: type = 1, u32 length, UTF-8 bytes. Max **300** bytes per message →
split longer text on UTF-8 boundaries. The server injects via key events: reliable for ASCII only.

**INJECT_TOUCH_EVENT — 32 bytes**

| off | size | field |
|---|---|---|
| 0 | 1 | type = 2 |
| 1 | 1 | action: 0 = DOWN, 1 = UP, 2 = MOVE |
| 2 | 8 | pointer id (i64). Use 0, 1, 2… per finger. Do **not** use −1/−2/−3 (mouse / generic / virtual) |
| 10 | 4 | x (i32) |
| 14 | 4 | y (i32) |
| 18 | 2 | screen width (u16) — **must equal current video width** or the server ignores the event |
| 20 | 2 | screen height (u16) — same rule |
| 22 | 2 | pressure (u16 fixed point: 1.0 → `0xFFFF`, 0.0 → 0). DOWN/MOVE: `0xFFFF`; UP: 0 |
| 24 | 4 | action button (0 for touch) |
| 28 | 4 | buttons (0 for touch) |

Always send plain DOWN/UP per pointer id; the server itself turns a second DOWN into
`ACTION_POINTER_DOWN`.

**SET_CLIPBOARD — 14 + n bytes**: type = 9, u64 sequence (0 = no ack wanted), 1 byte paste (1 = paste after
set), u32 length, UTF-8 text. Used for non-ASCII text entry. It **overwrites the phone clipboard**.

### Device messages (device → client, control socket)

The device writes clipboard / ack / uhid messages back on the control socket. We never use them, but
the socket must be **drained continuously** (a background loop reading into a discard buffer) or the
device side can block. Exit the loop on EOF.

## FFmpeg interop

Libraries come from the scrcpy archive (`avutil-*.dll`, `avcodec-*.dll`, plus whatever else that archive
ships). Resolve names by glob in the scrcpy folder — the major number differs per scrcpy release (4.1 →
FFmpeg 8.1: `avutil-60.dll`, `avcodec-62.dll`; verify on disk). Load **avutil first, then the rest,
avcodec last**, each by absolute path with `NativeLibrary.Load(path)`, so avcodec's import of avutil
resolves to the already-loaded module. Get functions with `NativeLibrary.GetExport` and call through
`delegate* unmanaged[Cdecl]<...>`.

Functions: `avcodec_version`, `avcodec_find_decoder(int id)` (H.264 = **27**), `avcodec_alloc_context3`,
`avcodec_open2(ctx, codec, null)`, `av_packet_alloc`, `av_new_packet(pkt, size)`, `av_packet_unref`,
`av_packet_free(&pkt)`, `av_frame_alloc`, `av_frame_unref`, `av_frame_free(&frame)`,
`avcodec_send_packet`, `avcodec_receive_frame`, `avcodec_free_context(&ctx)`.

Return codes: `0` ok; `AVERROR(EAGAIN)` = **−11** (need more input); `AVERROR_EOF` = `-0x20464F45`.

Struct fields read/written by offset (x64; stable across FFmpeg 5–8; re-verify against
`libavutil/frame.h` and `libavcodec/packet.h` of the shipped version when bumping):

| struct | field | offset | type |
|---|---|---|---|
| AVPacket | data | 24 | `byte*` |
| AVPacket | size | 32 | `int` |
| AVPacket | flags | 40 | `int` (`AV_PKT_FLAG_KEY = 1`) |
| AVFrame | data[0..2] | 0, 8, 16 | `byte*` |
| AVFrame | linesize[0..2] | 64, 68, 72 | `int` |
| AVFrame | width | 104 | `int` |
| AVFrame | height | 108 | `int` |
| AVFrame | format | 116 | `int` (`YUV420P = 0`, `YUVJ420P = 12`, `NV12 = 23`) |

Fill a packet with `av_new_packet(pkt, n)` (it allocates with the required padding), then copy the
payload into `pkt->data`. Supported output formats: YUV420P / YUVJ420P (three planes) and NV12 (Y + UV
interleaved). Anything else → decoder error "unsupported pixel format N".

Copy planes into managed arrays before `av_frame_unref` — `YuvFrame` must not point into FFmpeg memory.

## LatestFrame and stability

- `LatestFrame` holds the newest `YuvFrame` (swap with `Interlocked.Exchange`), a monotonically
  increasing `Sequence`, and lets callers await "a frame with sequence > N".
- **Stability signature:** 32×32 grid of mean luma sampled from the Y plane. Two signatures are "equal"
  when every cell differs by ≤ 2. scrcpy's encoder repeats the previous frame while the screen is idle,
  so "no new frames" is **not** a stability signal — only content comparison is.
- `WaitStableAsync(settleMs, timeoutMs)`: stable = signature unchanged for `settleMs`. On timeout return
  the latest frame and report `settled: false` (a blinking cursor or video never settles).

## PNG

RGB24, 8-bit, colour type 2, no interlace. Each row is prefixed with filter byte **1 (Sub)**. IDAT = the
whole filtered image through `ZLibStream` (`CompressionLevel.Fastest`). CRC32 over chunk type + data
(standard table, polynomial `0xEDB88320`). YUV→RGB: BT.601 limited range, integer math:

```
c = Y - 16; d = U - 128; e = V - 128
R = clamp((298*c + 409*e + 128) >> 8)
G = clamp((298*c - 100*d - 208*e + 128) >> 8)
B = clamp((298*c + 516*d + 128) >> 8)
```

## Android keycodes used by `android_key`

`home 3, back 4, call 5, endcall 6, dpad_up 19, dpad_down 20, dpad_left 21, dpad_right 22,
dpad_center 23, volume_up 24, volume_down 25, power 26, camera 27, tab 61, space 62, enter 66, del 67
(backspace), menu 82, search 84, media_play_pause 85, page_up 92, page_down 93, escape 111,
forward_del 112, move_home 122, move_end 123, app_switch 187 (recents), wakeup 224, sleep 223`.
A raw integer `keycode` is also accepted.
