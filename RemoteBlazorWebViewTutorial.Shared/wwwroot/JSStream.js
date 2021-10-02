function jsToDotNetStreamReturnValue() {
    var len = 10000000;
    var data = new Uint8Array(len);
    for (i = 0; i < len; i++)
        data[i] = i % 256;

    return data;
}


function increment() {
    DotNet.invokeMethodAsync('RemoteBlazorWebViewTutorial.Shared', 'Increment');
}