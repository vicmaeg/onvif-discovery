# Onvif Discovery

[![NuGet version (OnvifDiscovery)](https://img.shields.io/nuget/v/OnvifDiscovery.svg?style=flat-square)](https://www.nuget.org/packages/OnvifDiscovery/) [![Build Status](https://dev.azure.com/vmaeg/onvif-discovery/_apis/build/status/vmartos.onvif-discovery?branchName=master)](https://dev.azure.com/vmaeg/onvif-discovery/_build/latest?definitionId=2&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=vmartos_OnvifSharp&metric=alert_status)](https://sonarcloud.io/dashboard?id=vmartos_OnvifSharp)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=vmartos_OnvifSharp&metric=coverage)](https://sonarcloud.io/dashboard?id=vmartos_OnvifSharp)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=vmartos_OnvifSharp&metric=code_smells)](https://sonarcloud.io/dashboard?id=vmartos_OnvifSharp)

OnvifDiscovery is a simple cross-platform library to discover ONVIF compliant devices.

## Where can I use it

OnvifDiscovery targets .NET Standard 2.0, so it can run on platforms:

* .NET core >= 2.0 (Windows, MacOS, linux)
* .NET Framework >= 4.6.1 (Windows)
* Mono >= 5.4 (Windows, MacOS, linux)
* Xamarin.iOS >= 10.14 (iOS)
* Xamarin.Mac >= 3.8 (MacOS)
* Xamarin.Android >= 8.0 (Android)

More info: [click here](https://docs.microsoft.com/es-es/dotnet/standard/net-standard)

## Getting started

OnvifDiscovery sends a probe message to all available network interfaces and waits the timeout specified in order to get the list of discovered onvif devices that replied to the probe message.

To use the library install and add a reference of the OnvifDiscovery nuget package, then call the discover method like the following sample:

```cs
// add the using
using OnvifDiscovery;

// Create a Discovery instance
var onvifDiscovery = new Discovery ();

// Call the asynchronous method Discover with a timeout of 1 second
var onvifDevices = await onvifDiscovery.Discover (1);

// Alternatively, you can call Discover with a cancellation token
CancellationTokenSource cancellation = new CancellationTokenSource ();
var onvifDevices = await onvifDiscovery.Discover (1, cancellation.Token);
```

Finally, you can also use the Discover method passing a callback, so you will receive calls to that method every time a new camera is discovered, take into account that this callback can be called at the same time from different threads, so make sure your callback is thread-safe:

```cs
// add the using
using OnvifDiscovery;

// Create a Discovery instance
var onvifDiscovery = new Discovery ();

// You can call Discover with a callback (Action) and CancellationToken
CancellationTokenSource cancellation = new CancellationTokenSource ();
await onvifDiscovery.Discover (1, OnNewDevice, cancellation.Token);

private void OnNewDevice (DiscoveryDevice device)
{
    // New device discovered
}
```
