using ChessRPS.Pages.MainWindowStates;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChessRPS
{
	public partial class MainWindow : Window
	{
		public static readonly int BOARD_SIZE = 7;

		internal IGameState State { get; set; }
		public (int row, int col) HoveredPosition { get; set; }

		private Grid board;
		internal SquareImage[,] squares = new SquareImage[BOARD_SIZE, BOARD_SIZE];

		public MainWindow(int gameId)
		{
			InitializeComponent();

			InitBoard();
			State = new SelectFlagState(this);
			GameSocket.Instance.OnBroadcast += OnReceivedData;
			GameId = gameId;
			userTxt.Text = (string)Prefs.Instance["name"];
		}

		public void OnReceivedData(JObject json)
		{
            var type = (string)json["type"];

            if (type == "msg")
			{
				//handle chat
			}
			else if(type != "draw")
				State.OnReceivedData(json); //draw is not for us
		}

		private void InitBoard()
		{
			board = new Grid();

			for (int i = 0; i < BOARD_SIZE; i++)
			{
				board.RowDefinitions.Add(new RowDefinition());
				board.ColumnDefinitions.Add(new ColumnDefinition());
			}

			Grid.SetColumn(board, 0);
			rootView.Children.Add(board);
			PrepareSquares();
		}

		internal void PrepareSquares()
		{
			//iterate on board
			for (int r = 0; r < BOARD_SIZE; r++)
			{
				string colorPath = r <= 1 ? ImageFactory.BLUE : r <= 4 ? ImageFactory.NONE : ImageFactory.RED;

				for (int c = 0; c < BOARD_SIZE; c++)
				{

					#region Init Views
					var img = new Image
					{
						Source = ImageFactory.LoadImage(colorPath)
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
					#endregion

					squares[r, c] = new SquareImage
					{
						Image = img,
						Border = border,
						Button = button,
						Square = new Square(r, c)
						{
							MyRPS = r >= BOARD_SIZE - 2,
							Type = r < 2 ? SquareType.UnknownOpponent : SquareType.Empty //my squares are empty yet, type will be defined later (in RPSState)
						}
					};

					//copying position to variables - to save current value when calling Click event
					int row = r,
						col = c;
					button.MouseEnter += (s, e) => //handler when mouse enter square
					{
						HoveredPosition = (row, col);
						State.OnHoveringStart(squares[row, col]);
					};
					button.MouseLeave += (s, e) => //hendler when mouse leaves square
					{
						State.OnHoveringEnd(squares[row, col]);
					};
					button.Click += OnSquareClick;

					Grid.SetRow(button, r);
					Grid.SetColumn(button, c);
					Board.Children.Add(button);
				}
			}
		}

		private void OnSquareClick(object sender, RoutedEventArgs e)
		{
			var (row, col) = HoveredPosition;

			State.SelectSquare(squares[row, col]); //TODO - cleaner code when getting image on click
		}

		//Readonly properties
		public Button StartBtn => doneBtn;

		public Button RndRpsBtn => randomRspBtn;

		public TextBlock MsgTxt => msgTxt;

		public Grid Board => board;

		public int GameId { get; }

		private async void OnRandomRPSClick(object sender, RoutedEventArgs e)
		{
			var json = await MyHttpClient.Game.SendRequestAsync(MyHttpClient.Endpoints.RANDOM, new JObject
			{
				["token"] = Prefs.Instance.Token,
				["gameId"] = GameId
			});

			var types = new Dictionary<SquareType, string> {
				{ SquareType.Rock, ImageFactory.RED_ROCK },
				{ SquareType.Paper, ImageFactory.RED_PAPER },
				{ SquareType.Scissors, ImageFactory.RED_SCISSORS }
			};

			foreach (var entry in json)
			{
				int position = int.Parse(entry.Key);
				var squareImg = squares[position / 10, position % 10];
				squareImg.Square.Type = (SquareType)Enum.Parse(typeof(SquareType), entry.Value["type"].ToString(), true);
				squareImg.Image.Source = ImageFactory.LoadImage(types[squareImg.Square.Type]);
			}

			StartBtn.IsEnabled = true;
		}

		private void OnDoneClick(object sender, RoutedEventArgs e)
		{
			State.Done();
		}

		internal void GameOver(bool won)
		{
			MsgTxt.Text = $"You {(won ? "Won!" : "lost")}";
			State = new GameOverState(this);
		}

        protected override void OnClosed(EventArgs e)
        {
            //GameSocket.Instance.OnBroadcast -= OnReceivedData; //clean up
            base.OnClosed(e);
        }

		private void Cheat(object sender, RoutedEventArgs e)
		{
			State = new MyTurnState(this);
		}
	}
}
