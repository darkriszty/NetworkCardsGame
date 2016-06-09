using Shared.TcpCommunication;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EchoClient
{
	class Program
	{
		private static NetworkStreamWriter _writer = new NetworkStreamWriter(Constants.MaxWriteRetry, Constants.WriteRetryDelaySeconds);

		static void Main(string[] args)
		{
			Task main = MainAsync(args);
			main.Wait();
		}

		private static async Task MainAsync(string[] args)
		{
			using (TcpClient client = new TcpClient("::1", 8080))
			{
				using (NetworkStream stream = client.GetStream())
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						while (true)
						{
							Console.WriteLine("What to send?");
							string line = Console.ReadLine();

							if (!await _writer.WriteLineAsync(stream, line))
							{
								continue;
							}

							string response = await reader.ReadLineAsync();
							Console.WriteLine($"Response from server {response}");
						}
					}
				}
			}
		}
	}
}
