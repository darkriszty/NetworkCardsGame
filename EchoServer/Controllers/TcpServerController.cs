using Shared;
using Shared.CommunicationProtocol.v1;
using Shared.Diagnostics;
using Shared.TcpCommunication;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoServer.Controllers
{
	internal class TcpServerController : IController
	{
		private const int TCP_PORT = 8080;
		private TcpListener _server;
		private NetworkStreamWriter _writer = new NetworkStreamWriter(Constants.MaxWriteRetry, Constants.WriteRetryDelaySeconds);
		private NetworkStreamReader _reader = new NetworkStreamReader(Constants.MaxReadRetry, 1);
		private ClientStore _clientStore = new ClientStore();
		private TraceSource _trace;

		public TcpServerController(TraceSource trace, ClientStore clientStore, NetworkStreamReader reader, NetworkStreamWriter writer)
		{
			if (trace == null) throw new ArgumentNullException(nameof(trace));

			_trace = trace;
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			try
			{
				_server = new TcpListener(IPAddress.Loopback, TCP_PORT);

				_trace.TraceInformation("Starting listener");
				_server.Start();

				Task broadcasting = new HeartbeatController(_server.LocalEndpoint.ToString(), TraceSourceFactory.GetDefaultTraceSource()).RunAsync(cancellationToken);

				while (true)
				{
					TcpClient client = await _server.AcceptTcpClientAsync();

					_clientStore.AddInitialClient(client);

#pragma warning disable CS4014
					Task.Factory.StartNew(() => HandleConnection(client));
#pragma warning restore CS4014
				}
			}
			finally
			{
				if (_server != null)
				{
					try
					{
						_server.Stop();
					}
					catch (Exception ex)
					{
						_trace.TraceError($"Failed to stop server{Environment.NewLine}{ex}");
					}
				}
			}
		}

		private async Task HandleConnection(TcpClient client)
		{
			_trace.TraceInformation($"New connection from {client.Client.RemoteEndPoint}");
			client.ReceiveTimeout = 30;
			client.SendTimeout = 30;

			try
			{
				using (NetworkStream stream = client.GetStream())
				{
					while (true)
					{
						// read the data from the client
						string line = await _reader.ReadLineAsync(stream, false);

						// if the data was not read during the retry process then close this client
						if (line == null)
							break;

						bool commandFound = ProcessPossibleCommand(client, line);

						string userName = _clientStore.GetUserName(client);
						_trace.TraceInformation($"{userName}: {line}");

						// echo back
						if (!commandFound)
							await _writer.WriteLineAsync(stream, line);
					}
				}
			}
			finally
			{
				_trace.TraceVerbose("Client connection closed");
				client.Close();
				string userName = _clientStore.RemoveClient(client);
				_trace.TraceInformation($"{userName} disconnected");
			}
		}

		private bool ProcessPossibleCommand(TcpClient client, string line)
		{
			if (line.StartsWith(CommunicationFormats.SetUserName))
			{
				string userName = line.Substring(CommunicationFormats.SetUserName.Length);
				_clientStore.SetUserName(client, userName);
				return true;
			}

			return false;
		}
	}
}
