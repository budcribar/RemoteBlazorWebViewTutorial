The steps to convert a WPF Blazor application to a Remote WPF Blazor application

- Change the Microsoft.AspNetCore.Components.WebView.Wpf package reference to PeakSWC.RemoteBlazorWebView.Wpf in the WpfBlazor.csproj.
   
   Change the reference from Microsoft
   ```
   <PackageReference Include="Microsoft.AspNetCore.Components.WebView.Wpf" Version="7.0.59" />
   ```
   To PeakSWC
   ```
   <PackageReference Include="PeakSWC.RemoteBlazorWebView.Wpf" Version="7.0.3" />
   ```
- Modify the MainWindow.xaml file

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
  <blazor:BlazorWebView x:Name="RemoteBlazorWebView" HostPage="wwwroot\index.html" Services="{DynamicResource services}">
  ```

  To
  ```
  <blazor:BlazorWebView x:Name="RemoteBlazorWebView" HostPage="wwwroot\index.html" Services="{DynamicResource services}">
  ```

- Modify the MainWindow.xaml.cs file. Set the Id to Empty so a new Id is generated each time, Add event handler to display a URI when we are ready to connect and finally set the Server Uri

   Change:
   ```
   serviceCollection.AddWpfBlazorWebView();
   ```
   To:
   ```
      serviceCollection.AddRemoteWpfBlazorWebView();
     
   ```
   Add the following using statement to MainWindow.xaml.cs to import the required namespaces.
   ```
   using PeakSWC.RemoteBlazorWebView;
   using PeakSWC.RemoteWebView;
   ```

At this point your WPF application should run normally. In order to be able to run your application remotely, follow the following steps

- Make the following modifications after the  serviceCollection.AddRemoteWpfBlazorWebView(); statement in MainWindow.xaml.cs file. Set the Id to Empty so a new Id is generated each time, Add event handler to display a URI when we are ready to connect and finally set the Server Uri

```
      RemoteBlazorWebView.Id = Guid.Empty;
      RemoteBlazorWebView.ReadyToConnect += Rbwv_ReadyToConnect;
      RemoteBlazorWebView.ServerUri = new Uri( @"https://localhost:5001");
   }

    private void Rbwv_ReadyToConnect(object? sender, ReadyToConnectEventArgs e)
    {
        (sender as IBlazorWebView)?.NavigateToString($"<a href='{e.Url}app/{e.Id}' target='_blank'> {e.Url}app/{e.Id}</a>");
    }
```
<!--
- Install the RemoteWebViewService
```console
dotnet tool update -g PeakSWC.RemoteWebViewService --version 7.*-* 
```
-->

- Start the server in the background
```console
RemoteWebViewService &
```


- Run the WPF Application. There will be a hyperlink showing that when clicked, will show the UI
