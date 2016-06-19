using Shared.TcpCommunication;
using Shared.CommunicationProtocol.v1;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Diagnostics;
using Shared.Diagnostics;

namespace EchoClient
{
	class Program
	{
		private const int UDP_PORT = 8081;
		private static UdpClient _listener;
		private static volatile bool _isConnected = false;
		private static NetworkStreamWriter _writer = new NetworkStreamWriter(Constants.MaxWriteRetry, Constants.WriteRetryDelaySeconds);
		private static NetworkStreamReader _reader = new NetworkStreamReader(Constants.MaxReadRetry, Constants.ReadRetryDelaySeconds);
		private static TraceSource _trace = TraceSourceFactory.GetDefaultTraceSource();

		static void Main(string[] args)
		{
			Task main = MainAsync(args);
			main.Wait();
		}

		static async Task MainAsync(string[] args)
		{
			Task startClient = StartTcpClient("localhost", 8080);
			Task readHeartBeatAndConnect = ReadHearbeatAndConnectAsync();

			await Task.WhenAll(startClient, readHeartBeatAndConnect);
		}

		private static async Task StartTcpClient(string ip, int port)
		{
			if (_isConnected)
				return;

			_isConnected = true;
			using (TcpClient client = new TcpClient())
			{
				try
				{
					client.ConnectAsync(ip, port).Wait(3000);
				}
				catch (Exception)
				{
					_trace.TraceWarning($"Unable to connect, try again later.");
					_isConnected = false;
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
						_isConnected = false;
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
			_isConnected = false;
		}

		static async Task ReadHearbeatAndConnectAsync()
		{
			while (true)
			{
				await Task.Delay(3000);

				// don't do anything if there is already a server connection
				if (_isConnected)
					continue;

				if (_listener == null)
					_listener = new UdpClient(new IPEndPoint(IPAddress.Any, UDP_PORT));

				UdpReceiveResult udp = await _listener.ReceiveAsync();
				string broadcastData = Encoding.ASCII.GetString(udp.Buffer);
				_trace.TraceVerbose($"Received broadcast '{broadcastData}'");
				if (broadcastData.StartsWith(CommunicationFormats.ServerHeartbeat))
				{
					string ipAndPort = broadcastData.Substring(CommunicationFormats.ServerHeartbeat.Length);

					if (ipAndPort.IndexOf(':') > 0)
					{
						// remove any brackets (eg: [::1]:8080 ==> ::1:8080)
						ipAndPort = ipAndPort.Replace("[", string.Empty).Replace("]", string.Empty);
						string ip = ipAndPort.Substring(0, ipAndPort.LastIndexOf(':'));
						string port = ipAndPort.Substring(ipAndPort.LastIndexOf(':') + 1);

						int portNum;
						if (!string.IsNullOrWhiteSpace(ip) && !string.IsNullOrWhiteSpace(port)&& int.TryParse(port, out portNum))
						{
							_trace.TraceVerbose("Server detected, trying to connect...");
							StartTcpClient(ip, portNum);
						}
					}
				}
			}
		}
	}
}
