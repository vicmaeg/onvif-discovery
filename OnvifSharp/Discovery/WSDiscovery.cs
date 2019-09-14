using OnvifSharp.Discovery.Common;
using OnvifSharp.Discovery.Interfaces;
using OnvifSharp.Discovery.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OnvifSharp.Discovery
{
	public class WSDiscovery : IWSDiscovery
	{
		public async Task<IEnumerable<DiscoveryDevice>> Discover (int timeout, CancellationToken cancellationToken = default)
		{
			var devices = new List<DiscoveryDevice> ();
			NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces ();
			List<Task<IEnumerable<DiscoveryDevice>>> discoveries = new List<Task<IEnumerable<DiscoveryDevice>>> ();
			foreach (NetworkInterface adapter in nics) {
				// Only select interfaces that are Ethernet type and support IPv4 (important to minimize waiting time)
				if (adapter.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
					!adapter.NetworkInterfaceType.ToString ().ToLower ().StartsWith ("wireless")) continue;
				if (adapter.OperationalStatus == OperationalStatus.Down) { continue; }
				if (adapter.Supports (NetworkInterfaceComponent.IPv4) == false) { continue; }
				
				IPInterfaceProperties adapterProperties = adapter.GetIPProperties ();
				foreach (var ua in adapterProperties.UnicastAddresses) {
					if (ua.Address.AddressFamily == AddressFamily.InterNetwork) {                 
						IPEndPoint myLocalEndPoint = new IPEndPoint (ua.Address, 0); // port does not matter
						UdpClientWrapper sc = new UdpClientWrapper (myLocalEndPoint);
						discoveries.Add(Discover (timeout, sc, cancellationToken));
					}
				}
			}
			var discoverResults = await Task.WhenAll (discoveries);
			foreach (var results in discoverResults) {
				devices.AddRange (results);
			}
			return devices;
		}

		public async Task<IEnumerable<DiscoveryDevice>> Discover (int timeout, IUdpClient client,
		   CancellationToken cancellationToken = default)
		{

			bool isRunning;

			var devices = new List<DiscoveryDevice> ();
			var responses = new List<UdpReceiveResult> ();
			isRunning = true;
			var cts = new CancellationTokenSource (TimeSpan.FromSeconds (timeout));
			try {
				await SendProbe (client);
				while (isRunning) {
					if (cts.IsCancellationRequested || cancellationToken.IsCancellationRequested) {
						break;
					}
					var response = await client.ReceiveAsync ().WithCancellation (cancellationToken).WithCancellation (cts.Token);
					if (!IsAlreadyDiscovered (response, responses)) {
						responses.Add (response);
					}
				}
			}
			catch (OperationCanceledException) {
				isRunning = false;
			}
			finally {
				client.Close ();
			}
			if (cancellationToken.IsCancellationRequested) {
				return devices;
			}
			devices = ProcessResponses (responses);
			return devices;

		}

		private bool IsAlreadyDiscovered (UdpReceiveResult device, List<UdpReceiveResult> devices)
		{
			var query = devices.Where (temp => temp.RemoteEndPoint.ToString ().Equals (device.RemoteEndPoint.ToString ())).ToArray ();
			return query.Length > 0;
		}

		async Task SendProbe (IUdpClient client)
		{
			var message = WSProbeMessageBuilder.NewProbeMessage ();

			var multicastEndpoint = new IPEndPoint (IPAddress.Parse (Constants.WS_MULTICAST_ADDRESS), Constants.WS_MULTICAST_PORT);
			await client.SendAsync (message, message.Length, multicastEndpoint);
		}

		List<DiscoveryDevice> ProcessResponses (IEnumerable<UdpReceiveResult> responses)
		{
			var processedResponse = new List<DiscoveryDevice> ();
			foreach (var response in responses) {
				if (response.Buffer != null) {
					string strResponse = Encoding.UTF8.GetString (response.Buffer);
					XmlProbeReponse xmlResponse = DeserializeResponse (strResponse);
					if (!String.IsNullOrEmpty (xmlResponse.Body.ProbeMatches[0].Scopes)) {
						processedResponse.Add (CreateDevice (xmlResponse, response.RemoteEndPoint));
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

		DiscoveryDevice CreateDevice (XmlProbeReponse response, IPEndPoint remoteEndpoint)
		{
			var discoveryDevice = new DiscoveryDevice ();
			string scopes = response.Body.ProbeMatches[0].Scopes;
			discoveryDevice.Address = remoteEndpoint.Address.ToString ();
			discoveryDevice.Model = Regex.Match (scopes, "(?<=hardware/).*?(?= )").Value;
			discoveryDevice.Mac = GetMacAddress (discoveryDevice.Address);
			discoveryDevice.Mfr = ParseMfrFromScopes (scopes);
			string xaddr = response.Body.ProbeMatches[0].XAddrs;
			Uri uri = new Uri (xaddr);
			discoveryDevice.Port = uri.Port;
			return discoveryDevice;
		}

		string GetMacAddress (string ipAddress)
		{
			string macAddress = String.Empty;
			System.Diagnostics.Process pProcess = new System.Diagnostics.Process ();
			pProcess.StartInfo.FileName = "arp";
			pProcess.StartInfo.Arguments = "-a " + ipAddress;
			pProcess.StartInfo.UseShellExecute = false;
			pProcess.StartInfo.RedirectStandardOutput = true;
			pProcess.StartInfo.CreateNoWindow = true;
			pProcess.Start ();
			string strOutput = pProcess.StandardOutput.ReadToEnd ();
			string[] substrings = strOutput.Split ('-');
			pProcess.Close ();
			if (substrings.Length >= 8) {
				macAddress = substrings[3].Substring (Math.Max (0, substrings[3].Length - 2))
						 + ":" + substrings[4] + ":" + substrings[5] + ":" + substrings[6]
						 + ":" + substrings[7] + ":"
						 + substrings[8].Substring (0, 2);
			}
			return macAddress;
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
	}
}
