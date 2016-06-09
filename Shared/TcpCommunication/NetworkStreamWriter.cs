using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Shared.TcpCommunication
{
	public class NetworkStreamWriter
	{
		private readonly int _maxWriteRetry;
		private readonly int _writeRetryDelaySeconds;

		public NetworkStreamWriter(int maxWriteRetry, int writeRetryDelaySeconds)
		{
			_maxWriteRetry = maxWriteRetry;
			_writeRetryDelaySeconds = writeRetryDelaySeconds;
		}

		public async Task<bool> WriteLineAsync(NetworkStream stream, string line)
		{
			StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

			int writeTry = 0;
			bool writtenSuccessfully = false;
			while (!writtenSuccessfully && writeTry < _maxWriteRetry)
			{
				try
				{
					writeTry++;
					await writer.WriteLineAsync(line);
					writtenSuccessfully = true;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to write data into stream, try {writeTry} / {_maxWriteRetry}");
					if (!writtenSuccessfully && writeTry == _maxWriteRetry)
					{
						Console.WriteLine($"Write retry reached, please check your connectivity and try again. Error details: {Environment.NewLine}{ex.Message}");
					}
					else
					{
						await Task.Delay(_writeRetryDelaySeconds * 1000);
					}
				}
			}

			return writtenSuccessfully;
		}
	}
}
