using Shared.Diagnostics;
using Shared.TcpCommunication;

namespace EchoServer.Controllers
{
	internal static class TcpServerControllerFactory
	{
		internal static TcpServerController CreateTcpServerController()
		{
			return new TcpServerController(
				TraceSourceFactory.GetDefaultTraceSource(), 
				new ClientStore(), 
				new NetworkStreamReader(Constants.MaxReadRetry, 1), 
				new NetworkStreamWriter(Constants.MaxWriteRetry, Constants.WriteRetryDelaySeconds));
		}
	}
}
