# RemoteBlazorWebViewTutorial

The RemoteBlazorWebView Wpf control allows you to interact with the user interface of a program developed with the BlazorWebView Wpf control using a web browser. This is accomplished by setting up a server (RemoteableWebService) in the cloud and pointing your browser to it. 

Run the following command to install the RemoteableWebWindowService

```console
dotnet tool update -g PeakSWC.RemoteableWebViewService --version 6.*-*
```

RemoteBlazorWebView is a drop in replacement for BlazorWebView and with a minimal change you will be able to either run your app locally or remotely control your application.


# How it works

RemoteBlazorWebView.Wpf has two modes of operation. In the first default mode, it works just like the BlazorWebView.Wpf Control (see [BlazorDesktopWPF](https://github.com/jorgearteiro/BlazorDesktopWPF)). In the second mode, a url is specified on the control's properties. In this mode, all GUI interactions are sent to a server which can be accessed with a browser. Hosting the server (RemotableWebViewService.exe) in the cloud allows you to remotely control an application which is behind a firewall or does not have a static IP address.


# Quick Start

Install the RemotableWebViewService
```console
dotnet tool update -g PeakSWC.RemoteableWebViewService --version 0.*-*
```

Start the server
```console
RemoteableWebViewService
```

Open the RemoteBlazorWebViewTutorial.sln with Visual Studio

Open the project settings page for the RemoteBlazorWebViewTutorial.WpfApp using Visual Studio

Click on the debug tab and set the application arguments to https://localhost:443

Click on the run button in Visual Studio (make sure the RemoteBlazorWebViewTutorial.WppfApp is set as Startup Project) 

A main window will come up with a URL. Click on it

At this point the sample blazor app will be running in a web browser!

Next we will run the app locally.

Stop RemoteBlazorWebViewTutorial.WpfApp if you still have it running in Visual Studio

Open the project settings page for the RemoteBlazorWebViewTutorial.WpfApp using Visual Studio

Click on the debug tab and clear the application arguments. i.e. Delete the "https://localhost:443"

Now click on the run button in Visual Studio and the app will show the User Interface and run locally




