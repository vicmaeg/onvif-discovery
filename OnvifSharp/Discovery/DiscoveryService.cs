using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OnvifSharp.Discovery.Client;
using OnvifSharp.Discovery.Common;
using OnvifSharp.Discovery.Interfaces;
using OnvifSharp.Discovery.Models;

namespace OnvifSharp.Discovery
{
	public class DiscoveryService : IDiscoveryService
	{
		readonly IWSDiscovery wsDiscovery;
		CancellationTokenSource cancellation;
		bool isRunning;

		public DiscoveryService () : this (new WSDiscovery ())
		{

		}

		public DiscoveryService (IWSDiscovery wsDiscovery)
		{
			DiscoveredDevices = new ObservableCollection<DiscoveryDevice> ();
			this.wsDiscovery = wsDiscovery;
		}

		public ObservableCollection<DiscoveryDevice> DiscoveredDevices { get; }

		public async Task Start ()
		{
			if (isRunning) {
				throw new InvalidOperationException ("The discovery is already running");
			}
			isRunning = true;
			cancellation = new CancellationTokenSource ();
			try {
				while (isRunning) {
					var devicesDiscovered = await wsDiscovery.Discover (Constants.WS_TIMEOUT, cancellation.Token);
					if (cancellation.IsCancellationRequested) {
						isRunning = false;
						break;
					}
					SyncDiscoveryDevices (devicesDiscovered);
				}
			} catch (OperationCanceledException) {
				isRunning = false;
			}
		}

		public void Stop ()
		{
			isRunning = false;
			cancellation?.Cancel ();
		}

		void SyncDiscoveryDevices (IEnumerable<DiscoveryDevice> syncDevices)
		{
			var lostDevices = DiscoveredDevices.Except (syncDevices);
			foreach (var lostDevice in lostDevices) {
				DiscoveredDevices.Remove (lostDevice);
			}
			var newDevices = syncDevices.Except (DiscoveredDevices);
			foreach (var newDevice in newDevices) {
				DiscoveredDevices.Add (newDevice);
			}
		}
	}
}
