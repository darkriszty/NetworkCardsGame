using System.Collections.Concurrent;
using System.Net.Sockets;

namespace EchoServer
{
	/// <summary>
	/// Holds the client connections with their respective usernames.
	/// </summary>
	internal class ClientStore
	{
		private ConcurrentDictionary<TcpClient, string> _clients = new ConcurrentDictionary<TcpClient, string>(); //TcpClient - username

		/// <summary>
		/// Adds a new client that doesn't have a username yet.
		/// </summary>
		internal void AddInitialClient(TcpClient client)
		{
			_clients.AddOrUpdate(client, string.Empty, (key, oldValue) => string.Empty);
		}

		/// <summary>
		/// Returns the username associated with the provided client.
		/// </summary>
		internal string GetUserName(TcpClient client)
		{
			string userName = _clients[client];
			return userName;
		}

		/// <summary>
		/// Removes the specified client and returns its username.
		/// </summary>
		internal string RemoveClient(TcpClient client)
		{
			string userName = null;
			_clients.TryRemove(client, out userName);
			return userName;
		}

		/// <summary>
		/// Updates the username of the specified client.
		/// </summary>
		/// <param name="client">The client to update</param>
		/// <param name="userName">The username to set</param>
		internal void SetUserName(TcpClient client, string userName)
		{
			_clients.AddOrUpdate(client, userName, (k, v) => userName);
		}
	}
}
