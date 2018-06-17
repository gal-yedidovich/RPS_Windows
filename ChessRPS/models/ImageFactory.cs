using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Client.models;

namespace Client
{
	public class ImageFactory
	{
		public static readonly string BLUE = "/images/blue.png",
										BLACK = "/images/black.png",
										RED = "/images/red.png",
										NONE = "/images/transparent.png",
										BLUE_TRAP = "/images/b_trap.png",
										BLUE_FLAG = "/images/b_flag.png",
										BLUE_ROCK = "/images/b_rock.png",
										BLUE_SCISSORS = "/images/b_scissors.png",
										BLUE_PAPER = "/images/b_paper.png",
										RED_TRAP = "/images/r_trap.png",
										RED_FLAG = "/images/r_flag.png",
										RED_ROCK = "/images/r_rock.png",
										RED_SCISSORS = "/images/r_scissors.png",
										RED_PAPER = "/images/r_paper.png";


		private static readonly Dictionary<string, BitmapImage> _CACHE = new Dictionary<string, BitmapImage>();
		public static ImageSource LoadImage(string path)
		{
			if (!_CACHE.ContainsKey(path)) _CACHE[path] = new BitmapImage(new Uri("pack://application:,,," + path));

			return _CACHE[path];
		}

		internal static ImageSource LoadBlueImage(SquareType type)
		{
			string path;
			switch (type)
			{
				case SquareType.Flag:
					path = BLUE_FLAG;
					break;
				case SquareType.Trap:
					path = BLUE_TRAP;
					break;
				case SquareType.Rock:
					path = BLUE_ROCK;
					break;
				case SquareType.Paper:
					path = BLUE_PAPER;
					break;
				case SquareType.Scissors:
					path = BLUE_SCISSORS;
					break;
				default: throw new ArgumentException("invalid type");
			}
			return LoadImage(path);
		}
	}
}