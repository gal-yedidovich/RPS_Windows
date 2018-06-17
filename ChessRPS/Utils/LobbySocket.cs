using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client.Utils
{
	/// <summary>
	/// singleton class, that listen for broadcasts from server - to update chat and players list
	/// </summary>
	class LobbySocket
	{
		public static readonly LobbySocket instance = new LobbySocket();

		private Socket socket;
		private IPEndPoint ep;
		private LobbySocket()
		{
			socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
			ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15001); //port of lobby broadcasts
			Connect();
		}

		public void Connect()
		{
			
			Task.Run(async () =>
			{
				await socket.ConnectAsync(ep);

				int token = Prefs.Instance.Opt<int>(Prefs.KEYS.token);
				var test = BitConverter.GetBytes(token);

				socket.Send(test);

				ListenToServer();
			});
		}

		private void ListenToServer()
		{
			while (true)
			{
				StringBuilder sb = new StringBuilder();
				byte[] buffer = new byte[4];
				socket.Receive(buffer); //receive data size
 
				int size = BitConverter.ToInt32(buffer, 0); //get size of data
				buffer = new byte[size];
				socket.Receive(buffer); //receive actual data

				string data = Encoding.UTF8.GetString(buffer);

				OnRefresh?.Invoke(JObject.Parse(data));
			}
		}

		public Action<JObject> OnRefresh { get; set; }

		public void Disconnect()
		{
			socket.Disconnect(true);
		}
	}
}
