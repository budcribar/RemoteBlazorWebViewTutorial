# RemoteBlazorWebViewTutorial

Microsoft has recently introduced two Blazor WebView Controls in .NET 6. One control (Microsoft.AspNetCore.Components.WebView.Wpf) targets Windows Presentation Foundation (WPF) apps and
the other (Microsoft.AspNetCore.Components.WebView.WindowsForms) targets Windows Form (WinForms) apps. The Microsoft controls allow developers to create user interfaces for desktop apps using Blazor web technology. The desktop apps using Blazor have the full feature set and performance of .NET 6 and are not contrained like a Blazor WebAssembly app.

The Remote versions of the Blazor WebView Controls (PeakSWC.RemoteBlazorWebView.Wpf and PeakSWC.RemoteBlazorWebView.WindowsForms) act as a drop-in replacement for the Microsoft controls along with the ability to access the user interface over a secure connection to a public server using a web browser. This is accomplished by setting up a secure server (RemoteWebWindowService) in the cloud and pointing your browser to it. 
With a couple of minimal changes you will be able to either run your app locally or remotely control your application.

The RemoteBlazorWebView package allows developers to create a Blazor UI on Windows, Mac, and Linux. RemoteBlazorWebView is based on Microsoft's WebView control and photino.Blazor. It shares the same server with the WinForms and Wpf controls (RemoteWebWindowService). 
# Use Cases
The primary use case is to be able to share a desktop application controlling hardware with an external service technician. Typically, the application is behind a corporate firewall and is not easily accessed by the technician. The Remote Blazor controls allow the desktop application to be started in "Remote" mode which generates a unique Url for the technician to access the user interface.

Another use case is to be able to monitor data that is behind a firewall or on a private network without the cost and complexity to store the data externally. For example, if a brewer wanted to monitor fermentation data such as PH, Gravity, and Pressure they could build an app showing real-time graphical data using Blazor components and, with only a couple of changes, be able to view the user interface with a web browser from outside of the firewall. 

# How it works

RemoteBlazorWebView.Wpf has two modes of operation. In the first default mode, it works just like the Microsoft BlazorWebView Controls (see [BlazorDesktopWPF](https://github.com/jorgearteiro/BlazorDesktopWPF)). In the second mode, a url is specified on the control's properties. In this mode, all GUI interactions are sent to a server which can be accessed with a browser. Hosting the server (RemotableWebViewService.exe) in the cloud allows you to remotely control an application which is behind a firewall or does not have a static IP address.


# Demo Video
![RemoteBlazorWebView](https://admin.remoteblazorwebview.org/RemoteBlazorWebView.gif)

# Download and Run Demo
- Download the latest Release.zip under the Assets folder from https://github.com/budcribar/RemoteBlazorWebViewTutorial/releases
- Extract the files
- Run either the WinForms (RemoteBlazorWebViewTutorial.WinFormsApp.exe) or the Wpf Sample Program (RemoteBlazorWebViewTutorial.WpfApp.exe) which shows the Blazor UI running normally
- Next start the server and run the sample application through the browser
    - Open a powershell window and execute RemoteWebViewService in the background
    ```console
    .\RemoteWebViewService &
    ```
    - Run the sample program with the local server option
    ```console
    .\RemoteBlazorWebViewTutorial.WpfApp.exe -u=https://localhost:5001
    ```
    - Click the link in the sample program

- Finally, Run the sample program and use the secured cloud server 
    ```console
    .\RemoteBlazorWebViewTutorial.WpfApp.exe -u=https://server.remoteblazorwebview.org:443
    ```
    - Click the link in the sample program
    - Create a new user and sign in


# Build and Run Demo

- Install the RemoteWebViewService
```console
dotnet tool update -g PeakSWC.RemoteWebViewService --version 6.*-* 
```

- Start the server
```console
RemoteWebViewService
```

- Open the RemoteBlazorWebViewTutorial.sln with Visual Studio

- Select the Local Profile from the dropdown next to the Run button and click on run

- Make sure the RemoteBlazorWebViewTutorial.WppfApp is set as Startup Project
  
- Next we will run the application remotely

- Select the Remote Profile from the dropdown next to the Run button and click on run

- A main window will come up with a URL. Click on it

- At this point the sample blazor app will be running in a web browser served from RemoteWebViewService!

- Finally, we will run the application from a cloud server (https://server.remoteblazorwebview.org/)

- Select the Cloud Profile from the dropdown next to the Run button and click on run

- A main window will come up with a URL. Click on it

- At this point the sample blazor app will be running in a web browser served from https://server.remoteblazorwebview.org. 
Accessing the server will require that you set up a login. The server is setup for demo purposes only. Contact me at budcribar@msn.com if you would like
help setting up a production server.

# Security

Each time a client application starts it attaches to the server with a unique Guid. This Guid needs to be specified on the Url in order to access the application and provides the first layer of security.
The application is locked once a browser attaches to the server at a given Guid and no other browser instance has access.

The demo version of the RemoteWebViewService does not include authentication but it can be built and configured with Azure AD B2C. 
When built with Azure AD B2C authentication, the server requires that users are authenticated in order to access the endpoints which serve up the demo application user interface. 
The server code is hosted in the RemoteBlazorWebView github repository and includes a Visual Studio project to build the server. This added layer of security should be sufficient for most use cases but the server can be further locked down with firewall rules if needed.
