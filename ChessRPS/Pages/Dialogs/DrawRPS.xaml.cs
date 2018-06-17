using Client;
using Client.ViewModels;
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
    public partial class DrawRPS : Window
    {
        internal IState State { get; set; }
        internal SquareImage[] Opponent { get; set; }
        internal SquareImage Attacker { get; set; }
        internal SquareImage Target { get; set; }

        public DrawRPS()
        {
            InitializeComponent();
            InitBoard();
        }

        private void InitBoard()
        {

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
    }
}
