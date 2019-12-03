using OnvifDiscovery.Client;
using OnvifDiscovery.Common;
using OnvifDiscovery.Exceptions;
using OnvifDiscovery.Interfaces;
using OnvifDiscovery.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
		/// Discover new onvif devices on the network
		/// </summary>
		/// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>a list of <see cref="DiscoveryDevice"/></returns>
		public async Task<IEnumerable<DiscoveryDevice>> Discover (int timeout, CancellationToken cancellationToken = default)
		{
			var devices = new List<DiscoveryDevice> ();
			List<Task<IEnumerable<DiscoveryDevice>>> discoveries = new List<Task<IEnumerable<DiscoveryDevice>>> ();

			var clients = clientFactory.CreateClientForeachInterface ();
			if (!clients.Any ()) {
				throw new DiscoveryException ("Missing valid NetworkInterfaces, UdpClients could not be created");
			}
			foreach (var client in clients) {
				discoveries.Add (Discover (timeout, client, cancellationToken));
			}

			var discoverResults = await Task.WhenAll (discoveries);
			foreach (var results in discoverResults) {
				devices.AddRange (results);
			}
			return devices;
		}

		async Task<IEnumerable<DiscoveryDevice>> Discover (int timeout, IOnvifUdpClient client,
		   CancellationToken cancellationToken = default)
		{
			Guid messageId = Guid.NewGuid ();
			var responses = new List<UdpReceiveResult> ();
			var cts = new CancellationTokenSource (TimeSpan.FromSeconds (timeout));

			try {
				await SendProbe (client, messageId);
				while (true) {
					if (cts.IsCancellationRequested || cancellationToken.IsCancellationRequested) {
						break;
					}
					var response = await client.ReceiveAsync ().WithCancellation (cancellationToken).WithCancellation (cts.Token);
					if (!IsAlreadyDiscovered (response, responses)) {
						responses.Add (response);
					}
				}
			} catch (OperationCanceledException) {
				// Either the user canceled the action or the timeout has fired
			} finally {
				client.Close ();
			}
			if (cancellationToken.IsCancellationRequested) {
				return new List<DiscoveryDevice> ();
			}
			return ProcessResponses (responses, messageId);
		}

		async Task SendProbe (IOnvifUdpClient client, Guid messageId)
		{
			var multicastEndpoint = new IPEndPoint (IPAddress.Parse (Constants.WS_MULTICAST_ADDRESS), Constants.WS_MULTICAST_PORT);
			await client.SendProbeAsync (messageId, multicastEndpoint);
		}

		IEnumerable<DiscoveryDevice> ProcessResponses (IEnumerable<UdpReceiveResult> responses, Guid messageId)
		{
			var processedResponse = new List<DiscoveryDevice> ();
			foreach (var response in responses) {
				if (response.Buffer != null) {
					string strResponse = Encoding.UTF8.GetString (response.Buffer);
					XmlProbeReponse xmlResponse = DeserializeResponse (strResponse);
					if (IsFromProbeMessage (messageId, xmlResponse)
						&& xmlResponse.Body.ProbeMatches.Any ()
						&& !string.IsNullOrEmpty (xmlResponse.Body.ProbeMatches[0].Scopes)) {
						var device = CreateDevice (xmlResponse.Body.ProbeMatches[0], response.RemoteEndPoint);
						processedResponse.Add (device);
					}
				}
			}
			return processedResponse;
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
			return response.Header.RelatesTo.Contains (messageId.ToString ());
		}

		DiscoveryDevice CreateDevice (ProbeMatch probeMatch, IPEndPoint remoteEndpoint)
		{
			var discoveryDevice = new DiscoveryDevice ();
			string scopes = probeMatch.Scopes;
			discoveryDevice.Address = remoteEndpoint.Address.ToString ();
			discoveryDevice.Model = Regex.Match (scopes, "(?<=hardware/).*?(?= )")?.Value;
			discoveryDevice.Mfr = ParseMfrFromScopes (scopes);
			discoveryDevice.XAdresses = ConvertToList (probeMatch.XAddrs);
			discoveryDevice.Types = ConvertToList (probeMatch.Types);
			return discoveryDevice;
		}

		string ParseMfrFromScopes (string scopes)
		{
			var nameQuery = scopes.Split (' ').Where (scope => scope.Contains ("name/")).ToArray ();
			var mfrQuery = scopes.Split (' ').Where (scope => scope.Contains ("mfr/")).ToArray ();
			if (mfrQuery.Length > 0) {
				var match = Regex.Match (Uri.UnescapeDataString (mfrQuery[0]), Constants.PATTERN);
				return match.Groups[6].Value;
			}
			if (nameQuery.Length > 0) {
				var match = Regex.Match (Uri.UnescapeDataString (nameQuery[0]), Constants.PATTERN);
				string temp = match.Groups[6].Value;
				if (temp.Contains (" ")) {
					temp = match.Groups[6].Value.Split (' ')[0];
				}
				return temp;
			}
			return string.Empty;
		}

		IEnumerable<string> ConvertToList (string spacedListString)
		{
			var strings = spacedListString.Split (null);
			foreach (var str in strings) {
				yield return str.Trim ();
			}
		}
	}
}
