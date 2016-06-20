using Shared;
using Shared.CommunicationProtocol.v1;
using Shared.Diagnostics;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer.Controllers
{
	/// <summary>
	/// Responsible to send out heartbeat messages with the server connection details.
	/// </summary>
	internal class HeartbeatController : IController
	{
		private const int UDP_PORT = 8081;
		private UdpClient _broadcaster;
		private readonly TraceSource _trace;
		private readonly string _serverAddress;

		public HeartbeatController(string serverAddress, TraceSource trace)
		{
			if (string.IsNullOrWhiteSpace(serverAddress)) throw new ArgumentNullException(nameof(serverAddress));
			if (trace == null) throw new ArgumentNullException(nameof(trace));

			_serverAddress = serverAddress;
			_trace = trace;
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			using (_broadcaster = new UdpClient())
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					await Task.Delay(3000);

					IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, UDP_PORT);

					string hearbeatData = string.Concat(CommunicationFormats.ServerHeartbeat, _serverAddress);
					byte[] bytes = Encoding.ASCII.GetBytes(hearbeatData);

					_trace.TraceVerbose($"Broadcasting '{hearbeatData}'");
					await _broadcaster.SendAsync(bytes, bytes.Length, ip);
				}
			}
		}
	}
}
