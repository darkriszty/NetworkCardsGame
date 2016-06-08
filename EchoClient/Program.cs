using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EchoClient
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
			TcpClient client = new TcpClient("::1", 8080);
			NetworkStream stream = client.GetStream();

			StreamReader reader = new StreamReader(stream);
			StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

			while (true)
			{
				Console.WriteLine("What to send?");
				string line = Console.ReadLine();
				await writer.WriteLineAsync(line);
				string response = await reader.ReadLineAsync();
				Console.WriteLine($"Response from server {response}");
			}

			client.Close();
		}
	}
}
