using System;
using System.IO;
using System.Text;
using System.Drawing;//!References>System.Drawing!
using System.Diagnostics;
using Accord.Video.FFMPEG;

namespace MediaConsole
{
    class Program
    {
        /// <summary>
        /// Max size of an image.
        /// </summary>
        static readonly int
            MaxWidth = Console.LargestWindowWidth,
            MaxHeight = Console.LargestWindowHeight;

        /// <summary>
        /// Color quality for the videos.
        /// Console will not change the color if color is equal, which is much faster.
        /// The color difference is the max difference(r + g + g) between the current pixel and the next pixel.
        /// </summary>
        const int minColorDifference = 0,
                  maxColorDifference = 50;

        /// <summary>
        /// Default pixels used for black/white images and video's. Sorted on density.
        /// </summary>
        static readonly char[] DefaultPixels = new char[] { '█', '█', '▓', '▓', '▒', '░', ' ', ' ' };

        static void Main(string[] args)
        {
            /*                          Process args.                           */
            if (args.Length <= 0 || !File.Exists(args[0]))//Check file.
            {
                Console.WriteLine("Usage: MediaConsole.exe {path to image/video} Colors:[on|off] Background:[dark|white] [Pixels]");
                return;
            }

            bool colors = true;//Check colors.
            if (args.Length >= 2 && args[1].Equals("on", StringComparison.CurrentCultureIgnoreCase))
                colors = true;
            else if (args.Length >= 2 && args[1].Equals("off", StringComparison.CurrentCultureIgnoreCase))
                colors = false;

            //Change colors for a black/white image only.
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

            char[] pixels = null;//Change pixels for a black/white image only.
            if (!colors && args.Length >= 4)
            {
                pixels = new char[args[3].Length];
                for (int i = 0; i < pixels.Length; i++)//Copy all chars to pixels.
                    pixels[i] = args[3][i];
            }
            else if (!colors)//Use default pixels.
                pixels = DefaultPixels;

            //Display image or play video.
            if (Path.GetExtension(args[0]).Equals(".mp4"))
                PlayMP4(args[0], colors, pixels);
            else
                PrintImage(args[0], colors, pixels);

            Console.ReadLine();
            Console.Clear();
        }

        /// <summary>
        /// Print an image to the console.
        /// </summary>
        /// <param name="filePath">Path to the location of the picture</param>
        /// <param name="colors">off = black/white image</param>
        /// <param name="pixels">Pixels used when creating a black/white image. Sorted on density</param>
        static void PrintImage(string filePath, bool colors, char[] pixels)
        {
            using (Image image = Image.FromFile(filePath))
            {
                ImagePrinter printer = new ImagePrinter(colors);

                if (colors)
                {
                    Size maxSize = printer.GetMaxSize(image.Width, image.Height, MaxWidth, MaxHeight * 2);

                    Console.SetWindowSize(maxSize.Width, maxSize.Height / 2);//Set window to the right size.
                    StringBuilder consoleImage = printer.CreateImage(image, maxSize, minColorDifference);//Create image with max quality.
                    Console.Write(consoleImage);//Print image to console.
                }
                else
                {
                    Size maxSize = printer.GetMaxSize(image.Width, image.Height / 2, MaxWidth - 1, MaxHeight);

                    Console.SetWindowSize(maxSize.Width + 1, maxSize.Height);//Set window to the right size.
                    StringBuilder consoleImage = printer.CreateBlackWhiteImage(image, maxSize, pixels);//Create image.
                    Console.Write(consoleImage);//Print image to console.
                }
            }
        }

        /// <summary>
        /// Play an video file in the console.
        /// </summary>
        /// <param name="filePath">Path to the video</param>
        /// <param name="colors">off = black/white frames</param>
        /// <param name="pixels">Pixels used when creating a black/white frame. Sorted on density</param>
        static void PlayMP4(string filePath, bool colors, char[] pixels)
        {
            Console.Write("Press any key to start...");
            Console.ReadKey();

            using (var videoReader = new VideoFileReader())
            {
                videoReader.Open(filePath);

                //Variables used to display frames.
                ImagePrinter printer = new ImagePrinter(colors);
                int width = Console.WindowWidth,
                    height = Console.WindowHeight;

                int colorDifference = (int)(maxColorDifference * 0.25d);//75%
                Size videoSize = colors ?
                    new Size(Console.WindowWidth, Console.WindowHeight * 2)://Black/white.
                    new Size(Console.WindowWidth - 1, Console.WindowHeight);//With colors.

                int colorQuality = (minQuality + maxQuality) / 2;

                //Variables used to calculate fps.
                double framesCounter = 0;
                var st = new Stopwatch();
                st.Start();

                for (long frame = 0; frame < videoReader.FrameCount; frame++, framesCounter++)
                {
                    if (st.ElapsedMilliseconds >= 500)//Update fps every 0, 5 second.
                    {
                        double fps = framesCounter / (st.ElapsedMilliseconds / 1000d);

                        if (colors)//Also show and calculate colorQuality.
                        {
                            if (fps < 10 && colorDifference < maxColorDifference)//Decrease quality.
                                colorDifference+=2;
                            else if (fps > 20 && colorDifference > minColorDifference)//Increase quality.
                                colorDifference-=2;

                            double colorQualityPercentage = 100d / (minColorDifference - maxColorDifference) * (colorDifference - maxColorDifference);//Calculate colorQualityPercentage.
                            Console.Title = $"{Math.Round(fps, 1)}fps  Color quality: {Math.Round(colorQualityPercentage, 1)}%";
                        }
                        else
                            Console.Title = $"{Math.Round(fps, 1)}fps";

                        if (width != Console.WindowWidth || height != Console.WindowHeight)//When window is resized.
                        {
                            Console.Clear();
                            Console.CursorVisible = false;

                            videoSize = colors ?
                                new Size(Console.WindowWidth, Console.WindowHeight * 2) ://Black/white.
                                new Size(Console.WindowWidth - 1, Console.WindowHeight);//With colors.

                            width = Console.WindowWidth;
                            height = Console.WindowHeight;
                        }

                        framesCounter = 0;
                        st.Restart();
                    }

                    //Display frame.
                    if (colors)
                    {
                        StringBuilder consolImage = printer.CreateImage(videoReader.ReadVideoFrame(), videoSize, colorDifference);//Create image.
                        Console.Write(consolImage);//Display image.
                    }
                    else
                    {
                        StringBuilder consoleImage = printer.CreateBlackWhiteImage(videoReader.ReadVideoFrame(), videoSize, pixels);//Create image.
                        Console.Write(consoleImage);//Display image.
                    }

                    Console.SetCursorPosition(0, 0);//Go back to 0,0 to redraw next frame.
                }
            }
        }
    }
}
