using ChessRPS.Pages.Dialogs;
using Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChessRPS.Pages
{
	/// <summary>
	/// Interaction logic for Lobby.xaml
	/// </summary>
	public partial class Lobby : Window
	{
		private List<(string name, int token)> CurrentPlayersList { get; set; }

		public Lobby()
		{
			InitializeComponent();
			LobbySocket.instance.OnRefresh = OnNetworkInfoHandler; //set refresh listerner
			LoadLobbyUsers();
		}

		private void OnNetworkInfoHandler(JObject json)
		{
			switch (json["type"].ToString())
			{
				case "new_user":
					var user = (name: json["name"].ToString(), (int)json["token"]);
					Prefs.Instance.Opt<List<(string, int)>>("players").Add(user);
					CurrentPlayersList.Add(user);
					Dispatcher.Invoke(() => playersListBox.Items.Add(user.name));
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
					if ((bool)json["accept"]) GoToGame(json);
					break;
				case "msg":
					string msg = $"{json["sender"]}: {json["content"]}";

					break;
			}
		}

		private void HandleInvite(MessageBoxResult result, JObject json)
		{
			bool accept = result == MessageBoxResult.Yes;
			json["accept"] = accept;
			json["type"] = "answer";
			json["target_token"] = Prefs.Instance.Opt<int>(Prefs.KEYS.token);

			MyHttpClient.Lobby.SendRequestAsync(MyHttpClient.Endpoints.INVITE, json);

			if (accept) GoToGame(json);
		}

		private void GoToGame(JObject json)
		{
			Dispatcher.Invoke(() =>
			{
				this.Hide();
				//LobbySocket.instance.Disconnect();
				new MainWindow((int)json["game_id"]).ShowDialog();
				//LobbySocket.instance.Connect();
				LoadLobbyUsers();
				this.Show();
			});
		}

		private async void LoadLobbyUsers()
		{
			//refresh players list
			var json = await MyHttpClient.Lobby.SendRequestAsync(MyHttpClient.Endpoints.LOBBY_PLAYERS, new JObject()
			{
				[Prefs.KEYS.token] = Prefs.Instance.Opt<int>(Prefs.KEYS.token)
			});


			RefreshPlayerList(json);
		}

		private void RefreshPlayerList(JObject json)
		{
			CurrentPlayersList = ((JArray)json[Prefs.KEYS.playerList])
								.Select(jt => (name: (string)jt["name"], token: (int)jt["token"]))
								.ToList();
			Prefs.Instance["players"] = CurrentPlayersList;

			//re
			playersListBox.Items.Clear();
			foreach (var (name, token) in CurrentPlayersList)
			{
				playersListBox.Items.Add(name);
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			new LoadingDialog("Exiting", () =>
			{
				try
				{
					var json = MyHttpClient.Lobby.SendRequestAsync(MyHttpClient.Endpoints.LOGOUT, new JObject()
					{
						["token"] = Prefs.KEYS.token
					}).Result;
				}
				catch { }
			}).ShowDialog();

			base.OnClosing(e);
		}

		private void SendChatMsg(object sender, RoutedEventArgs e)
		{

		}

		private void OnPlayerSelected(object sender, SelectionChangedEventArgs e)
		{
			var (name, token) = CurrentPlayersList[playersListBox.SelectedIndex];

			new LoadingDialog($"Inviting {name} to game", () =>
			{
				MyHttpClient.Lobby.SendRequestAsync(MyHttpClient.Endpoints.INVITE, new JObject
				{
					["sender_token"] = Prefs.Instance.Opt<int>(Prefs.KEYS.token),
					["target_token"] = token,
					["req_type"] = "invite"
				});
			}).ShowDialog();
		}
	}
}
