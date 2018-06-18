using ChessRPS.Utils;
using Client;
using Client.models;
using Client.Utils;
using Client.ViewModels;
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

namespace ChessRPS.Pages.Dialogs
{
	public partial class DrawRPS
	{
		private IDialogState State { get; set; }
		private SquareImage[] Opponent { get; set; }
		private SquareImage attacker, target;
		private int gameID;

		public DrawRPS(SquareImage attacker, SquareImage target, int gameID)
		{
			InitializeComponent();

			State = new ChoosingState(this);
			this.attacker = attacker; //dependency injection, in order to change those squares later
			this.target = target;
			this.gameID = gameID;

			GameSocket.Instance.OnBroadcast += OnReceive; //register to event
			InitBoard();
		}

		/// <summary>
		/// Event handler for OnBroadcast event. when data is receive from GameSocket
		/// </summary>
		/// <param name="json">data from server</param>
		private void OnReceive(JObject json)
		{
			State.OnOpponentSelected(json);
		}

		private void InitBoard()
		{
			string[] paths = { ImageFactory.RED_ROCK, ImageFactory.RED_PAPER, ImageFactory.RED_SCISSORS };

			Opponent = new SquareImage[3];
			SquareImage[] squares = new SquareImage[3];

			for (int i = 0; i < 3; i++)
			{
				//set opponent squares
				{
					var (button, border, img) = CreateView(ImageFactory.BLUE);
					Opponent[i] = new SquareImage
					{
						Button = button,
						Border = border,
						Image = img,
						Square = new Square(0, i)
						{
							Type = SquareType.Rock + i
						}
					};


					Grid.SetColumn(button, i);
					Grid.SetRow(button, 0);
					rootView.Children.Add(button);
				}

				//set my square
				{
					const int row = 2;
					var (button, border, img) = CreateView(paths[i]);

					squares[i] = new SquareImage()
					{
						Button = button,
						Image = img,
						Border = border,
						Square = new Square(row, i)
						{
							Type = SquareType.Rock + i
						}
					};

					int index = i;
					button.Click += (s, e) =>
					{
						State.OnSelecting(squares[index]);
					};

					Grid.SetColumn(button, i);
					Grid.SetRow(button, row);
					rootView.Children.Add(button);
				}
			}
		}

		private (Button button, Border border, Image img) CreateView(string path)
		{
			var img = new Image
			{
				Source = ImageFactory.LoadImage(path)
			};

			var border = new Border
			{
				BorderThickness = new Thickness(1),
				BorderBrush = new SolidColorBrush(Colors.Black),
				Child = img
			};

			var button = new Button
			{
				Content = border,
				Padding = new Thickness(0)
			};
			return (button, border, img);
		}

		private void ResolveDraw(SquareImage mySelection, string oppSelection, int result)
		{
			SquareType type;
			int index;
			switch (oppSelection.ToLower())
			{
				case "rock":
					type = SquareType.Rock;
					index = 0;
					break;
				case "paper":
					type = SquareType.Paper;
					index = 1;
					break;
				default:
					type = SquareType.Scissors;
					index = 2;
					break;
			}

			//hide other squares
			Dispatcher.Invoke(async () =>
			{
				Opponent[(index + 1) % 3].Button.Visibility = Visibility.Collapsed;
				Opponent[(index + 2) % 3].Button.Visibility = Visibility.Collapsed;

				Opponent[index].Image.Source = ImageFactory.LoadBlueImage(type);
				var opp_tuple = (Opponent[index].Square.Type, Opponent[index].Image.Source);
				var my_tuple = (mySelection.Square.Type, mySelection.Image.Source);

				await Task.Delay(500); //for UX

				if (attacker.Square.MyRPS) UpdateSquares(my_tuple, opp_tuple); //im attacking
				else UpdateSquares(opp_tuple, my_tuple);

				MainWindowStates.Moves.Battle(result, attacker, target, gameID);
				this.Close();
			});
		}

		private void UpdateSquares((SquareType type, ImageSource source) attacker, (SquareType type, ImageSource source) target)
		{
			this.attacker.Image.Source = attacker.source;
			this.attacker.Square.Type = attacker.type;

			this.target.Image.Source = target.source;
			this.target.Square.Type = target.type;
		}

		protected override void OnClosed(EventArgs e)
		{
			GameSocket.Instance.OnBroadcast -= OnReceive; //un register from event
			base.OnClosed(e);
		}

		#region States
		private interface IDialogState
		{
			void OnSelecting(SquareImage squareImage);
			void OnOpponentSelected(JObject json);
		}

		private class ChoosingState : IDialogState
		{
			private readonly DrawRPS context;
			private string opponentSelection;

			public ChoosingState(DrawRPS context)
			{
				this.context = context;
				opponentSelection = null;
			}

			public void OnOpponentSelected(JObject json)
			{
				string selected = (string)json["opponent"];
				opponentSelection = selected;

				//show that opponent has selected
			}

			public async void OnSelecting(SquareImage selected)
			{
				selected.Button.Background = new SolidColorBrush(Colors.Green);

				//send selection to server
				context.loadBar.Visibility = Visibility.Visible;
				var response = await MyHttpClient.Game.SendRequestAsync(MyHttpClient.Endpoints.DRAW, new JObject
				{
					["token"] = Prefs.Instance.Token,
					["decision"] = selected.Square.Type.ToString(),
					["gameId"] = context.gameID,
				});

				if (opponentSelection != null) //opponent has selected
				{
					context.loadBar.Visibility = Visibility.Collapsed;
					int result = (int)response["result"];
					context.ResolveDraw(selected, opponentSelection, result);

					context.Close();
				}
				else context.State = new WaitingState(context, selected); //wait for opponent
			}
		}

		private class WaitingState : IDialogState
		{
			private readonly DrawRPS context;
			private readonly SquareImage mySelection;

			public WaitingState(DrawRPS context, SquareImage mySelection)
			{
				this.context = context;
				this.mySelection = mySelection;
				//start progress bar
				context.loadBar.Visibility = Visibility.Visible;
			}

			public void OnOpponentSelected(JObject json)
			{
				string selected = (string)json["opponent"];
				int result = (int)json["result"];

				context.ResolveDraw(mySelection, selected, result);
				//stop progress bar
				context.loadBar.Visibility = Visibility.Collapsed;
				context.Close();
			}

			public void OnSelecting(SquareImage squareImage) { }//Do Nothing
		}
		#endregion
	}
}
