using ChessRPS.Utils;
using Client;
using Client.models;
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
    /// <summary>
    /// Interaction logic for DrawRPS.xaml
    /// </summary>
    public partial class DrawRPS
    {
        private IDialogState State { get; set; }
        internal SquareImage[] Opponent { get; set; }
        internal SquareImage Attacker { get; set; }
        internal SquareImage Target { get; set; }

        public DrawRPS(SquareImage attacker, SquareImage target)
        {
            InitializeComponent();

            State = new ChoosingState(this);
            Attacker = attacker;
            Target = target;

            GameSocket.Instance.OnBroadcast += OnReceive;
            InitBoard();
        }

        private void OnReceive(JObject json)
        {
            string selection = (string)json["selection"];

            State.OnOpponentSelected(selection);
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
                    var (button, border, img) = CreateView(paths[i]);

                    squares[i] = new SquareImage()
                    {
                        Button = button,
                        Image = img,
                        Border = border,
                        Square = new Square(1, i)
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
                    Grid.SetRow(button, 1);
                    rootView.Children.Add(button);
                }
            }
        }

        private static (Button button, Border border, Image img) CreateView(string path)
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

        #region States
        private interface IDialogState
        {
            void OnSelecting(SquareImage squareImage);
            void OnOpponentSelected(string selected);
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

            public void OnOpponentSelected(string selected)
            {
                opponentSelection = selected;
            }

            public void OnSelecting(SquareImage squareImage)
            {
                squareImage.Button.Background = new SolidColorBrush(Colors.Green);
                if (opponentSelection != null) //opponent has selected
                {

                }
            }
        }

        /// <summary>
        /// state definition, for waiting for opponent after i've selected
        /// </summary>
        private class WaitingState : IDialogState
        {
            public void OnOpponentSelected(string selected)
            {
                //
            }

            public void OnSelecting(SquareImage squareImage)
            {
                //
            }
        }

        private async void ResolveDraw(string oppSelection, int result)
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
            Opponent[(index + 1) % 3].Button.Visibility = Visibility.Collapsed;
            Opponent[(index + 2) % 3].Button.Visibility = Visibility.Collapsed;

            Opponent[index].Image.Source = ImageFactory.LoadBlueImage(type);
            await Task.Delay(500); //for UX

            if (Attacker.Square.MyRPS) //im attacking
            {
                Attacker.Image.Source = Attacker.Image.Source;
                Attacker.Square.Type = Attacker.Square.Type;

                Target.Image.Source = Target.Image.Source;
                Target.Square.Type = Target.Square.Type;
            }
            else
            {
                Attacker.Image.Source = Target.Image.Source;
                Attacker.Square.Type = Target.Square.Type;

                Target.Image.Source = Attacker.Image.Source;
                Target.Square.Type = Attacker.Square.Type;
            }

            MainWindowStates.Moves.Battle(result, Attacker, Target);
            this.Close();
        }
        #endregion

        protected override void OnClosed(EventArgs e)
        {
            GameSocket.Instance.OnBroadcast -= OnReceive;
            base.OnClosed(e);
        }
    }
}
