using OnvifDiscovery.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OnvifDiscovery.Interfaces
{
	public interface IOnvifDiscovery
	{
		Task<IEnumerable<DiscoveryDevice>> Discover (int timeout,
			CancellationToken cancellationToken = default);
	}
}
