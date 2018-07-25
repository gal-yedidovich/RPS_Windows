using ChessRPS.Utils;
using Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ChessRPS.Pages
{
	/// <summary>
	/// Interaction logic for Lobby.xaml
	/// </summary>
	public partial class Lobby : Window
	{
		private LoadingDialog loadDialog;
		private List<(string name, int token)> CurrentPlayersList { get; set; }

		public Lobby()
		{
			InitializeComponent();
			SocketClient.Lobby.OnBroadcast += OnNetworkInfoHandler; //set refresh listerner
			LoadLobbyUsers();
		}

		private void OnNetworkInfoHandler(JObject json)
		{
			switch (json["type"].ToString())
			{
				case "new_user":
					int token = (int)json["token"];
					var user = (name: json["name"].ToString(), token);
					CurrentPlayersList.Add(user);
					Dispatcher.Invoke(() =>
					{
						if (!CurrentPlayersList.Select(u => u.token).Contains(token))
						{
							playersListBox.Items.Add(user.name);
						}
					});
					break;
				case "invite":
					Dispatcher.Invoke(() =>
					{
						string sender = json["sender_name"].ToString();
						var result = MessageBox.Show($"You are invited to play with {sender}\nDo you want to play?", "invite", MessageBoxButton.YesNo);
						HandleInvite(result, json);
					});
					break;
				case "answer":
					Dispatcher.Invoke(() => loadDialog.Close());
					if ((bool)json["accept"]) GoToGame(json);
					else MessageBox.Show($"{json["name"]} refused to play");
					break;
				case "msg":
					string msg = $"{json["sender"]}: {json["content"]}";
					Dispatcher.Invoke(() => HandleNewMessage(msg));
					break;
			}
		}

		private void HandleNewMessage(string msg)
		{
			chatListBox.Items.Add(msg);
		}

		private async void HandleInvite(MessageBoxResult result, JObject json)
		{
			bool accept = result == MessageBoxResult.Yes;
			json["accept"] = accept;
			json["type"] = "answer";
			json["target_token"] = Prefs.Instance.Token;

			await MyHttpClient.Lobby.SendRequestAsync(MyHttpClient.Endpoints.INVITE, json);

			if (accept) GoToGame(json);
		}

		private void GoToGame(JObject json)
		{
			Dispatcher.Invoke(() =>
			{
				this.Hide();
				SocketClient.Lobby.OnBroadcast -= OnNetworkInfoHandler; //un-register server

				new MainWindow((int)json["game_id"]).ShowDialog();

				SocketClient.Lobby.OnBroadcast += OnNetworkInfoHandler; //un-register server
				LoadLobbyUsers();
				this.Show();
			});
		}

		private async void LoadLobbyUsers()
		{
			//refresh players list
			var json = await MyHttpClient.Lobby.SendRequestAsync(MyHttpClient.Endpoints.LOBBY_PLAYERS, new JObject()
			{
				["token"] = Prefs.Token
			});


			RefreshPlayerList(json);
		}

		private void RefreshPlayerList(JObject json)
		{
			CurrentPlayersList = ((JArray)json["playerList"])
								.Select(jt => (name: (string)jt["name"], token: (int)jt["token"]))
								.ToList();

			playersListBox.Items.Clear();
			foreach (var (name, token) in CurrentPlayersList)
			{
				playersListBox.Items.Add(name);
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			new LoadingDialog("Exiting", async () =>
			{
				var json = await MyHttpClient.Lobby.SendRequestAsync(MyHttpClient.Endpoints.LOGOUT, new JObject()
				{
					["token"] = Prefs.Token
				});
			}).ShowDialog();

			base.OnClosing(e);
		}

		private async void SendChatMsg(object sender, RoutedEventArgs e)
		{
			var msg = msgTxt.Text.Trim();
			if (msg.Length == 0) return;

			//send message
			var response = await MyHttpClient.Lobby.SendRequestAsync(MyHttpClient.Endpoints.CHAT, new JObject()
			{
				["msg"] = msg,
				["token"] = Prefs.Token,
			});

			//add to listbox
			if ((bool)response["success"])
			{
				chatListBox.Items.Add($"Me: {msg}");
			}
		}

		private async void OnPlayerSelected(object sender, SelectionChangedEventArgs e)
		{
			if (playersListBox.SelectedIndex == -1) return;
			var (name, token) = CurrentPlayersList[playersListBox.SelectedIndex];

			//new LoadingDialog($"Inviting {name} to game", async () =>
			//{
			var json = await MyHttpClient.Lobby.SendRequestAsync(MyHttpClient.Endpoints.INVITE, new JObject
			{
				["sender_token"] = Prefs.Token,
				["target_token"] = token,
				["req_type"] = "invite"
			});
			//}).ShowDialog();

			loadDialog = new LoadingDialog($"Inviting {name} to game", null);
			loadDialog.ShowDialog();

			playersListBox.SelectedIndex = -1;
		}
	}
}
