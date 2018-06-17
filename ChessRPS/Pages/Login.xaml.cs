using Client;
using Client.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChessRPS.Pages
{
	/// <summary>
	/// Interaction logic for Login.xaml
	/// </summary>
	public partial class Login : Window
	{
		public Login()
		{
			InitializeComponent();
		}

		private async void OnLoginClick(object sender, RoutedEventArgs e)
		{
			string name = nameTxt.Text;

			if (name.Length >= 2)
			{
				JObject json = new JObject
				{
					["name"] = name,
					["req_type"] = "login"
				};

				progressBar.Visibility = Visibility.Visible;
				var resposne = await MyHttpClient.Lobby.SendRequestAsync(endpoint: "/login", data: json);
				progressBar.Visibility = Visibility.Hidden;

				int token = resposne.Value<int>("token");
				Prefs.Instance["token"] = token;

				new Lobby().Show();
				this.Close();
			}
		}
	}
}
