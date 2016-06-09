using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoServer
{
	class Program
	{
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
					using (StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true })
					{
						using (StreamReader reader = new StreamReader(stream, Encoding.ASCII))
						{
							while (true)
							{
								string line = await reader.ReadLineAsync();
								Console.WriteLine($"Received {line}");
								await writer.WriteLineAsync(line);
							}
						}
					}
				}
			}
			finally
			{
				Console.WriteLine("Client connection closed");
				client.Close();
			}
		}
	}
}
