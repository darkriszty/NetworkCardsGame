using EchoServer.Controllers;
using Shared.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
	class Program
	{
		static void Main(string[] args)
		{
			Task main = MainAsync(args);
			main.Wait();
		}

		static async Task MainAsync(string[] args)
		{
			CancellationTokenSource cts = new CancellationTokenSource();
			CancellationToken cancellationToken = cts.Token;

			await new MainController(TraceSourceFactory.GetDefaultTraceSource()).RunAsync(cancellationToken);
		}
	}
}
