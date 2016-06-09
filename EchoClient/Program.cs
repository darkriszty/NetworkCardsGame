using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EchoClient
{
	class Program
	{
		const int MAX_WRITE_RETRY = 3;
		const int WRITE_RETRY_DELAY_SECONDS = 3;

		static void Main(string[] args)
		{
			Task main = MainAsync(args);
			main.Wait();
		}

		static async Task MainAsync(string[] args)
		{
			using (TcpClient client = new TcpClient("::1", 8080))
			{
				using (NetworkStream stream = client.GetStream())
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
						{
							while (true)
							{
								Console.WriteLine("What to send?");
								string line = Console.ReadLine();

								int writeTry = 0;
								bool writtenSuccessfully = false;
								while (!writtenSuccessfully && writeTry < MAX_WRITE_RETRY)
								{
									try
									{
										writeTry++;
										await writer.WriteLineAsync(line);
										writtenSuccessfully = true;
									}
									catch (Exception ex)
									{
										Console.WriteLine($"Failed to send data to server, try {writeTry} / {MAX_WRITE_RETRY}");
										if (!writtenSuccessfully && writeTry == MAX_WRITE_RETRY)
										{
											Console.WriteLine($"Write retry reach, please check your connectivity with the server and try again. Error details: {Environment.NewLine}{ex.Message}");
										}
										else
										{
											await Task.Delay(WRITE_RETRY_DELAY_SECONDS * 1000);
										}
									}
								}

								if (!writtenSuccessfully)
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
}
