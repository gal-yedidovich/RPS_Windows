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
using System.Windows.Shapes;

namespace ChessRPS.Pages
{
	/// <summary>
	/// Interaction logic for LoadingDialog.xaml
	/// </summary>
	public partial class LoadingDialog : Window
	{
		Action onLoad;

		public LoadingDialog(string title, Action onLoad)
		{
			InitializeComponent();
			Loaded += OnLoad;
			DialogTitle.Text = title;
			this.onLoad = onLoad;
		}

		private void OnLoad(object sender, RoutedEventArgs e)
		{
			if (onLoad != null)
			{
				onLoad();
				Close();
			}
		}

		public LoadingDialog SetTitle(string title)
		{
			DialogTitle.Text = title;
			return this;
		}
	}
}
