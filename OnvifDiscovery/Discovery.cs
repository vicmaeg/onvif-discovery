using OnvifDiscovery.Client;
using OnvifDiscovery.Common;
using OnvifDiscovery.Exceptions;
using OnvifDiscovery.Interfaces;
using OnvifDiscovery.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OnvifDiscovery
{
	/// <summary>
	/// Onvif Discovery, has the logic to discover onvif compliant devices on the network
	/// </summary>
	public class Discovery : IDiscovery
	{
		readonly IUdpClientFactory clientFactory;

		/// <summary>
		/// Creates an instance of <see cref="Discovery"/>
		/// </summary>
		public Discovery () : this (new UdpClientFactory ())
		{
		}

		/// <summary>
		/// Creates an instance of <see cref="Discovery"/>
		/// </summary>
		/// <param name="clientFactory">An UDP client factory instance</param>
		public Discovery (IUdpClientFactory clientFactory)
		{
			this.clientFactory = clientFactory;
		}

		/// <summary>
		/// Discover new onvif devices on the network passing a callback
		/// to retrieve devices as they reply
		/// </summary>
		/// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
		/// <param name="onDeviceDiscovered">A method that is called each time a new device replies.</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The Task to be awaited</returns>
		public async Task Discover (int timeout, Action<DiscoveryDevice> onDeviceDiscovered,
			CancellationToken cancellationToken = default)
		{
			var clients = clientFactory.CreateClientForeachInterface ();
			
			if (!clients.Any ()) {
				throw new DiscoveryException ("Missing valid NetworkInterfaces, UdpClients could not be created");
			}

			var discoveredDevicesIPs = new ConcurrentDictionary<string, bool> ();
			void deviceDiscovered(DiscoveryDevice discoveryDevice)
			{
				if (discoveredDevicesIPs.TryAdd(discoveryDevice.Address, true)) {
					onDeviceDiscovered (discoveryDevice);
				}
			}

			var discoveries = clients.Select (client => Discover (timeout, client, deviceDiscovered, cancellationToken)).ToArray();

			await Task.WhenAll(discoveries);
		}

		/// <summary>
		/// Discover new onvif devices on the network
		/// </summary>
		/// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>a list of <see cref="DiscoveryDevice"/></returns>
		/// <remarks>Use the <see cref="Discover(int, Action{DiscoveryDevice}, CancellationToken)"/> 
		///  overload (with an action as a parameter) if you want to retrieve devices as they reply.</remarks>
		public async Task<IEnumerable<DiscoveryDevice>> Discover (int timeout, CancellationToken cancellationToken = default)
		{
			var devices = new List<DiscoveryDevice> ();
			await Discover (timeout, d => devices.Add (d), cancellationToken);

			return devices;
		}

		async Task Discover (int timeout, IOnvifUdpClient client,
			Action<DiscoveryDevice> onDeviceDiscovered,
			CancellationToken cancellationToken = default)
		{
			Guid messageId = Guid.NewGuid ();
			var responses = new List<UdpReceiveResult> ();
			var cts = new CancellationTokenSource (TimeSpan.FromSeconds (timeout));

			try {
				await SendProbe (client, messageId);
				while (true) {
					if (cts.IsCancellationRequested || cancellationToken.IsCancellationRequested)
						break;
					try {
						var response = await client.ReceiveAsync ()
										.WithCancellation (cancellationToken)
										.WithCancellation (cts.Token);

						if (IsAlreadyDiscovered (response, responses))
							continue;

						responses.Add (response);
						var discoveredDevice = ProcessResponse (response, messageId);
						if (discoveredDevice != null) {
#pragma warning disable 4014 // Just trigger the callback and forget about it. This is expected to avoid locking the loop
							Task.Run (() => onDeviceDiscovered (discoveredDevice));
#pragma warning restore 4014
						}
					} catch (OperationCanceledException) {
						// Either the user canceled the action or the timeout has fired
					} catch (Exception) {
						// we catch all exceptions !
						// Something might be bad in the response of a camera when call ReceiveAsync (BeginReceive in socket) fail
					}
				}
			} finally {
				client.Close ();
			}
		}

		async Task SendProbe (IOnvifUdpClient client, Guid messageId)
		{
			var multicastEndpoint = new IPEndPoint (IPAddress.Parse (Constants.WS_MULTICAST_ADDRESS), Constants.WS_MULTICAST_PORT);
			await client.SendProbeAsync (messageId, multicastEndpoint);
		}

		DiscoveryDevice ProcessResponse (UdpReceiveResult response, Guid messageId)
		{
			if (response.Buffer != null) {
				string strResponse = Encoding.UTF8.GetString (response.Buffer);
				XmlProbeReponse xmlResponse = DeserializeResponse (strResponse);
				if (IsFromProbeMessage (messageId, xmlResponse)
					&& xmlResponse.Body.ProbeMatches.Any ()
					&& !string.IsNullOrEmpty (xmlResponse.Body.ProbeMatches[0].Scopes)) {
					return DeviceFactory.CreateDevice (xmlResponse.Body.ProbeMatches[0], response.RemoteEndPoint);
				}
			}
			return null;
		}

		XmlProbeReponse DeserializeResponse (string xml)
		{
			XmlSerializer serializer = new XmlSerializer (typeof (XmlProbeReponse));
			XmlReaderSettings settings = new XmlReaderSettings ();
			using (StringReader textReader = new StringReader (xml)) {
				using (XmlReader xmlReader = XmlReader.Create (textReader, settings)) {
					return (XmlProbeReponse)serializer.Deserialize (xmlReader);
				}
			}
		}

		bool IsAlreadyDiscovered (UdpReceiveResult device, List<UdpReceiveResult> devices)
		{
			var deviceEndpointString = device.RemoteEndPoint.ToString ();
			return devices.Any (d => d.RemoteEndPoint.ToString ().Equals (deviceEndpointString));
		}

		bool IsFromProbeMessage (Guid messageId, XmlProbeReponse response)
		{
			return response?.Header?.RelatesTo.Contains (messageId.ToString ()) ?? false;
		}
	}
}
