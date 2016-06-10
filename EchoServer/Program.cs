using Shared.TcpCommunication;
using Shared.TcpProtocol.v1;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EchoServer
{
	class Program
	{
		private static NetworkStreamWriter _writer = new NetworkStreamWriter(Constants.MaxWriteRetry, Constants.WriteRetryDelaySeconds);
		private static NetworkStreamReader _reader = new NetworkStreamReader(Constants.MaxReadRetry, 1);
		private static ConcurrentDictionary<TcpClient, string> _clients = new ConcurrentDictionary<TcpClient, string>(); //TcpClient - username

		static void Main(string[] args)
		{
			Task main = MainAsync(args);
			main.Wait();
		}

		static async Task MainAsync(string[] args)
		{
			Console.WriteLine("Starting server");
#pragma warning disable CS4014
			StartServer();
#pragma warning restore CS4014
			AcceptCommands();
		}

		static void AcceptCommands()
		{
			string line = null;
			do
			{
				Console.WriteLine("Awaiting commands");
				line = Console.ReadLine();
				Console.WriteLine($"Command: {line}");
			} while (line != "exit");
		}

		static async Task StartServer()
		{
			TcpListener server = null;

			try
			{
				server = new TcpListener(IPAddress.IPv6Loopback, 8080);

				Console.WriteLine("Starting listener");
				server.Start();

				while (true)
				{
					TcpClient client = await server.AcceptTcpClientAsync();

					_clients.AddOrUpdate(client, string.Empty, (key, oldValue) => string.Empty);

#pragma warning disable CS4014
					Task.Factory.StartNew(() => HandleConnection(client));
#pragma warning restore CS4014
				}
			}
			finally
			{
				if (server != null)
				{
					try
					{
						server.Stop();
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Failed to stop server{Environment.NewLine}{ex}");
					}
				}
			}
		}

		static async Task HandleConnection(TcpClient client)
		{
			Console.WriteLine($"New connection from {client.Client.RemoteEndPoint}");
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

						ProcessPossibleCommand(client, line);

						string userName = _clients[client];
						Console.WriteLine($"{userName}: {line}");

						// echo back
						await _writer.WriteLineAsync(stream, line);
					}
				}
			}
			finally
			{
				Console.WriteLine("Client connection closed");
				client.Close();
				string userName;
				_clients.TryRemove(client, out userName);
				Console.WriteLine($"{userName} disconnected");
			}
		}

		private static void ProcessPossibleCommand(TcpClient client, string line)
		{
			if (line.StartsWith(CommunicationFormats.SetUserName))
			{
				string userName = line.Substring(CommunicationFormats.SetUserName.Length);
				_clients.AddOrUpdate(client, userName, (k, v) => userName);
			}
		}
	}
}
