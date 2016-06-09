using Shared.TcpCommunication;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EchoClient
{
	class Program
	{
		private static NetworkStreamWriter _writer = new NetworkStreamWriter(Constants.MaxWriteRetry, Constants.WriteRetryDelaySeconds);
		private static NetworkStreamReader _reader = new NetworkStreamReader(Constants.MaxReadRetry, Constants.ReadRetryDelaySeconds);

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
					while (true)
					{
						// read the data to send to the server
						Console.WriteLine("What to send?");
						string line = Console.ReadLine();

						// send the text to the server
						if (!await _writer.WriteLineAsync(stream, line))
							continue;

						// read the response of the server
						string response = await _reader.ReadLineAsync(stream);
						if (response == null)
							continue;

						Console.WriteLine($"Response from server {response}");
					}
				}
			}
		}
	}
}
