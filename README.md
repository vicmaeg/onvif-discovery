# Onvif Discovery

[![NuGet version (OnvifDiscovery)](https://img.shields.io/nuget/v/OnvifDiscovery.svg?style=flat-square)](https://www.nuget.org/packages/OnvifDiscovery/) [![Build Status](https://dev.azure.com/vmaeg/onvif-discovery/_apis/build/status/vicmaeg.onvif-discovery?branchName=master)](https://dev.azure.com/vmaeg/onvif-discovery/_build/latest?definitionId=3&branchName=master)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=vicmaeg_onvif-discovery&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=vicmaeg_onvif-discovery)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=vicmaeg_onvif-discovery&metric=coverage)](https://sonarcloud.io/summary/new_code?id=vicmaeg_onvif-discovery)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=vicmaeg_onvif-discovery&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=vicmaeg_onvif-discovery)

OnvifDiscovery is a simple cross-platform .NET library to discover ONVIF compliant devices.

## Getting started

OnvifDiscovery sends a probe message to all available network interfaces and waits the timeout specified in order to get the list of discovered onvif devices that replied to the probe message.

To use the library install and add a reference of the OnvifDiscovery nuget package, then call the discover method like the following sample:

```cs
// add the using
using OnvifDiscovery;

// Create a Discovery instance
var onvifDiscovery = new Discovery ();

// Call the asynchronous method DiscoverAsync that returns IAsyncEnumerable
// with a timeout of 1 second
await foreach (var device in discovery.DiscoverAsync(1, cancellationToken))
{
    // New device discovered
}
```

Finally, you can also use the DiscoverAsync method by passing a `ChannelWriter<DiscoveryDevice>`, the method will write to the channel devices as soon as they are discovered:

```cs
// add the using
using OnvifDiscovery;

// Create a Discovery instance
var onvifDiscovery = new Discovery ();

// You can call Discover with a ChannelWriter and CancellationToken
CancellationTokenSource cancellation = new CancellationTokenSource ();
var channel = Channel.CreateUnbounded<DiscoveryDevice>();

var discoverTask = onvifDiscovery.DiscoverAsync(channel.Writer, 1, cancellationToken);
await foreach (var device in channel.Reader.ReadAllAsync(cancellationToken))
{
    // New device discovered
}
```

## Obsolete methods from version 1.X
When you update to version 2 you can see that previous available methods are marked as obsolete.
Please use the new methods explained above as they have better support for asynchronous programming.