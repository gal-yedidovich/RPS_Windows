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
		Action function;
		public LoadingDialog(string title, Action function)
		{
			InitializeComponent();
			Loaded += OnLoad;
			DialogTitle.Text = title;
			this.function = function;
		}

		private async void OnLoad(object sender, RoutedEventArgs e)
		{
			await Task.Run(function);
			Close();
		}
	}
}
