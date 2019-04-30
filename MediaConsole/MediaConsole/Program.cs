using System;
using System.IO;
using System.Drawing;//!References>System.Drawing!

namespace MediaConsole
{
    class Program
    {
        /// <summary>
        /// Max differences between two colors.
        /// Difference is calculated like this:
        /// (red1 - red2) + (green1 - green2) + (blue1 - blue2)
        /// </summary>
        const int maxColorDifferenceImage = 0;
        const int maxColorDifferenceVideo = 0;
        const int maximumMaxColorDifferenceVideo = 50;

        /// <summary>
        /// Default pixels for a black/white image.
        /// </summary>
        static readonly char[] DefaultPixels = { '█', '█', '▓', '▓', '▒', '░', ' ', ' ' };

        static void Main(string[] args)
        {
            /*                          Process args.                           */
            if (args.Length <= 0 || !File.Exists(path:args[0]))//Check if image/video exists.
            {
                Console.WriteLine("Usage: MediaConsole.exe {path to image/video} Colors:[on|off] Background:[dark|white] [Pixels]");
                return;
            }

            //Check colors.
            bool colors =
                args.Length < 2 ||
                args[1].Equals("on", StringComparison.CurrentCultureIgnoreCase);

            //Change colors of the console, for a black/white image only.
            if (!colors && args.Length >= 3 && args[2].Equals("black", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
            }

            char[] pixels;//Change pixels, for a black/white image only.
            if (!colors && args.Length >= 4)
            {
                pixels = new char[args[3].Length];
                for (int i = 0; i < pixels.Length; i++)//Copy all chars to pixels.
                    pixels[i] = args[3][i];
            }
            else//Use default pixels.
                pixels = DefaultPixels;

            /*                      Display image/Play video.                           */
            if (Path.GetExtension(path:args[0]).Equals(".mp4"))
            {
                if (colors)
                    new ColoredConsoleVideo().Play(filePath: args[0], maxColorDifferenceVideo, maximumMaxColorDifferenceVideo);
                else
                    new ConsoleVideo().Play(filePath: args[0],pixels);
            }
            else do
                {
                    Console.Clear();

                    using (Image image = Image.FromFile(filename:args[0]))
                    {
                        Size imageSize = GetMaxSize(image.Width, image.Height,Console.WindowWidth - 1,Console.WindowHeight * 2);
                        Console.WindowHeight = imageSize.Height / 2;//1 pixel is 50% of a row.

                        using (Bitmap resizedImage = new Bitmap(image, imageSize))
                        {
                            if (colors)
                            {
                                var consoleImage = new ColoredConsoleImage();
                                Console.Write(consoleImage.Create(resizedImage, maxColorDifferenceImage));
                            }
                            else
                            {
                                var consoleImage = new ConsoleImage();
                                Console.Write(consoleImage.Create(resizedImage, pixels));
                            }
                        }
                    }
                } while (Console.ReadKey().Key != ConsoleKey.C);

            Console.Clear();
        }

        /// <summary>
        /// Calculate max image size.
        /// </summary>
        static Size GetMaxSize(double width, double height, double maxWidth, double maxHeight)
        {
            double wfactor = width / maxWidth;
            double hfactor = height / maxHeight;

            double resizeFactor = Math.Max(wfactor, hfactor);
            return new Size((int)(width / resizeFactor), (int)(height / resizeFactor));
        }
    }
}
