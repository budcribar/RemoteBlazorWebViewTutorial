<h1>Converting a WPF Blazor Application to a Remote WPF Blazor Application</h1>

- Modify the `WpfBlazor.csproj` file.
   
   Change the reference from Microsoft
   ```
   <PackageReference Include="Microsoft.AspNetCore.Components.WebView.Wpf" Version="10.0.110" />
   ```
   To PeakSWC
   ```
   <PackageReference Include="PeakSWC.RemoteBlazorWebView.Wpf" Version="10.0.14" />
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

   The finished project is in [WpfBlazorRemote](../WpfBlazorRemote). The generator's options and error codes are in its
   [README](https://github.com/budcribar/RemoteBlazorWebView/blob/net10/src/RemoteBlazorWebView.SourceGenerators/README.md).

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

<h2>Advanced Features</h2>

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

<h3>Handling Browser Refresh</h3>

A browser refresh typically resets a web server and re-loads the home page. To achieve equivalent functionality when refreshing an application with an embedded web browser control, you can restart the application. With the generator, add `RestartOnRefresh = true` to `[RemoteBlazor]`. By hand, add the following event handler:

```
RemoteBlazorWebView.Refreshed += (_, _) =>
{
    RemoteBlazorWebView.Restart();
    Close();
};
```

<h3>Handling Browser Disconnect</h3>

When a user disconnects from the application, a Disconnect event is triggered. Various circumstances can cause a disconnect, such as closing the browser or tab, or navigating away from the application. To gracefully shut down the application upon user disconnection, add `ShutdownOnDisconnect = true` to `[RemoteBlazor]`, or by hand add the following event handler:
```csharp
 RemoteBlazorWebView.Disconnected += (_,_) => Application.Current.Shutdown();
```

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
<PublishDir>bin\release\net7.0-windows\publish\win-x64</PublishDir>
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
