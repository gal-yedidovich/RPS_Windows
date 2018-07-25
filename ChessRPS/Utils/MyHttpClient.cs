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
				LOGIN = "/login",
				LOGOUT = "/logout",
				LOBBY_PLAYERS = "/lobby/players",
				CHAT = "/lobby/chat",
				INVITE = "/lobby/invite",
				FLAG = "/game/flag",
				TRAP = "/game/trap",
				RANDOM = "/game/random",
				READY = "/game/ready",
				MOVE = "/game/move",
				DRAW = "/game/draw",
				FORFEIT = "/game/forfeit",
				NEW_GAME = "/game/new";
		}

		public static MyHttpClient Lobby { get; } = new MyHttpClient(8003);
		public static MyHttpClient Game { get; } = new MyHttpClient(8004);

		private HttpClient client;
		private MyHttpClient(int port)
		{
			client = new HttpClient { BaseAddress = new Uri($"http://{Prefs.ServerIP}:{port}") };
		}

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
