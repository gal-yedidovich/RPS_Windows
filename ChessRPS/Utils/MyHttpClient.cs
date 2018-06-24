using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client.Utils
{
	class MyHttpClient
	{
		public static class Endpoints
		{

			public static readonly string
				LOBBY_PLAYERS = "/lobby/players",
				INVITE = "/lobby/invite",
				LOGOUT = "/logout",
				LOGIN = "/login",
                GET_PORT = "/game/get_port",
				READY = "/game/ready",
				MOVE = "/game/move",
				DRAW = "/game/draw",
				RANDOM = "/game/random",
				FLAG = "/game/flag",
				TRAP = "/game/trap",
                CHAT = "/lobby/chat";
        }

		private static readonly MyHttpClient lobby = new MyHttpClient(8003),
											game = new MyHttpClient(8004);

		private HttpClient client;
		private MyHttpClient(int port)
		{
			client = new HttpClient { BaseAddress = new Uri($"http://{Prefs.Instance.ServerAddress}:{port}") };
		}

		public static MyHttpClient Lobby => lobby;

		public static MyHttpClient Game => game;

		public async void SendRequestAsync(string endpoint, JObject data, Action pre, Action<JObject> post)
		{
			pre?.Invoke();
			post?.Invoke(await SendRequestAsync(endpoint, data));
		}

		public async Task<JObject> SendRequestAsync(string endpoint, JObject data)
		{
			var response = await client.PostAsync(endpoint, new StringContent(data.ToString(), Encoding.UTF8, "application/json"));
			string resData = await response.Content.ReadAsStringAsync();
			return JObject.Parse(resData);
		}
	}
}
