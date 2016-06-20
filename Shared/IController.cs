using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
	public interface IController
	{
		/// <summary>
		/// Run the controller.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token</param>
		Task RunAsync(CancellationToken cancellationToken);
	}
}
