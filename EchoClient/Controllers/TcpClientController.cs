using Shared;
using Shared.CommunicationProtocol.v1;
using Shared.Diagnostics;
using Shared.TcpCommunication;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoClient.Controllers
{
	internal class TcpClientController : IController
	{
		private readonly TraceSource _trace;
		private readonly string _ip;
		private readonly int _port;
		private readonly NetworkStreamReader _reader;
		private readonly NetworkStreamWriter _writer;

		public TcpClientController(TraceSource trace, string ip, int port, NetworkStreamReader reader, NetworkStreamWriter writer)
		{
			if (trace == null) throw new ArgumentNullException(nameof(trace));
			if (string.IsNullOrWhiteSpace(ip)) throw new ArgumentNullException(nameof(ip));
			if (reader == null) throw new ArgumentNullException(nameof(reader));
			if (writer == null) throw new ArgumentNullException(nameof(writer));

			_trace = trace;
			_ip = ip;
			_port = port;
			_reader = reader;
			_writer = writer;
		}

		public async Task RunAsync(CancellationToken cancellationToken)
		{
			//if (_isConnected)
			//	return;

			//_isConnected = true;
			using (TcpClient client = new TcpClient())
			{
				try
				{
					client.ConnectAsync(_ip, _port).Wait(3000);
				}
				catch (Exception)
				{
					_trace.TraceWarning($"Unable to connect, try again later.");
					//_isConnected = false;
					return;
				}
				using (NetworkStream stream = client.GetStream())
				{
					_trace.TraceInformation("Username:");
					string userName = Console.ReadLine();
					string userNameCommand = string.Concat(CommunicationFormats.SetUserName, userName);
					if (!await _writer.WriteLineAsync(stream, userNameCommand))
					{
						_trace.TraceError("Failed to send username to server, stopping.");
						//_isConnected = false;
						return;
					}

					while (true)
					{
						// read the data to send to the server
						_trace.TraceInformation("What to send?");
						string line = Console.ReadLine();

						// send the text to the server
						if (!await _writer.WriteLineAsync(stream, line))
							break;

						// read the response of the server
						string response = await _reader.ReadLineAsync(stream);
						if (response == null)
							break;

						_trace.TraceVerbose($"Response from server {response}");
					}
				}
			}
			//_isConnected = false;
		}
	}
}
