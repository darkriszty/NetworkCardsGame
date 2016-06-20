using Shared;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer.Controllers
{
	/// <summary>
	/// Responsible to process server side console commands.
	/// </summary>
	internal class CommandProcessingController : IController
	{
		private readonly TraceSource _trace;

		public CommandProcessingController(TraceSource trace)
		{
			if (trace == null) throw new ArgumentNullException(nameof(trace));

			_trace = trace;
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				string line = null;
				while (true)
				{
					_trace.TraceInformation("Awaiting commands");
					await Task.Run(() => { line = Console.ReadLine(); });
					_trace.TraceInformation($"Command: {line}");

					if (line == "exit")
						return;
				}
			}
		}
	}
}
