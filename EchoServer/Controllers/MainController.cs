using Shared;
using Shared.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer.Controllers
{
	internal class MainController : IController
	{
		private IEnumerable<Task> _tasks;
		private readonly TraceSource _trace;

		public MainController(TraceSource trace)
		{
			if (trace == null) throw new ArgumentNullException(nameof(trace));

			_trace = trace;
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			_tasks = CreateMainTasks(cancellationToken);

			// stop when either one of the tasks finishes
			await Task.WhenAny(_tasks);

			//TODO: double check the error tracing here
		}

		public IEnumerable<Task> CreateMainTasks(CancellationToken cancellationToken)
		{
			string serverListeningAddress;
			Task tcpServer = TcpServerControllerFactory.CreateTcpServerController(out serverListeningAddress).RunAsync(cancellationToken);
			Task commandProcessing = new CommandProcessingController(TraceSourceFactory.GetDefaultTraceSource()).RunAsync(cancellationToken);
			Task broadcasting = new HeartbeatController(serverListeningAddress, TraceSourceFactory.GetDefaultTraceSource()).RunAsync(cancellationToken);

			yield return tcpServer;
			yield return commandProcessing;
			yield return broadcasting;
		}
	}
}
