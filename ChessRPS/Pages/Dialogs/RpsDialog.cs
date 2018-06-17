using ChessRPS;
using Client.models;
using Client.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Client
{
    class RpsDialog
    {
        public MainPage Page { get; }

        public ContentDialog Dialog { get; set; }
        public IState State { get; set; }
        public SquareImage[] Opponent { get; set; }
        internal SquareImage Attacker { get; set; }
        internal SquareImage Target { get; set; }

        public RpsDialog(MainPage page, SquareImage attacker, SquareImage target)
        {
            Dialog = new ContentDialog() { Content = PrepareDialogContent() };
            State = new ChoosingState(this);
            Page = page;
            Attacker = attacker;
            Target = target;
        }

        private object PrepareDialogContent()
        {
            var grid = new Grid();
            for (int i = 0; i < 3; i++) grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < 2; i++) grid.RowDefinitions.Add(new RowDefinition());

            string[] paths = { ImageFactory.RED_ROCK, ImageFactory.RED_PAPER, ImageFactory.RED_SCISSORS };

            Opponent = new SquareImage[3];

            for (int i = 0; i < 3; i++)
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
                grid.Children.Add(button);
            }

            SquareImage[] squares = new SquareImage[3];

            for (int i = 0; i < 3; i++)
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
                grid.Children.Add(button);
            }

            return grid;
        }

        private static (Button button, Border border, Image img) CreateView(string path)
        {
            var img = new Image
            {
                Source = ImageFactory.LoadImage(path) //0, 0, 0, 1, 2, 3
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

        public IAsyncOperation<ContentDialogResult> ShowAsync() => Dialog.ShowAsync();
    }

    interface IState
    {
        void OnSelecting(SquareImage squareImage);
    }

    class ChoosingState : IState
    {
        private RpsDialog _context;

        public ChoosingState(RpsDialog context)
        {
            _context = context;
        }

        public void OnSelecting(SquareImage squareImage)
        {
            squareImage.Button.Background = new SolidColorBrush(Colors.Green);
            _context.State = new ResolvingState(_context, squareImage);
        }
    }

    internal class ResolvingState : IState
    {
        private RpsDialog _context;
        private SquareImage _selected;

        public ResolvingState(RpsDialog context, SquareImage squareImage)
        {
            _context = context;
            _selected = squareImage;

            ReceiveOpponentSelection();
        }

        private async void ReceiveOpponentSelection()
        {
            //mock selecting opponent
            int index = new Random().Next(3);
            await Task.Delay(100);

            string[] rps = { ImageFactory.BLUE_ROCK, ImageFactory.BLUE_PAPER, ImageFactory.BLUE_SCISSORS };

            var op_selected = _context.Opponent[index];
            op_selected.Image.Source = ImageFactory.LoadImage(rps[index]);
            _context.Opponent[(index + 1) % 3].Button.Visibility = Visibility.Collapsed;
            _context.Opponent[(index + 2) % 3].Button.Visibility = Visibility.Collapsed;

            await Task.Delay(500); //delay for UX
            _context.Dialog.Hide();

            if (_context.Attacker.Square.MyRPS) // i'm attacking
            {
                _context.Attacker.Image.Source = _selected.Image.Source;
                _context.Attacker.Square.Type = _selected.Square.Type;

                _context.Target.Image.Source = op_selected.Image.Source;
                _context.Target.Square.Type = op_selected.Square.Type;
            }
            else
            {
                _context.Attacker.Image.Source = op_selected.Image.Source;
                _context.Attacker.Square.Type = op_selected.Square.Type;

                _context.Target.Image.Source = _selected.Image.Source;
                _context.Target.Square.Type = _selected.Square.Type;
            }
        }

        public void OnSelecting(SquareImage squareImage)
        {
            //do nothing
        }
    }
}
