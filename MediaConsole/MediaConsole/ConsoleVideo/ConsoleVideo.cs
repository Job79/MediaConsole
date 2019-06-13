using System;
using System.Drawing;
using System.Diagnostics;
using Accord.Video.FFMPEG;

namespace MediaConsole
{
    class ConsoleVideo
    {
        public void Play(string filePath, char[] pixels)
        {
            using (var videoReader = new VideoFileReader())
            {
                videoReader.Open(filePath);

                //Variables used to display frames.
                var consoleImage = new ConsoleImage();
                int consoleWidth = Console.WindowWidth,
                    consoleHeight = Console.WindowHeight;

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
                        double fps = framesCounter / (st.ElapsedMilliseconds / 1000d);
                        Console.Title = $"{Math.Round(fps, 2)}fps";

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
                        Console.Write(consoleImage.Create(frame, pixels));

                    Console.SetCursorPosition(0, 0);
                }
            }
        }
    }
}
