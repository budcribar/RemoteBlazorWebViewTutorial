<h1>Converting a WPF Blazor Application to a Remote WPF Blazor Application</h1>

- Modify the `WpfBlazor.csproj` file.
   
   Change the reference from Microsoft
   ```
   <PackageReference Include="Microsoft.AspNetCore.Components.WebView.Wpf" Version="11.0.10" />
   ```
   To PeakSWC
   ```
   <PackageReference Include="PeakSWC.RemoteBlazorWebView.Wpf" Version="11.0.0" />
   ```
- Modify the `MainWindow.xaml` file

  Change 
  ```
  xmlns:blazor="clr-namespace:Microsoft.AspNetCore.Components.WebView.Wpf;assembly=Microsoft.AspNetCore.Components.WebView.Wpf"
  ```
  To
  ```
  xmlns:blazor="clr-namespace:PeakSWC.RemoteBlazorWebView.Wpf;assembly=PeakSWC.RemoteBlazorWebView.Wpf"
  ```

  Change
  ```
  <blazor:BlazorWebView x:Name="MicrosoftBlazorWebView" HostPage="wwwroot\index.html" Services="{DynamicResource services}">
  ```

  To
  ```
  <blazor:BlazorWebView x:Name="RemoteBlazorWebView" HostPage="wwwroot\index.html" Services="{DynamicResource services}">
  ```

- Modify the `MainWindow.xaml.cs` file. Mark the window with `[RemoteBlazor]`, make it `partial` (it already is), switch to the AddRemoteWpfBlazorWebView service, and call the generated `InitializeRemoteBlazor()`

   The `[RemoteBlazor]` source generator comes with the PeakSWC package. It writes the code that sets the Id, the Server Uri and the ReadyToConnect link handler for you.

   Change:
   ```
   public partial class MainWindow : Window
   {
       public MainWindow()
       {
           InitializeComponent();
           var serviceCollection = new ServiceCollection();
           serviceCollection.AddWpfBlazorWebView();
           Resources.Add("services", serviceCollection.BuildServiceProvider());
       }
   }
   ```
   To:
   ```
   [RemoteBlazor(nameof(RemoteBlazorWebView), Mode = RemoteBlazorMode.Remote, ServerUri = "https://localhost:5001")]
   public partial class MainWindow : Window
   {
       public MainWindow()
       {
           InitializeComponent();
           var serviceCollection = new ServiceCollection();
           serviceCollection.AddRemoteWpfBlazorWebView();
           Resources.Add("services", serviceCollection.BuildServiceProvider());
           InitializeRemoteBlazor();
       }
   }
   ```
   Add the following using statement to MainWindow.xaml.cs to import the required namespace.
   ```
   using PeakSWC.RemoteBlazorWebView;
   ```

   That's the whole change. `-u=<server uri>` and `-i=<guid>` on the command line override `ServerUri` and the id. Other modes:

   | You want | Use |
   |---|---|
   | Remote only when started with `-u=<server uri>`, local otherwise | `[RemoteBlazor(nameof(RemoteBlazorWebView))]` (the default mode, `Runtime`) |
   | Always remote | `Mode = RemoteBlazorMode.Remote, ServerUri = "..."` |
   | Back to local without touching the code | `dotnet build -p:RemoteBlazorMode=Local`, or `<RemoteBlazorMode>Local</RemoteBlazorMode>` in `WpfBlazor.csproj` |

   The finished project is in [WpfBlazorRemote](../WpfBlazorRemote); it also has the next step, [Sharing Control](#sharing-control), switched on. The generator's options and error codes are in its
   [README](https://github.com/budcribar/RemoteBlazorWebView/blob/net11/src/RemoteBlazorWebView.SourceGenerators/README.md).

   <details>
   <summary>The same thing by hand, without the generator</summary>

   In `MainWindow.xaml.cs`, add the AddRemoteWpfBlazorWebView service, set the Id to Empty so a new Id is generated each time, add the ReadyToConnect handler,  and finally set the Server Uri

   Change:
   ```
   serviceCollection.AddWpfBlazorWebView();
   Resources.Add("services", serviceCollection.BuildServiceProvider());
   ```
   To:
   ```
   serviceCollection.AddRemoteWpfBlazorWebView();
   Resources.Add("services", serviceCollection.BuildServiceProvider());
   RemoteBlazorWebView.Id = Guid.Empty;
   RemoteBlazorWebView.ReadyToConnect += Rbwv_ReadyToConnect;
   RemoteBlazorWebView.ServerUri = new Uri( @"https://localhost:5001");
     
   ```
   Add the following using statement to MainWindow.xaml.cs to import the required namespaces.
   ```
   using PeakSWC.RemoteBlazorWebView;
   using PeakSWC.RemoteWebView;
   ```

   Add event handler after the MainWindow constructor to display a URI when we are ready to connect. 
   ```
    private void Rbwv_ReadyToConnect(object? sender, ReadyToConnectEventArgs e)
    {
        (sender as IBlazorWebView)?.NavigateToString($"<a href='{e.Url}app/{e.Id}' target='_blank'> {e.Url}app/{e.Id}</a>");
    }
   ```

   </details>


- Install the RemoteWebViewService
```console
dotnet tool update -g PeakSWC.RemoteWebViewService --version 10.*-* 
```

- Start the server in the background
```console
RemoteWebViewService &
```

- Run the WPF Application. A hyperlink will be displayed, and when clicked, it will reveal the user interface.

That is the simplest remote app: it runs only in the browser, and the window shows the link. The next section is the recommended way to use it once that works.

<h2 id="sharing-control">Sharing Control</h2>

The person at the PC and a remote user use the same app and hand control back and forth (packages and server 11.0.0 or later). This is the recommended way to run a remote app.

- The app's own window shows the app, with the person at the PC (the owner) in control.
- A remote user opens the session's link in a browser and sees the same app, read-only at first.
- The owner gives control to the remote user and can take it back at any time. Whoever does not have control sees the app but cannot use it.
- A session has one owner, one remote user (the first to open the link) and any number of read-only viewers on the same link.

With the generator, add `ShareControl = true` to `[RemoteBlazor]`:

```
[RemoteBlazor(nameof(RemoteBlazorWebView), Mode = RemoteBlazorMode.Remote, ServerUri = "https://localhost:5001",
    ShareControl = true, RestartOnRefresh = true)]
public partial class MainWindow : Window
```

<h3>The session bar</h3>

The session bar is where control is handed over: a dot that is lit while anyone is connected remotely, who has control, Give control / Take control, and Copy link for the owner. It is opt-in. RemoteBlazorWebView draws nothing on your pages unless you ask, and you choose one of three ways.

**1. No bar.** Leave the placeholder out. Nothing is drawn. With `ShareControl` there are then no buttons to hand control over, so this suits an unattended session (below), or an app that draws its own controls from `window.RemoteControl`.

**2. The built-in bar.** Put an empty element where the bar should sit. Each browser draws its own bar over it.

```
 <div id="rbwv-session-controls" data-share-link="true" data-restart="true"></div>
```

| On the placeholder | What it does |
|---|---|
| `data-share-link="true"` | Shows Copy link (owner only), which copies the link the remote user joins at (`<server>/mirror/<id>`) |
| `data-restart="true"` | Shows Restart app for whoever has control, and for the owner at any time. See [Restarting the App](#restarting-the-app) |
| `data-host-name`, `data-remote-name` | What to call the two parties in "... has control". The defaults are "The host" and "Remote user" |

- Add the placeholder above the `<h1>` in `Counter.razor`, run the app and press Copy link. Open the link in a browser: that is the remote user. Press Give control in the app window, and Take control to get it back.

The tutorial's other sample apps do this in their layout (`RemoteBlazorWebViewTutorial.Shared/Shared/MainLayout.razor`), which is where it belongs in an app with more than one page: a placeholder on one page goes away when the user navigates.

**3. Your own bar.** Put a `<template>` inside the placeholder and mark its elements with `data-rbwv`. Each browser gets its own copy of the template with only the parts that apply to it: the status is filled in and the buttons are wired. The layout, the wording, the classes and anything else in the template are yours.

```
 <div id="rbwv-session-controls" class="session-bar-slot" data-remote-name="The helper">
     <template>
         <span data-rbwv="dot"></span>
         <strong data-rbwv="status" data-text-you="You are in control" data-text-view-only="Watching"></strong>
         <button class="session-btn" data-rbwv="give">Hand over</button>
         <button class="session-btn session-btn-take" data-rbwv="take">Take back</button>
         <button class="session-btn" data-rbwv="copy-link" data-copied-text="Link copied">Invite someone</button>
         <button class="session-btn" data-rbwv="restart">Restart</button>
         <span data-rbwv="restart-question">Restart the app for everyone?</span>
         <button class="session-btn session-btn-take" data-rbwv="restart-confirm">Yes, restart</button>
         <button class="session-btn" data-rbwv="restart-cancel">No</button>
     </template>
 </div>
```

| `data-rbwv` | Shown to |
|---|---|
| `dot` | Everyone. Gets the class `rbwv-control-dot-on` while anyone is connected remotely |
| `status` | Everyone. The text is filled in: "You have control", "Remote user has control", "View only"... `data-text-you`, `data-text-view-only`, `data-text-none`, `data-text-switching` and `data-text-restarting` replace the wording |
| `give` | Whoever has control, when there is someone to give it to |
| `take` | The owner, when someone else has control |
| `copy-link` | The owner. `data-copied-text` is shown for a moment after the link is copied |
| `restart` | Whoever has control, and the owner. With `restart-question`, `restart-confirm` and `restart-cancel` in the template the user is asked first; without them Restart restarts at once |
| `voice-status`, `voice-act`, `voice-mute`, `voice-end` | The voice call's status and buttons (their wording follows the call) |

- Replace the placeholder in `Counter.razor` with the template above and add the `.session-bar-slot`, `.rbwv-control-custom` and `.session-btn` rules from `WpfBlazorRemote/wwwroot/css/app.css` to your `app.css`. Run the app: the bar is now a dark pill with your wording. This is what `WpfBlazorRemote` contains.

The bar is drawn by a script in each browser, not by Blazor: the app renders once and every browser shows that same render, so a bar made of Blazor components would say the same thing to everyone and could not be clicked by whoever does not have control. For anything a template cannot do, `window.RemoteControl` gives a script of your own the same state and actions.

With a placeholder, the bar still only shows when there is something to control: the app shares control or uses voice.

<h3>Unattended sessions</h3>

When nobody is at the PC to hand control over, the remote user can be given control as soon as they join. The owner can still take it back. Use `Unattended = true` instead of `ShareControl = true`, or keep `ShareControl = true` and start the app with `--unattended` to choose it for one run.

<h3>Voice</h3>

A voice call between the person at the PC and the remote user, with its buttons in the session bar. It needs the WPF control and an https server. Add `Voice = true` to `[RemoteBlazor]`, or start the app with `--voice`.

<h3>By hand</h3>

Without the generator, replace the ReadyToConnect handler so the window opens the app's own page as the owner, and enable mirrors:

```
 RemoteBlazorWebView.EnableMirrors = true;
 RemoteBlazorWebView.EnableVoice = true;   // optional
 RemoteBlazorWebView.ReadyToConnect += (_, e) =>
     RemoteBlazorWebView.NavigateToUrl(new Uri($"{e.Url}app/{e.Id}?role=owner"));
```

For an unattended session use `?role=owner&control=remote`.

<h2 id="restarting-the-app">Restarting the App</h2>

A remote app is restarted from the browser in two ways: Restart app in the session bar, or reloading the app's own page (a browser refresh). Either way the server tells the app, the app starts a fresh copy of itself on the same server and session id, and the old copy closes. Browser pages wait for the new copy and load again by themselves.

With the generator, add `RestartOnRefresh = true` to `[RemoteBlazor]`. The generated code:

- starts the new copy with this run's own command line, so switches such as `--voice` survive the restart;
- uses the session id the server assigned, so the link keeps working;
- still restarts when the window is not responding. The restart request is normally handled on the UI thread; if that thread has not picked it up within 5 seconds, the new copy is started from a background thread and the stuck process is ended.

By hand, add the following event handler. `Restart()` starts a copy with only `-u=` and `-i=`.

```
RemoteBlazorWebView.Refreshed += (_, _) =>
{
    RemoteBlazorWebView.Restart();
    Close();
};
```

To cover a window that is not responding as well, set `RestartWhenUnresponsive`. It is called on a background thread, so it must not touch the window:

```
var server = RemoteBlazorWebView.ServerUri;
var id = RemoteBlazorWebView.Id;
RemoteBlazorWebView.ReadyToConnect += (_, e) => id = e.Id;
RemoteBlazorWebView.RestartWhenUnresponsive = () =>
{
    Process.Start(new ProcessStartInfo(Environment.ProcessPath!) { ArgumentList = { $"-u={server}", $"-i={id}" } });
    Environment.Exit(0);
};
```

<h2>Settings From Your Own Configuration</h2>

The generated code reads `-u=`, `-i=`, `--unattended` and `--voice` from the command line. An app that works its settings out from more than that (environment variables, a settings file, saved state) passes what it found, and anything left out still comes from the command line and the attribute:

```
InitializeRemoteBlazor(new RemoteBlazorStartup
{
    ServerUri = settings.Server,
    Id = settings.SessionId,
    Unattended = settings.Unattended,
    Voice = settings.Voice,
    RestartArguments = (server, id) => settings.ArgumentsFor(server, id),   // the command line of the restarted copy
});
```

<h2>Other Modes</h2>

Two older, one-way arrangements are still supported. In both, only one side can use the app.

<h3>Mirroring</h3>

A read-only version of the user interface can be "mirrored" in the WPF Application, enabling a user to monitor the application while another person operates it remotely. To set this up, handle the Connected event and navigate to a mirror URL within the BlazorWebView control.

With the generator, add `Mirror = true` to `[RemoteBlazor]`. By hand:

- Insert the following in the MainWindow.xaml.cs after the ReadyToConnect handler is set up.

```
 RemoteBlazorWebView.Connected += Rbwv_Connected;
 RemoteBlazorWebView.EnableMirrors = true;
```

- Now add the event handler

```
 private void Rbwv_Connected(object? sender, ConnectedEventArgs e)
        {
            RemoteBlazorWebView.WebView.CoreWebView2.Navigate($"{e.Url}mirror/{e.Id}");
            var user = e.User.Length > 0 ? $"by user {e.User}" : "";
            Title += $" Controlled remotely {user} from ip address {e.IpAddress}";
        }
```

<h3>Reversing The Mirror</h3>

This example demonstrates a read-only mirror accessible through a web browser, with the application being controlled locally. This scenario enables remote monitoring without the capacity for interaction with the application.

How it works: the app still runs remotely, with mirrors enabled. A plain WebView2 control sits over the BlazorWebView and opens the app's own page on the server (`<server>/app/<id>`), so the person at the PC drives the app through the server and it looks like a local app. Once that WebView2 connects, the app opens the system browser on the read-only mirror (`<server>/mirror/<id>`), which is the view you share.

**With the generator.** Add the WebView2 control to `MainWindow.xaml` (the generator can't add controls):

```
xmlns:webView2="clr-namespace:Microsoft.Web.WebView2.Wpf;assembly=Microsoft.Web.WebView2.Wpf"
```
```
<Grid>
    <blazor:BlazorWebView x:Name="RemoteBlazorWebView" HostPage="wwwroot\index.html" Services="{DynamicResource services}">
        ...
    </blazor:BlazorWebView>
    <webView2:WebView2 x:Name="WebView" />
</Grid>
```

Then name it in `[RemoteBlazor]`. The generator sets `EnableMirrors`, points the WebView2 at the app when the server is ready (instead of showing the link), and opens the mirror in the browser when it connects:

```
[RemoteBlazor(nameof(RemoteBlazorWebView), Mode = RemoteBlazorMode.Remote, ServerUri = "https://localhost:5001",
    ReverseMirror = nameof(WebView))]
public partial class MainWindow : Window
```

`ReverseMirror` can't be combined with `Mirror`. In WinForms, dock a `Microsoft.Web.WebView2.WinForms.WebView2` over the BlazorWebView and name that.

**By hand.**

- Modify MainWindow.xaml add the namespace and the WebView2 control. It should look like:

```
<Window x:Class="WpfBlazor.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:blazor="clr-namespace:PeakSWC.RemoteBlazorWebView.Wpf;assembly=PeakSWC.RemoteBlazorWebView.Wpf"
        xmlns:local="clr-namespace:WpfBlazor"
        xmlns:webView2="clr-namespace:Microsoft.Web.WebView2.Wpf;assembly=Microsoft.Web.WebView2.Wpf"
        mc:Ignorable="d"
        Title="MainWindow" Height="450" Width="800">
    <Grid>
        <blazor:BlazorWebView x:Name="RemoteBlazorWebView" HostPage="wwwroot\index.html" Services="{DynamicResource services} EnableMirrors="True">
                <blazor:BlazorWebView.RootComponents>
                <blazor:RootComponent Selector="#app" ComponentType="{x:Type local:Counter}" />
            </blazor:BlazorWebView.RootComponents>
        </blazor:BlazorWebView>
        <webView2:WebView2 x:Name="WebView" />
    </Grid>
</Window>
```

Modify the MainWindow.xaml.cs to look like

```
public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddRemoteWpfBlazorWebView();
            Resources.Add("services", serviceCollection.BuildServiceProvider());           
            RemoteBlazorWebView.Id = Guid.Empty;         
            RemoteBlazorWebView.ReadyToConnect += Rbwv_ReadyToConnect;
            RemoteBlazorWebView.ServerUri = new Uri(@"https://localhost:5001");
            RemoteBlazorWebView.Connected += Rbwv_Connected;
            RemoteBlazorWebView.EnableMirrors = true;
        }
        private void Rbwv_ReadyToConnect(object? sender, ReadyToConnectEventArgs e)
        {
            WebView.Source = new Uri($"{e.Url}app/{e.Id}");          
        }
        private void Rbwv_Connected(object? sender, ConnectedEventArgs e)
        {         
            Process.Start(new ProcessStartInfo { FileName = $"{e.Url}mirror/{e.Id}", UseShellExecute = true });
        }
    }
    
```

<h2>More</h2>

<h3>Handling Browser Disconnect</h3>

When a user disconnects from the application, a Disconnect event is triggered. Various circumstances can cause a disconnect, such as closing the browser or tab, or navigating away from the application. To gracefully shut down the application upon user disconnection, add `ShutdownOnDisconnect = true` to `[RemoteBlazor]`, or by hand add the following event handler:
With the generator, add `ShutdownOnDisconnect = true` to `[RemoteBlazor]`. By hand:
```csharp
 RemoteBlazorWebView.Disconnected += (_,_) => Application.Current.Shutdown();
```
This suits the browser-only arrangement, where the app has no user once the browser is gone. In a shared session the person at the PC is still using the app, so leave it off there.

This event handler ensures that the application is properly closed when the user disconnects, maintaining a smooth user experience.


<h3>Enabling Developer Tools</h3>

Add the BlazorWebViewDeveloperTools class to the serviceCollection in order to enable the developer tools.
```
serviceCollection.AddSingleton(new PeakSWC.RemoteBlazorWebView.Wpf.BlazorWebViewDeveloperTools { Enabled = true });
```

After enabling developer tools, you can access them in your Blazor WebView app by right-clicking on the WebView and selecting "Inspect" from the context menu, or by pressing Ctrl + Shift + I or F12 on your keyboard.
<h3>Building a Single File Executable</h3>

Follow the following steps to embed the wwwroot directory and it's contents in order to create a single file executable

Insert the following statement within the first PropertyGroup of the .csproj file. This variable needs to be set at build time so we cannot rely on the variable being set when publishing.

```
<PublishDir>bin\release\net11.0-windows\publish\win-x64</PublishDir>
```

Next add the following ItemGroup which will map all of the contents of the wwwroot directory into the Embedded Resources

```
<ItemGroup>
	<Parent Include="$(PublishDir)\wwwroot\**\*.*" />
	<EmbeddedResource Include="@(Parent)">
		<LogicalName>WpfBlazor.$([System.String]::Copy('%(RecursiveDir)').Replace('\', '.').Replace('/','.'))%(Filename)%(Extension)</LogicalName>
		<Link>$(PublishDir)\wwwroot\%(RecursiveDir)%(Filename)%(Extension)</Link>
		<Visible>true</Visible>
	</EmbeddedResource>
</ItemGroup>
```

Now you must publish your project twice. The first time you publish the wwwroot files will be collected and placed in the publish directory. However, they will not be embedded as resources since they have not been built before the project has been compiled.
After the second publish, the wwwroot files will be embedded and you can deploy your application without needing to deploy the wwwroot directory. 
