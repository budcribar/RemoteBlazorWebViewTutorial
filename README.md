# RemoteBlazorWebViewTutorial

Microsoft has recently introduced two Blazor WebView Controls in .NET 6 Preview 3. One Blazor WebView control (Microsoft.AspNetCore.Components.WebView.Wpf) targets Windows Presentation Foundation (WPF) apps and
the other (Microsoft.AspNetCore.Components.WebView.WindowsForms) targets Windows Form (WinForms) apps. The Microsoft controls allow developers to create user interfaces for desktop apps using Blazor web technology. The desktop apps using Blazor have the full feature set and performance of .NET 6 and are not contrained like a Blazor WebAssembly app.

The Remote versions of the Blazor WebView Controls (PeakSWC.RemoteBlazorWebView.Wpf and PeakSWC.RemoteBlazorWebView.WindowsForms) act as a drop-in replacement for the Microsoft controls along with the ability to share the user interface over a secure connection to a public server using a web browser. This is accomplished by setting up a secure server (RemoteWebWindowService) in the cloud and pointing your browser to it. 
With a couple of minimal changes you will be able to either run your app locally or remotely control your application.

# Use Cases
The primary use case is to be able to share a desktop application controlling hardware with an external service technician. Typically, the application is behind a corporate firewall and is not easily accessed by the technician. The Remote Blazor controls allow the desktop application to be started in "Remote" mode which generates a unique Url for the technician to access the user interface.

Another use case is to be able to monitor data that is behind a firewall or on a private network without the cost and complexity to store the data externally. For example, if a brewer wanted to monitor fermentation data such as PH, Gravity, and Pressure they could build an app showing real-time graphical data using Blazor components and, with only a couple of changes, be able to view the user interface with a web browser from outside of the firewall. 

# How it works

RemoteBlazorWebView.Wpf has two modes of operation. In the first default mode, it works just like the Microsoft BlazorWebView Controls (see [BlazorDesktopWPF](https://github.com/jorgearteiro/BlazorDesktopWPF)). In the second mode, a url is specified on the control's properties. In this mode, all GUI interactions are sent to a server which can be accessed with a browser. Hosting the server (RemotableWebViewService.exe) in the cloud allows you to remotely control an application which is behind a firewall or does not have a static IP address.


# Demo Quick Start

*** Note the name change to RemoteWebViewService

Install the RemoteWebViewService
```console
dotnet tool update -g PeakSWC.RemoteWebViewService --version 6.*-* 
```

Start the server
```console
RemoteWebViewService
```

Open the RemoteBlazorWebViewTutorial.sln with Visual Studio

Run the Local Profile using the Run button 

Make sure the RemoteBlazorWebViewTutorial.WppfApp is set as Startup Project
  
Next we will run the application remotely

Run the Remote Profile using the Run button

A main window will come up with a URL. Click on it

At this point the sample blazor app will be running in a web browser!





