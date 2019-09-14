using OnvifSharp.Discovery.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OnvifSharp.Discovery.Interfaces
{
	public interface IWSDiscovery
	{
		Task<IEnumerable<DiscoveryDevice>> Discover (int timeout,
			CancellationToken cancellationToken = default);
		Task<IEnumerable<DiscoveryDevice>> Discover (int timeout, IUdpClient client,
			CancellationToken cancellationToken = default);
	}
}
