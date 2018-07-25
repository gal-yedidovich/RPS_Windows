using Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChessRPS.Utils
{
    public class SocketClient
    {
        public static readonly SocketClient Game = new SocketClient(15001);
        public static readonly SocketClient Lobby = new SocketClient(15002);

        private Socket socket;
        private readonly int port;

        private SocketClient(int port)
        {
            this.port = port;
        }

        private async void ListenToServer()
        {
            try
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                var ep = new IPEndPoint(Prefs.Instance.ServerAddress, port); //port of game broadcasts
                await socket.ConnectAsync(ep);
                socket.Send(BitConverter.GetBytes(Prefs.Instance.Token));
                while (true)
                {

                    byte[] buffer = new byte[4];
                    socket.Receive(buffer); //receive data size

                    int size = BitConverter.ToInt32(buffer, 0); //get size of data
                    buffer = new byte[size];
                    socket.Receive(buffer); //receive actual data

                    var data = JObject.Parse(Encoding.UTF8.GetString(buffer));
                    if ((string)data["type"] == "heartbeat") continue; //ignore heartbeats

                    OnBroadcast?.Invoke(data);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private void Kill()
        {
            socket?.Close();
        }

        public event Action<JObject> OnBroadcast;
    }
}
