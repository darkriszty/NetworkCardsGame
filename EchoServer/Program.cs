using EchoServer.Controllers;
using Shared.Diagnostics;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer
{
	class Program
	{
		private static TraceSource _trace = TraceSourceFactory.GetDefaultTraceSource();

		static void Main(string[] args)
		{
			Task main = MainAsync(args);
			main.Wait();
		}

		static async Task MainAsync(string[] args)
		{
			// for testing automatic client connection via broadcast feature
			await Task.Delay(2000);
			_trace.TraceInformation("Starting server");

			CancellationTokenSource cts = new CancellationTokenSource();
			CancellationToken cancellationToken = cts.Token;

			Task tcpServer = TcpServerControllerFactory.CreateTcpServerController().RunAsync(cancellationToken);
			Task commandProcessing = new CommandProcessingController(TraceSourceFactory.GetDefaultTraceSource()).RunAsync(cancellationToken);

			// close when either the server stops or the command processing returns
			await Task.WhenAny(tcpServer, commandProcessing);
		}
	}
}
