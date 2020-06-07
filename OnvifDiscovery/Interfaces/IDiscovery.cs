using OnvifDiscovery.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OnvifDiscovery.Interfaces
{
	/// <summary>
	/// Onvif Discovery, has the logic to discover onvif compliant devices on the network
	/// </summary>
	public interface IDiscovery
	{
		/// <summary>
		/// Discover new onvif devices on the network passing a callback
		/// to retrieve devices as they reply
		/// </summary>
		/// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
		/// <param name="onDeviceDiscovered">A method that is called each time a new device replies.</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>The Task to be awaited</returns>
		Task Discover (int timeout, Action<DiscoveryDevice> onDeviceDiscovered,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Discover new onvif devices on the network
		/// </summary>
		/// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>a list of <see cref="DiscoveryDevice"/></returns>
		/// <remarks>Use the <see cref="Discover(int, Action{DiscoveryDevice}, CancellationToken)"/> 
		///  overload (with an action as a parameter) if you want to retrieve devices as they reply.</remarks>
		Task<IEnumerable<DiscoveryDevice>> Discover (int timeout,
			CancellationToken cancellationToken = default);
	}
}
