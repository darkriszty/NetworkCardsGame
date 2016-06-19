using Shared.Diagnostics;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Shared.TcpCommunication
{
	public class NetworkStreamReader
	{
		private readonly int _maxReadRetry;
		private readonly int _readRetryDelaySeconds;
		private readonly TraceSource _trace = TraceSourceFactory.GetDefaultTraceSource();

		public NetworkStreamReader(int maxReadRetry, int readRetryDelaySeconds)
		{
			_maxReadRetry = maxReadRetry;
			_readRetryDelaySeconds = readRetryDelaySeconds;
		}

		public async Task<string> ReadLineAsync(NetworkStream stream, bool verbose = true)
		{
			string result = null;
			StreamReader reader = new StreamReader(stream);

			int readTry = 0;
			bool readSuccessfully = false;
			while (!readSuccessfully && readTry < _maxReadRetry)
			{
				try
				{
					readTry++;
					result = await reader.ReadLineAsync();
					readSuccessfully = true;
				}
				catch (Exception ex)
				{
					if (verbose)
						_trace.TraceWarning($"Failed to read data from stream, try {readTry} / {_maxReadRetry}");
					if (!readSuccessfully && readTry == _maxReadRetry)
					{
						if (verbose)
							_trace.TraceError($"Read retry reached, please check your connectivity and try again. Error details: {Environment.NewLine}{ex.Message}");
					}
					else
					{
						await Task.Delay(_readRetryDelaySeconds * 1000);
					}
				}
			}

			return result;
		}
	}
}
