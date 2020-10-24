# RemoteBlazorWebViewTutorial

This tutorial was forked from https://github.com/jspuij/BlazorWebViewTutorial. The difference is that the RemoteBlazorWebView allows you to interact with the user interface of a program developed with the BlazorWebView control using a web browser. This is accomplished by setting up a server (RemoteableWebService) in the cloud and pointing your browser to it. The server canm be downloaded at https://github.com/budcribar/RemoteBlazorWebView/releases
RemoteBlazorWebView is a drop in replacement for BlazorWebView and with a minimal change you will be able to remotely control your application.


# How it works

RemoteBlazorWebView has two modes of operation. In the first default mode, it works just like the BlazorWebView Control (see https://github.com/jspuij/BlazorWebViewTutorial). In the second mode, a url is specified on the Run method. In this mode, all GUI interactions are sent to a server which can be accessed with a browser. Hosting the server (RemotableWebViewService.exe) in the cloud allows you to remotely control an application which is behind a firewall or does not have a static IP address.


#Quick Start

Download the RemotableWebViewService.exe from https://github.com/budcribar/RemoteBlazorWebView/releases

Open a powershell window in your downloads directory

Run the command .\RemoteableWebService.exe

Open the RemoteBlazorWebViewTutorial.sln with Visual Studio

Open the project settings page for the BlazorWebViewTutorial.WpfApp using Visual Studio

Click on the debug tab and set the application arguments to https://localhost:443

Click on the run button in Visual Studio

A main window will come up with a URL. Click on it





