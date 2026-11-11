# RemoteBlazorWebViewTutorial

Microsoft is currently supporting two Blazor WebView Controls in .NET 10. One control (`Microsoft.AspNetCore.Components.WebView.Wpf`) targets Windows Presentation Foundation (WPF) apps and
the other (`Microsoft.AspNetCore.Components.WebView.WindowsForms`) targets Windows Form (WinForms) apps. 
These Microsoft controls enable developers to create user interfaces for desktop apps using Blazor web technology. 
Desktop apps using Blazor have the full feature set and performance of .NET 10 and are not constrained like a Blazor WebAssembly app.

The Remote versions of the Blazor WebView Controls (`PeakSWC.RemoteBlazorWebView.Wpf` and `PeakSWC.RemoteBlazorWebView.WindowsForms`) serve as a drop-in replacements for the Microsoft controls, 
with the added ability to access the user interface over a secure connection to a public server using a web browser. 
This is achieved by setting up a secure server (`RemoteWebViewService`) in the cloud and pointing your browser to it. 
With a few minor changes, you can run your app locally or remotely control your application.

The RemoteBlazorWebView package allows developers to create a Blazor UI on Windows, Mac, and Linux. 
RemoteBlazorWebView is based on Microsoft's WebView control and `Photino.Blazor`. 
It shares the same server with the WinForms and Wpf controls (`RemoteWebViewService`). 
# Use Cases
The primary use case is to share a desktop application with someone who is somewhere else: a service technician, a coworker, a support desk.
Typically, the application is behind a corporate firewall and is not easily reached from outside. 
The Remote Blazor controls let the desktop application be started in "Remote" mode, which generates a unique Url for the remote user. The person at the PC keeps using the app in its own window, 
hands control to the remote user when they want help, and takes it back when they are done.

Another use case is to be able to monitor data that is behind a firewall or on a private network without the cost and complexity to store the data externally. 
For example, if a brewer wanted to monitor fermentation data such as PH, Gravity, and Pressure they could build an app showing real-time graphical data using Blazor components and, with only a couple of changes, be able to view the user interface with a web browser from outside of the firewall. 

# How it works

`RemoteBlazorWebView.Wpf` has two modes of operation. In the first default mode, it works just like the Microsoft BlazorWebView Controls (see [BlazorDesktopWPF](https://github.com/jorgearteiro/BlazorDesktopWPF)). 
In the second mode, a url is specified on the control's properties. 
In this mode the app still runs on the PC, but its user interface goes through a server, where browsers can show it and send their input back. 
Hosting the server (`RemoteWebViewService`) in the cloud allows you to reach an application which is behind a firewall or does not have a static IP address.

What a remote app can do:

- **Sharing control.** The app shows in its own window with the person at the PC in control, and a remote user joins from a browser and is handed control. One owner, one remote user, any number of read-only viewers. This is the recommended arrangement.
- **A session bar** in every browser: who has control, Give control / Take control, Copy link, and Restart app. It is opt-in: the built-in bar where you put its placeholder, your own markup from a template, or no bar at all.
- **Unattended sessions**, where the remote user gets control as soon as they join.
- **A voice call** between the person at the PC and the remote user.
- **Browser only**, the simplest arrangement: the app runs only in the browser and the window shows the link. The sample programs below start this way.

A source generator writes the setup for you: mark the window with `[RemoteBlazor]` and call the generated `InitializeRemoteBlazor()`. 
See [Converting a WPF Blazor Application to a Remote WPF Blazor Application](WpfBlazor/ConvertToRemoteBlazor.md).


# Demo Video
![RemoteBlazorWebView](https://raw.githubusercontent.com/budcribar/RemoteBlazorWebView/net10/RemoteBlazorWebView.gif)

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
    .\RemoteBlazorWebViewTutorial.WpfApp.exe -u=https://server.remoteblazorwebview.com:34544
    ```
    - Click the link in the sample program


# Build and Run Demo

- Build from the command line (no Visual Studio required)
```console
dotnet build -p:EnvBuildMode=Package
```
`EnvBuildMode=Package` uses the published PeakSWC NuGet packages. Without it the build looks for a sibling RemoteBlazorWebView source checkout.

- Run an app with a server URL and, optionally, a fixed Guid
```console
dotnet run --project RemoteBlazorWebViewTutorial.WinFormsApp -- -u=https://localhost:5001 -i=<guid>
```
Browse to `<server url>/app/<guid>`. Each Guid can only be opened by one browser: a second visit returns 409 "Locked" by design.

- Try a shared session with the converted sample
```console
dotnet run --project WpfBlazorRemote
```
The app opens in its own window with you in control. Press Copy link, open the link in a browser, then press Give control. Add `-- --unattended` or `-- --voice` to try those.

- Install the RemoteWebViewService
```console
dotnet tool update -g PeakSWC.RemoteWebViewService --version 10.*-* 
```

- Start the server
```console
RemoteWebViewService
```

- Open the RemoteBlazorWebViewTutorial.sln with Visual Studio

- Select the Local Profile from the dropdown next to the Run button and click on run

- Make sure the RemoteBlazorWebViewTutorial.WpfApp is set as Startup Project
  
- Next we will run the application remotely

- Select the Remote Profile from the dropdown next to the Run button and click on run

- A main window will come up with a URL. Click on it

- At this point the sample blazor app will be running in a web browser served from RemoteWebViewService!

- Finally, we will run the application from a cloud server (https://server.remoteblazorwebview.com:34544)

- Select the Cloud Profile from the dropdown next to the Run button and click on run

- A main window will come up with a URL. Click on it

- At this point the sample blazor app will be running in a web browser served from https://server.remoteblazorwebview.com:34544. 
The server is setup for demo purposes only and has no login. Contact me at budcribar@msn.com if you would like
help setting up a production server.

# Using your own server

The cloud server uses a Let's Encrypt certificate. If you host your own server with a self-signed certificate, the client machine must trust that certificate (for example by importing it into the Windows Trusted Root store).

# Security

Each time a client application starts it attaches to the server with a unique Guid. This Guid is part of every link to the application and provides the first layer of security: treat a link as a secret.

- The application's own page (`/app/<guid>`) is locked once a browser attaches to it, and no other browser can open it.
- In a shared session, the link the remote user joins at (`/mirror/<guid>`) is read-only. Anyone who has it can watch. Only the first person to join can be handed control, and only the person at the PC, or whoever currently has control, can hand it over.
- The person at the PC can always take control back.

The demo version of the RemoteWebViewService does not include authentication but it can be built and configured with Azure AD B2C. 
When built with Azure AD B2C authentication, the server requires that users are authenticated in order to access the endpoints which serve up the demo application user interface. 
The server code is hosted in the RemoteBlazorWebView github repository and includes a Visual Studio project to build the server. This added layer of security should be sufficient for most use cases but the server can be further locked down with firewall rules if needed.

# Changing the default server port


Create an appsettings.json file with the contents: (Replace 5002 with the desired port)

```
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5002"
      }
    }
  }
}

```

- Copy the appsettings.json into the directory

```
cd %USERPROFILE%\.dotnet\tools
```


# Convert To Remote
[Converting a WPF Blazor Application to a Remote WPF Blazor Application](WpfBlazor/ConvertToRemoteBlazor.md): the conversion itself, then sharing control, the session bar, unattended sessions, voice and restarting.

The samples in this repository:

| Project | What it shows |
|---|---|
| `WpfBlazor` | A plain WPF Blazor app, the starting point of the conversion |
| `WpfBlazorRemote` | The same app converted, running as a shared session |
| `RemoteBlazorWebViewTutorial.WpfApp`, `.WinFormsApp` | The browser-only arrangement with a mirror in the window, set up by `[RemoteBlazor(Mirror = true, RestartOnRefresh = true, ShutdownOnDisconnect = true)]` |
| `RemoteBlazorWebViewTutorial` | The same on Photino (Windows, Mac, Linux), set up by hand: the generator covers the WPF and WinForms controls |


# Move a Component into Its Own Process
[Moving a Blazor Component into Its Own Process](WpfBlazor/ConvertToRemoteComponent.md): when it's worth it, and how to do it with remote components
