<h1>Converting a WPF Blazor Application to a Remote WPF Blazor Application</h1>

- Modify the `WpfBlazor.csproj` file.
   
   Change the reference from Microsoft
   ```
   <PackageReference Include="Microsoft.AspNetCore.Components.WebView.Wpf" Version="7.0.59" />
   ```
   To PeakSWC
   ```
   <PackageReference Include="PeakSWC.RemoteBlazorWebView.Wpf" Version="7.0.3" />
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

- Modify the `MainWindow.xaml.cs` file. Add the AddRemoteWpfBlazorWebView service, Set the Id to Empty so a new Id is generated each time, add the ReadyToConnect handler,  and finally set the Server Uri

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


- Install the RemoteWebViewService
```console
dotnet tool update -g PeakSWC.RemoteWebViewService --version 7.*-* 
```

- Start the server in the background
```console
RemoteWebViewService &
```

- Run the WPF Application. A hyperlink will be displayed, and when clicked, it will reveal the user interface.

<h2>Advanced Features</h2>

<h3>Mirroring</h3>

A read-only version of the user interface can be "mirrored" in the WPF Application, enabling a user to monitor the application while another person operates it remotely. To set this up, handle the Connected event and navigate to a mirror URL within the BlazorWebView control.

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
        <blazor:BlazorWebView x:Name="RemoteBlazorWebView" HostPage="wwwroot\index.html" Services="{DynamicResource services}">
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

A browser refresh typically resets a web server and re-loads the home page. To achieve equivalent functionality when refreshing an application with an embedded web browser control, you can restart the application. To do this, add the following event handler:

```
RemoteBlazorWebView.Refreshed += (_, _) =>
{
    RemoteBlazorWebView.Restart();
    Close();
};
```

<h3>Handling Browser Disconnect</h3>

When a user disconnects from the application, a Disconnect event is triggered. Various circumstances can cause a disconnect, such as closing the browser or tab, or navigating away from the application. To gracefully shut down the application upon user disconnection, add the following event handler:
```csharp
 RemoteBlazorWebView.Disconnected += (_,_) => Application.Current.Shutdown();
```

This event handler ensures that the application is properly closed when the user disconnects, maintaining a smooth user experience.
