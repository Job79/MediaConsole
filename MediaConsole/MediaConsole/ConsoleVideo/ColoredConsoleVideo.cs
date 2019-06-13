using System;
using System.Drawing;
using System.Diagnostics;
using Accord.Video.FFMPEG;

namespace MediaConsole
{
    class ColoredConsoleVideo
    {
        public void Play(string filePath, int maxColorDifference, int maximumMaxColorDifference)
        {
            using (var videoReader = new VideoFileReader())
            {
                videoReader.Open(filePath);

                //Variables used to display frames.
                var consoleImage = new ColoredConsoleImage();
                int consoleWidth = Console.WindowWidth,
                    consoleHeight = Console.WindowHeight;

                int colorDifference = (int)(maximumMaxColorDifference * 0.25d);//75%
                Size videoSize = new Size(Console.WindowWidth - 1, Console.WindowHeight * 2);

                //Variables used to calculate fps.
                double framesCounter = 0;
                var st = new Stopwatch();
                st.Start();

                for (int frameIndex = 0; frameIndex < videoReader.FrameCount; frameIndex++, framesCounter++)
                {
                    /*                          Handle user input.                          */
                    if (Console.KeyAvailable)//When any key is pressed.
                    {
                        switch (Console.ReadKey(true).Key)
                        {
                            case ConsoleKey.RightArrow:
                                frameIndex += (int)videoReader.FrameRate.Value * 5;//Skip 5 sec.
                                break;
                            case ConsoleKey.LeftArrow:
                                frameIndex -= (int)videoReader.FrameRate.Value * 5;//Go back 5 sec.
                                break;
                            case ConsoleKey.UpArrow://Increase quality.
                                if (colorDifference > maxColorDifference)
                                    colorDifference -= 2;
                                break;
                            case ConsoleKey.DownArrow://Decrease quality.
                                if (colorDifference < maximumMaxColorDifference)
                                    colorDifference += 2;
                                break;
                            case ConsoleKey.Spacebar:
                                st.Stop();
                                while (Console.ReadKey(true).Key != ConsoleKey.Spacebar) { }//Wait until space is pressed again.
                                st.Start();
                                break;
                            case ConsoleKey.C://Exit.
                                return;
                        }
                    }

                    /*                          Display fps.                        */
                    if (st.ElapsedMilliseconds >= 500)
                    {
                        double fps = framesCounter / (st.ElapsedMilliseconds/1000d);

                        //Auto increase/decrease color quality.
                        if (fps < 10 && colorDifference < maximumMaxColorDifference)//Decrease quality.
                            colorDifference += 2;
                        else if (fps > 20 && colorDifference > maxColorDifference)//Increase quality.
                            colorDifference -= 2;

                        double colorDifferencePercentage = 100d / (maxColorDifference - maximumMaxColorDifference) * (colorDifference - maximumMaxColorDifference);
                        Console.Title = $"{Math.Round(fps, 2)}fps  Color quality: {Math.Round(colorDifferencePercentage, 1)}%";

                        if (consoleWidth != Console.WindowWidth || consoleHeight != Console.WindowHeight)//When window is resized.
                        {
                            Console.Clear();
                            Console.CursorVisible = false;

                            videoSize = new Size(Console.WindowWidth - 1, Console.WindowHeight * 2);
                            consoleWidth = Console.WindowWidth;
                            consoleHeight = Console.WindowHeight;
                        }

                        framesCounter = 0;
                        st.Restart();
                    }

                    /*                          Display frame.                          */
                    using (var plainFrame = videoReader.ReadVideoFrame(frameIndex))
                        using (Bitmap frame = new Bitmap(plainFrame, videoSize))
                            Console.Write(consoleImage.Create(frame, colorDifference));

                    Console.SetCursorPosition(0, 0);
                }
            }
        }
    }
}
