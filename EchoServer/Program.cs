using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

			TcpListener server = new TcpListener(IPAddress.IPv6Loopback, 8080);
			Console.WriteLine("Starting listener");
			server.Start();

			while (true)
			{
				TcpClient client = await server.AcceptTcpClientAsync();

				Task.Factory.StartNew(() => HandleConnection(client));
			}

			server.Stop();
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
