using Shared.Diagnostics;
using Shared.TcpCommunication;

namespace EchoClient.Controllers
{
	internal class TcpClientFactory
	{
		internal static TcpClientController CreateTcpServerController()
		{
			return new TcpClientController(
				TraceSourceFactory.GetDefaultTraceSource(),
				"localhost", 8080,
				new NetworkStreamReader(Constants.MaxReadRetry, Constants.ReadRetryDelaySeconds),
				new NetworkStreamWriter(Constants.MaxWriteRetry, Constants.WriteRetryDelaySeconds));
		}
	}
}
