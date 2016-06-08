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

			CancellationToken cancellationToken = new CancellationTokenSource().Token;

			TcpListener listener = new TcpListener(IPAddress.IPv6Loopback, 8080);

			listener.Start();

			TcpClient client = await listener.AcceptTcpClientAsync();
			client.ReceiveTimeout = 30;
			NetworkStream stream = client.GetStream();
			StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
			StreamReader reader = new StreamReader(stream, Encoding.ASCII);

			while (true)
			{
				string line = await reader.ReadLineAsync();
				Console.WriteLine($"Received {line}");
				await writer.WriteLineAsync(line);

				if (cancellationToken.IsCancellationRequested)
					break;
			}

			listener.Stop();
		}
	}
}
