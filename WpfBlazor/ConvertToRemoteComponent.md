# Moving a Blazor Component into Its Own Process

A remote component is a Blazor component that runs in a separate executable and shows up inside your app, in a sandboxed iframe.
The app passes it values and handles its callbacks, much as it would for a component in its own page. This guide covers when that
is worth doing, and how to convert a component.

It uses the `PeakSWC.RemoteBlazorWebView.RemoteComponents` package (the frame, the connection and a source generator) and the
`RemoteComponentHost` in `PeakSWC.RemoteBlazorWebView.Wpf`. The package README has the reference details.

## Why you would do it

A component in its own process is isolated from your app in ways an in-process component can't be:

- **It can run with fewer rights than the app.** The app might run elevated, or have access to files and accounts that the
  component's code should never reach. You decide how the process is started: as a normal user, with a restricted token, or in a
  folder of its own.
- **It can't touch the app's page.** The iframe is sandboxed without `allow-same-origin`, so it has an opaque origin. It can't read
  the app's DOM, storage or cookies, or another component's frame, and it can't navigate the app or open popups. The only message
  the app's page accepts from it is its content height.
- **A crash stays in its process.** An unhandled exception, a hang or a memory leak in the component ends or slows its process,
  not the app. The app shows an error in its place and can start it again.
- **It can ship separately.** The component's exe and its dependencies can be built, versioned and replaced on their own, for
  example by a third party, as long as the contract between the two sides doesn't change.
- **The data doesn't go through the browser.** Values and callbacks travel over the process's stdin and stdout. When the app is
  viewed remotely, the remote browser sees the component's page but can't forge what crosses between the app and the component.

Typical cases: UI that comes from code you don't fully trust (an extension, a customer script), UI that wraps a fragile native
library, and a feature that a separate team ships on its own schedule.

## Why you might not

- **Memory and start-up time.** Each component is a whole extra process: a .NET runtime, a WPF window that is never shown, and a
  WebView2 client with its own browser processes. Expect tens of megabytes per component at least, and a start-up of up to a
  second or two before the frame shows anything. Measure it on your target machines before deciding; for many small widgets this
  is the wrong tool.
- **Everything that crosses must be JSON.** Values and callback arguments are serialized (web defaults, enums as strings). You
  can't pass a `RenderFragment`, a service, an `EventCallback` with a non-serializable argument or a live object, and every change
  to a value sends it again.
- **Callbacks take at most one argument and return nothing.** A request that needs an answer is a callback plus a value the app
  sets in reply.
- **You need a relay.** The component's page is served by a RemoteWebViewService. Usually that is one embedded in the app
  (`PeakSWC.RemoteWebViewService.Lib`), which adds to the app's size.
- **Windows only, for now.** The host is the WPF control.

If you only want to keep a component from breaking the rest of the page, an `<ErrorBoundary>` in the same process is much cheaper.

## How to convert a component

The example moves a `Clock` component out of the app. The app sets its time zone and the next alarm; the component calls back when
the user snoozes it.

### 1. Put the contract in a library both sides reference

```csharp
using PeakSWC.RemoteBlazorWebView.RemoteComponents;

[RemoteComponentContract]
public interface IClock
{
    string TimeZone { get; }              // a value: app -> component
    Alarm? NextAlarm { get; }             // any JSON-serializable type
    Task Snooze();                        // a callback: component -> app
    Task Picked(DateTimeOffset when);     // at most one argument
}

public sealed record Alarm(DateTimeOffset When, string Label);
```

This library references `PeakSWC.RemoteBlazorWebView.RemoteComponents`, which brings the source generator. From the interface it
writes `ClockFrame` (the app's side) and `ClockReceiver` (the component's side).

The contract has to live in its own library. Razor compiles `.razor` files before source generators run, so a page can't use a
frame generated in its own project.

### 2. Create the component's exe

A WPF app (`<UseWPF>true</UseWPF>`, the Razor SDK) that references `PeakSWC.RemoteBlazorWebView.Wpf`, the contract library and the
component itself:

```csharp
public static class Program
{
    [STAThread]
    public static int Main(string[] args) => RemoteComponentHost.Run(new RemoteComponentHostOptions
    {
        ServerUri = new Uri(args[0]),
        GrpcBaseUri = new Uri(args[1]),
        Id = Guid.Parse(args[2]),
        ParentProcessId = int.Parse(args[3]),   // exit when the app does
        RootComponent = typeof(ClockRoot),
        ConfigureServices = (services, connection) => services.AddRemoteComponentReceiver<ClockReceiver>(connection),
    });
}
```

`ClockRoot` injects the contract (or `ClockReceiver`, which also has `Changed` and `HasValues`) and passes the values on:

```razor
@inject ClockReceiver Clock
@implements IDisposable

<Clock TimeZone="@Clock.TimeZone" NextAlarm="@Clock.NextAlarm" OnSnooze="() => Clock.Snooze()" />

@code {
    protected override void OnInitialized() => Clock.Changed += OnChanged;
    private void OnChanged() => InvokeAsync(StateHasChanged);   // raised on the reading thread
    public void Dispose() => Clock.Changed -= OnChanged;
}
```

Its `wwwroot\index.html` is an ordinary Blazor host page plus the height script, so the app can size the frame to its content:

```html
<div id="app">Loading…</div>
<script>
(function () {
    var last = 0;
    function post() {
        var h = Math.ceil(document.documentElement.scrollHeight);
        if (h !== last) { last = h; window.parent.postMessage({ type: 'rbwv-remote-height', height: h }, '*'); }
    }
    if (window.ResizeObserver) new ResizeObserver(post).observe(document.documentElement);
    window.addEventListener('load', post);
})();
</script>
<script src="_framework/blazor.webview.js"></script>
```

The process exits when the frame is closed or reloaded (the app starts a fresh one each time), when the relay goes away, or when
the app exits.

### 3. Start a relay in the app

```csharp
var relay = await RemoteWebViewServer.StartAsync(new RemoteWebViewServerOptions
{
    FrameAncestors = ["https://0.0.0.1"],   // the origin of a BlazorWebView page on .NET 8 and later
    AllowOpaqueOrigin = true,              // the frame is sandboxed without allow-same-origin
});
```

`FrameAncestors` lets only your app's page frame the relay's pages. Start one relay for all components and stop it when the app
exits.

### 4. Start the process and show the frame

The frame takes an `Open` function that starts the process and returns a `RemoteComponentSession`: the address to frame, the
connection over the process's stdin and stdout, and what to do when the frame goes away.

```csharp
public async Task<RemoteComponentSession> StartClockAsync(CancellationToken ct)
{
    var id = Guid.NewGuid();
    var process = Process.Start(new ProcessStartInfo("Clock.exe",
        $"{relay.BaseUri} {relay.GrpcUri} {id} {Environment.ProcessId}")
    {
        RedirectStandardInput = true, RedirectStandardOutput = true, UseShellExecute = false,
    })!;
    var connection = new RemoteComponentConnection(process.StandardOutput, process.StandardInput);
    return new RemoteComponentSession(new Uri(relay.BaseUri, id.ToString()), connection, () =>
    {
        connection.Dispose();
        if (!process.HasExited) process.Kill();
        return ValueTask.CompletedTask;
    });
}
```

This is where you decide how the component runs: as another user, with a restricted token (start it with `CreateProcessAsUser`
and pipes of your own), or from another folder. Whatever you choose, the component's stdin and stdout must be the two ends of
`RemoteComponentConnection`.

Then use the generated frame where the component used to be:

```razor
<ClockFrame Open="StartClockAsync" TimeZone="@zone" NextAlarm="@alarm" Snooze="OnSnooze" Title="Clock" />
```

The frame starts the process when it first renders, sends all the values once, then only the values that changed (compared as
JSON) on each render. It raises `Snooze` and `Picked` as ordinary `EventCallback`s. Extra attributes such as `class` go on the
iframe. If `Open` throws, the frame shows `ProblemContent` (by default a short paragraph with the error) instead.

### 5. Check it

- The frame loads and its height follows the content.
- Changing `zone` or `alarm` in the app updates the component; snoozing in the component reaches the app.
- Closing the page that holds the frame ends the component's process, and so does closing the app.
- If the component's process runs with fewer rights, check that it can still write its WebView2 data folder
  (`RemoteComponentHostOptions.WebViewDataFolder`), since the default is next to the exe.
