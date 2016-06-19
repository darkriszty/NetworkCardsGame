using Shared.CommunicationProtocol.v1;
using Shared.Diagnostics;
using Shared.TcpCommunication;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
	class Program
	{
		private const int TCP_PORT = 8080;
		private const int UDP_PORT = 8081;
		private static TcpListener _server;
		private static UdpClient _broadcaster;
		private static NetworkStreamWriter _writer = new NetworkStreamWriter(Constants.MaxWriteRetry, Constants.WriteRetryDelaySeconds);
		private static NetworkStreamReader _reader = new NetworkStreamReader(Constants.MaxReadRetry, 1);
		private static ClientStore _clientStore = new ClientStore();
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
			
			Task broadcasting = BroadcastHearbeatAsync();
			Task tcpServer = StartServer();
			Task commandProcessing = AcceptCommands();

			await Task.WhenAll(broadcasting, tcpServer, commandProcessing);


			if (_broadcaster != null)
				_broadcaster.Close();
		}

		static async Task AcceptCommands()
		{
			string line = null;
			do
			{
				_trace.TraceInformation("Awaiting commands");
				line = Console.ReadLine();
				_trace.TraceInformation($"Command: {line}");
			} while (line != "exit");
		}

		static async Task StartServer()
		{
			try
			{
				_server = new TcpListener(IPAddress.Loopback, TCP_PORT);

				_trace.TraceInformation("Starting listener");
				_server.Start();

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

		static async Task HandleConnection(TcpClient client)
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

		static async Task BroadcastHearbeatAsync()
		{
			while (true)
			{
				await Task.Delay(3000);

				// wait for the server to be created
				if (_server == null)
					continue;

				if (_broadcaster == null)
					_broadcaster = new UdpClient();

				IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, UDP_PORT);

				string hearbeatData = CommunicationFormats.ServerHeartbeat + _server.LocalEndpoint.ToString();
				byte[] bytes = Encoding.ASCII.GetBytes(hearbeatData);

				_trace.TraceVerbose($"Broadcasting '{hearbeatData}'");
				await _broadcaster.SendAsync(bytes, bytes.Length, ip);
			}
		}

		private static bool ProcessPossibleCommand(TcpClient client, string line)
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
