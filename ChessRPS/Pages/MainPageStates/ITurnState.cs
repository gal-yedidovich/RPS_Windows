using ChessRPS.Pages.Dialogs;
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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChessRPS.Pages.MainWindowStates
{
	interface IGameState
	{
		void OnHoveringStart(SquareImage squareImage);
		void OnHoveringEnd(SquareImage squareImage);
		void SelectSquare(SquareImage squareImg);
		void Done();
		void OnReceivedData(JObject json);
	}

	internal class SelectFlagState : IGameState
	{
		private MainWindow _context;
		private ImageSource _oldImageSource;
		private bool isOpponentReady;

		public SelectFlagState(MainWindow context)
		{
			this._context = context;
			isOpponentReady = false;
		}

		public void OnHoveringStart(SquareImage squareImage)
		{
			var img = squareImage.Image;
			var border = squareImage.Border;
			if (_context.HoveredPosition.row >= 5) //if hovering my rows
			{
				_oldImageSource = img.Source;
				img.Source = ImageFactory.LoadImage(ImageFactory.RED_FLAG);
				border.BorderBrush = new SolidColorBrush(Colors.Yellow);
			}
		}

		public void OnHoveringEnd(SquareImage squareImage)
		{
			if (_oldImageSource != null)
			{
				squareImage.Image.Source = _oldImageSource;
				_oldImageSource = null;
			}
			squareImage.Border.BorderBrush = new SolidColorBrush(Colors.Black);
		}

		public async void SelectSquare(SquareImage squareImg)
		{
			if (!squareImg.Square.MyRPS) return;

			var (row, col) = squareImg.Square.Position;
			var json = await MyHttpClient.Game.SendRequestAsync(MyHttpClient.Endpoints.FLAG, new JObject
			{
				["token"] = Prefs.Instance.Token,
				["gameId"] = _context.GameId,
				["row"] = row,
				["col"] = col
			});

			if ((bool)json["success"])
			{
				squareImg.Square.Type = SquareType.Flag;
				_context.State = new SelectTrapState(_context, isOpponentReady);
			}
			else throw new Exception("flag");
		}

		public void Done() { }

		public void OnReceivedData(JObject json)
		{
			if ((string)json["type"] == "opponent ready") isOpponentReady = true;
			_context.Dispatcher.Invoke(() => _context.MsgTxt.Text = "Opponent ready");
		}
	}

	internal class SelectTrapState : IGameState
	{
		private MainWindow _context;
		private ImageSource _oldImageSource;
		private bool isOpponentReady;

		public SelectTrapState(MainWindow context, bool opponentReady)
		{
			_context = context;
			isOpponentReady = opponentReady;
		}

		public void OnHoveringStart(SquareImage squareImage)
		{
			var img = squareImage.Image;
			var border = squareImage.Border;

			if (FlagImg(img)) return;

			if (_context.HoveredPosition.row >= 5) //if hovering my rows
			{
				_oldImageSource = img.Source;
				img.Source = ImageFactory.LoadImage(ImageFactory.RED_TRAP);
				border.BorderBrush = new SolidColorBrush(Colors.Yellow);
			}
		}

		public void OnHoveringEnd(SquareImage squareImage)
		{
			if (_oldImageSource != null)
			{
				squareImage.Image.Source = _oldImageSource;
				_oldImageSource = null;
			}
			squareImage.Border.BorderBrush = new SolidColorBrush(Colors.Black);
		}

		public async void SelectSquare(SquareImage squareImg)
		{
			if (squareImg.Square.Type != SquareType.Empty
				|| !squareImg.Square.MyRPS) return; //flag check & is valid square


			var (row, col) = squareImg.Square.Position;
			var json = await MyHttpClient.Game.SendRequestAsync(MyHttpClient.Endpoints.TRAP, new JObject
			{
				["token"] = Prefs.Instance.Token,
				["gameId"] = _context.GameId,
				["row"] = row,
				["col"] = col
			});

			if ((bool)json["success"])
			{
				squareImg.Square.Type = SquareType.Trap;
				_context.State = new RPSState(_context, isOpponentReady);
			}
			else throw new Exception("trap");
		}

		private bool FlagImg(Image img)
		{
			return ((BitmapImage)img.Source).UriSource.AbsolutePath.Contains("flag");
		}

		public void Done() { }

		public void OnReceivedData(JObject json)
		{
			if ((string)json["type"] == "opponent ready") isOpponentReady = true;
			_context.Dispatcher.Invoke(() => _context.MsgTxt.Text = "Opponent ready");
		}
	}

	internal class RPSState : IGameState
	{
		private MainWindow _context;
		private bool isOpponentReady;
		public RPSState(MainWindow context, bool opponentReady)
		{
			_context = context;
			_context.RndRpsBtn.IsEnabled = true;
			_context.RndRpsBtn.Content = "Random RPS";
			this.isOpponentReady = opponentReady;
		}

		public async void Done()
		{
			_context.RndRpsBtn.IsEnabled = false;
			_context.StartBtn.IsEnabled = false;
			//send ready

			JObject json = new JObject
			{
				["token"] = Prefs.Instance.Token,
				["gameId"] = _context.GameId
			};

			var res = await MyHttpClient.Game.SendRequestAsync(MyHttpClient.Endpoints.READY, json);
			if (!(bool)res["success"]) throw new Exception("ready request failed");
			var myTurn = (int)res["turn"] == Prefs.Instance.Token;
			if (isOpponentReady)
			{
				if (myTurn)
					_context.State = new MyTurnState(_context);
				else
					_context.State = new WaitingState(_context);
			}
			else _context.State = new ReadyState(_context, myTurn);
		}

		public void OnHoveringEnd(SquareImage squareImage)
		{
			squareImage.Border.BorderBrush = new SolidColorBrush(Colors.Black);
		}

		public void OnHoveringStart(SquareImage squareImage)
		{
			squareImage.Border.BorderBrush = new SolidColorBrush(Colors.Yellow);
		}

		public void OnReceivedData(JObject json)
		{
			//do nothing
			if ((string)json["type"] == "opponent ready") isOpponentReady = true;
			_context.Dispatcher.Invoke(() => _context.MsgTxt.Text = "Opponent ready");
		}

		public void SelectSquare(SquareImage squareImg) { }
	}

	/// <summary>
	/// State when im ready and opponent is not
	/// </summary>
	internal class ReadyState : IGameState
	{
		private MainWindow _context;
		private readonly bool myTurn;

		public ReadyState(MainWindow context, bool myTurn)
		{
			_context = context;
			this.myTurn = myTurn;
		}

		public void OnReceivedData(JObject json)
		{
			//when opponent ready
			if ((string)json["type"] == "opponent ready")
			{
				if (myTurn) _context.State = new MyTurnState(_context);
				else _context.State = new WaitingState(_context);
			}
		}

		public void Done() { }

		public void OnHoveringEnd(SquareImage squareImage) { }

		public void OnHoveringStart(SquareImage squareImage) { }

		public void SelectSquare(SquareImage squareImg) { }
	}

	internal class WaitingState : IGameState
	{
		private MainWindow _context;

		public WaitingState(MainWindow context)
		{
			_context = context;
			_context.Dispatcher.Invoke(() => _context.MsgTxt.Text = "Opponent's Turn");
		}

		public void Done() { }

		public void OnHoveringEnd(SquareImage squareImage) { }

		public void OnHoveringStart(SquareImage squareImage) { }

		public async void OnReceivedData(JObject json)
		{
			if ((string)json["type"] == "move") //opponent is moving
			{
				//rotate position
				int size = MainWindow.BOARD_SIZE;
				var from = (row: size - 1 - (int)json["from"]["row"], col: size - 1 - (int)json["from"]["col"]);
				var to = (row: size - 1 - (int)json["to"]["row"], col: size - 1 - (int)json["to"]["col"]);

				//get squares from rotated position
				var from_square = _context.squares[from.row, from.col]; //get selected square
				var to_square = _context.squares[to.row, to.col]; //get target square

				//invoke in UI
				if (json.ContainsKey("battle"))
				{
					if (json.ContainsKey("square_type"))//reveal unknown opponent
					{
						RevealOpponent((string)json["square_type"], from_square);
						await Task.Delay(1000); //wait for UX - let user see opponent rps
					}

					int result = (int)json["battle"];
					_context.Dispatcher.Invoke(() =>
						Moves.Battle(result, attacker: from_square, target: to_square, gameID: _context.GameId));
				}
				else _context.Dispatcher.Invoke(() =>
						Moves.MoveTo(from_square, to_square));

				if (json.ContainsKey("winner"))
					_context.Dispatcher.Invoke(() =>
						_context.GameOver(false));
				else _context.State = new MyTurnState(_context);
			}

		}

		private void RevealOpponent(string squareType, SquareImage pawn)
		{
			string path;
			SquareType type = (SquareType)Enum.Parse(typeof(SquareType), squareType, true);

			switch (type)
			{
				case SquareType.Paper:
					path = ImageFactory.BLUE_PAPER;
					break;
				case SquareType.Rock:
					path = ImageFactory.BLUE_ROCK;
					break;
				case SquareType.Scissors:
					path = ImageFactory.BLUE_SCISSORS;
					break;
				default:
					throw new Exception("invalid type");
			}

			_context.Dispatcher.Invoke(() => pawn.Image.Source = ImageFactory.LoadImage(path));
			pawn.Square.Type = type;
		}

		public void SelectSquare(SquareImage squareImg) { }
	}

	internal class MyTurnState : IGameState
	{
		private MainWindow _context;

		public MyTurnState(MainWindow context)
		{
			_context = context;
			_context.Dispatcher.Invoke(() => _context.MsgTxt.Text = "My Turn");
		}

		public void Done()
		{
			//do nothing
		}

		public void OnHoveringEnd(SquareImage squareImage)
		{
			squareImage.Border.BorderBrush = new SolidColorBrush(Colors.Black);
		}

		public void OnHoveringStart(SquareImage squareImage)
		{
			var border = squareImage.Border;
			if (squareImage.Square.MyRPS) border.BorderBrush = new SolidColorBrush(Colors.Yellow);
		}

		public void OnReceivedData(JObject json) { } //do nothing

		public void SelectSquare(SquareImage squareImg)
		{
			var selected = squareImg.Square;
			if (selected.MyRPS && selected.Type != SquareType.Trap && selected.Type != SquareType.Flag) //valid selection - find possible moves
			{
				List<SquareImage> possibleMoves = new List<SquareImage>();
				var (row, col) = selected.Position;
				var squares = _context.squares;

				//if move is valid & can go there (not my soldier)
				//check up
				if (row - 1 >= 0 && !squares[row - 1, col].Square.MyRPS) possibleMoves.Add(squares[row - 1, col]);
				//check left
				if (col - 1 >= 0 && !squares[row, col - 1].Square.MyRPS) possibleMoves.Add(squares[row, col - 1]);
				//check down
				if (row + 1 < MainWindow.BOARD_SIZE && !squares[row + 1, col].Square.MyRPS) possibleMoves.Add(squares[row + 1, col]);
				//check right
				if (col + 1 < MainWindow.BOARD_SIZE && !squares[row, col + 1].Square.MyRPS) possibleMoves.Add(squares[row, col + 1]);

				//highlight moves
				foreach (var move in possibleMoves)
				{
					move.Button.Background = new SolidColorBrush(Colors.Green);
				}

				//go to selected state
				_context.State = new SelectedSquareState(_context, squareImg, possibleMoves);
			}
		}
	}

	internal class SelectedSquareState : IGameState
	{
		private MainWindow _context;
		private List<SquareImage> _possibleMoves;
		private SquareImage _selected;

		public SelectedSquareState(MainWindow context, SquareImage selected, List<SquareImage> possibleMoves)
		{
			_context = context;
			_possibleMoves = possibleMoves;
			_selected = selected;
		}

		public void Done() { }

		public void OnHoveringEnd(SquareImage squareImage)
		{
			var color = Colors.Black;
			if (_possibleMoves.Contains(squareImage)) color = Colors.Green;
			squareImage.Border.BorderBrush = new SolidColorBrush(color);
		}

		public void OnHoveringStart(SquareImage squareImage)
		{
			squareImage.Border.BorderBrush = new SolidColorBrush(Colors.Yellow);
		}

		public async void SelectSquare(SquareImage squareImg)
		{
			//clear moves highlights
			//if chosen move
			//	do move
			//	go to waiting state
			//else
			//	go to myTurn state

			//clear highlighting
			foreach (var move in _possibleMoves) move.Button.ClearValue(Control.BackgroundProperty);


			if (_possibleMoves.Contains(squareImg)) await SendMove(squareImg);
			else (_context.State = new MyTurnState(_context)).SelectSquare(squareImg);//change state & execute clicking (selecting) on new square (this square is now selected)
		}

		private async Task SendMove(SquareImage target)
		{
			var (to_row, to_col) = target.Square.Position;
			var json = new JObject
			{
				["token"] = Prefs.Instance.Token,
				["gameId"] = _context.GameId,
				["type"] = "move",
				["from"] = new JObject
				{
					["row"] = _selected.Square.Position.row,
					["col"] = _selected.Square.Position.col,
				},
				["to"] = new JObject
				{
					["row"] = to_row,
					["col"] = to_col,
				}
			};

			if (target.Square.Type != SquareType.Empty && !target.Square.MyRPS)
				json["square_type"] = _selected.Square.Type.ToString();

			var response = await MyHttpClient.Game.SendRequestAsync(MyHttpClient.Endpoints.MOVE, json);
			if (!(bool)response["success"]) throw new Exception($"error here - {response}");

			if (response.ContainsKey("s_type")) //reveal unkown opponent
			{
				string t = (string)response["s_type"];
				var square = _context.squares[to_row, to_col];
				square.Square.Type = (SquareType)Enum.Parse(typeof(SquareType), t, true);

				_context.Dispatcher.Invoke(() => square.Image.Source = ImageFactory.LoadBlueImage(square.Square.Type));
				await Task.Delay(1000); //wait for UX - let user see opponent rps
			}

			if (response.ContainsKey("battle")) //do battle
			{
				int result = (int)response["battle"]; //get battle results
				_context.Dispatcher.Invoke(() => Moves.Battle(result, _selected, target, _context.GameId));
			}
			else _context.Dispatcher.Invoke(() => Moves.MoveTo(_selected, target));

			if (response.ContainsKey("winner"))
				_context.GameOver(true);
			else
				_context.State = new WaitingState(_context);
		}

		public void OnReceivedData(JObject json) { }
	}

	internal class GameOverState : IGameState
	{
		private MainWindow _context;

		public GameOverState(MainWindow context)
		{
			this._context = context;
			_context.StartBtn.IsEnabled = true;

		}

		public void Done()
		{
			//send request new game - if accept new game
		}

		public void OnHoveringEnd(SquareImage squareImage)
		{
			//do nothing
		}

		public void OnHoveringStart(SquareImage squareImage)
		{
			//do nothing
		}

		public void OnReceivedData(JObject json)
		{
			if ((bool)json["accept"])
			{
				_context.Board.Children.Clear(); //clear board
				_context.PrepareSquares(); //prepare from begining
				_context.State = new SelectFlagState(_context); //go to flag state

				_context.StartBtn.IsEnabled = false;
			}
			else
			{
				//refused dialog & exit to lobby
			}
		}

		public void SelectSquare(SquareImage squareImg) { }
	}

	internal static class Moves
	{
		internal static void Battle(int result, SquareImage attacker, SquareImage target, int gameID)
		{
			if (result == -1) Kill(attacker); //attack lost
			else if (result == 1) MoveTo(attacker, target); //attack won
			else if (result == 0) ResolveDraw(attacker, target, gameID); // draw
			else throw new Exception("unkown result"); //other
		}

		internal static void MoveTo(SquareImage selected, SquareImage target)
		{
			target.Image.Source = selected.Image.Source; //move image to target
			selected.Image.Source = ImageFactory.LoadImage(ImageFactory.NONE); //remove image from old position

			target.Square.MyRPS = selected.Square.MyRPS; //make square mine
			target.Square.Type = selected.Square.Type;

			selected.Square.MyRPS = false; //old square not mine
			selected.Square.Type = SquareType.Empty;
		}

		internal static void ResolveDraw(SquareImage selected, SquareImage target, int gameID)
		{
			var draw = new DrawRPS(selected, target, gameID, async result =>
			{
				await Task.Delay(1000);
				Battle(result, selected, target, gameID);
			});

			draw.ShowDialog();
		}

		internal static void Kill(SquareImage square)
		{
			square.Image.Source = ImageFactory.LoadImage(ImageFactory.NONE);
			square.Square.MyRPS = false;
			square.Square.Type = SquareType.Empty;
		}
	}
}
