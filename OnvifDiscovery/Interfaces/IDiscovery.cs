using OnvifDiscovery.Models;
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
		/// Discover new onvif devices on the network
		/// </summary>
		/// <param name="timeout">A timeout in seconds to wait for onvif devices</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>a list of <see cref="DiscoveryDevice"/></returns>
		Task<IEnumerable<DiscoveryDevice>> Discover (int timeout,
			CancellationToken cancellationToken = default);
	}
}
