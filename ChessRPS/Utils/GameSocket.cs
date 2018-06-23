using ChessRPS.Pages.Dialogs;
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
    public class GameSocket
    {
        public static readonly GameSocket Instance = new GameSocket();

        private Socket socket;

        private GameSocket()
        {
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            ConnectToPort();
        }

        private void ConnectToPort()
        {
            var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 15002); //port of game broadcasts

            new Thread(()=>
            //Task.Run(async () =>
            {
                //Thread.CurrentThread.IsBackground = false;
                //await socket.ConnectAsync(ep);
                socket.Connect(ep);
                socket.Send(BitConverter.GetBytes(Prefs.Instance.Token));
                ListenToServer();
            }).Start();
        }

        private void ListenToServer()
        {

            while (true)
            {
                byte[] buffer = new byte[4];
                socket.Receive(buffer); //receive data size

                int size = BitConverter.ToInt32(buffer, 0); //get size of data
                buffer = new byte[size];
                socket.Receive(buffer); //receive actual data

                var json = JObject.Parse(Encoding.UTF8.GetString(buffer));
                Debug.WriteLine(json.ToString());
                OnBroadcast?.Invoke(json);
            }
        }

        public event Action<JObject> OnBroadcast;

        public bool Connected => socket.Connected;
    }
}
