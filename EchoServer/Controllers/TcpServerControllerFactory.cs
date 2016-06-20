using Shared.Diagnostics;
using Shared.TcpCommunication;
using System.Net;
using System.Net.Sockets;

namespace EchoServer.Controllers
{
	internal static class TcpServerControllerFactory
	{
		private const int TCP_PORT = 8080;

		internal static TcpServerController CreateTcpServerController(out string address)
		{
			var server = new TcpListener(IPAddress.Loopback, TCP_PORT);
			server.Start();
			address = server.LocalEndpoint.ToString();

			return new TcpServerController(
				server,
				TraceSourceFactory.GetDefaultTraceSource(), 
				new ClientStore(), 
				new NetworkStreamReader(Constants.MaxReadRetry, 1), 
				new NetworkStreamWriter(Constants.MaxWriteRetry, Constants.WriteRetryDelaySeconds));
		}
	}
}
