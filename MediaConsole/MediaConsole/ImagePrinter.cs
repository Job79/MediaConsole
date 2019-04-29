using System;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MediaConsole
{
    class ImagePrinter
    {
        /// <summary>
        /// Configure console.
        /// </summary>
        public ImagePrinter(bool colors)
        {
            //Hide cursor to stop flickering.
            Console.CursorVisible = false;

            if (colors)//Enable colors. See: https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
            {
                var handle = GetStdHandle(-11);
                GetConsoleMode(handle, out int mode);
                SetConsoleMode(handle, mode | 0x4);
            }
        }

        /// <summary>
        /// Calculate max image size.
        /// </summary>
        /// <returns>Max image size for the passed variables</returns>
        public Size GetMaxSize(double width, double height, int maxWidth, int maxHeight)
        {
            double wfactor = width / maxWidth;
            double hfactor = height / maxHeight;

            double resizeFactor = Math.Max(wfactor, hfactor);
            return new Size((int)(width / resizeFactor), (int)(height / resizeFactor));
        }

        /// <summary>
        /// Create an console image of the passed image.
        /// </summary>
        /// <returns>The consoleimage with colors</returns>
        public StringBuilder CreateImage(Image plainImage, Size size, int colorDifference)
        {
            /*                              Create resized image.                          */
            using (Bitmap image = new Bitmap(plainImage, size))
            {
                /*                          Convert image.                          */
                StringBuilder builder = new StringBuilder();

                /* Variables used to optimize the drawing of the console image.
                   Colors will not be changed if they are the same of the current colors.*/
                Color topColor = new Color(),
                      bottomColor = new Color();

                for (int y = 0; y < image.Height - 1; y += 2)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color top = image.GetPixel(x, y);//Foreground of '▀'
                        Color bottom = image.GetPixel(x, y + 1);//Background of '▀'

                        /* The number 48 = background.
                           Number 38 = foreground.*/


                        /*                          Change no colors.                      */
                        if (ColorsEqual(top, topColor, colorDifference) && ColorsEqual(bottom, bottomColor, colorDifference))//Write pixels again.
                            builder.Append('▀');
                        else if (ColorsEqual(top, bottomColor, colorDifference) && ColorsEqual(bottom, topColor, colorDifference))//Switch bottom and top.
                            builder.Append('▄');
                        /*                          Change 1 color.                         */
                        else if (ColorsEqual(top, topColor, colorDifference))//Only change the bottom.
                        {
                            builder.Append("\x1b[48;2;");
                            builder.Append(bottom.R);
                            builder.Append(';');
                            builder.Append(bottom.G);
                            builder.Append(';');
                            builder.Append(bottom.B);
                            builder.Append("m▀");

                            bottomColor = bottom;
                        }
                        else if (ColorsEqual(bottom, bottomColor, colorDifference))//Only change the top.
                        {
                            builder.Append("\x1b[38;2;");
                            builder.Append(top.R);
                            builder.Append(';');
                            builder.Append(top.G);
                            builder.Append(';');
                            builder.Append(top.B);
                            builder.Append("m▀");

                            topColor = top;
                        }
                        else if (ColorsEqual(bottom, topColor, colorDifference))//Switch bottom and top and change top.
                        {
                            builder.Append("\x1b[38;2;");
                            builder.Append(top.R);
                            builder.Append(';');
                            builder.Append(top.G);
                            builder.Append(';');
                            builder.Append(top.B);
                            builder.Append("m▄");

                            topColor = top;
                        }
                        else if (ColorsEqual(top, bottomColor, colorDifference))//Switch bottom and top and change bottom.
                        {
                            builder.Append("\x1b[48;2;");
                            builder.Append(bottom.R);
                            builder.Append(';');
                            builder.Append(bottom.G);
                            builder.Append(';');
                            builder.Append(bottom.B);
                            builder.Append("m▄");

                            bottomColor = bottom;
                        }
                        /*                          Change both colors.                         */
                        else
                        {
                            builder.Append("\x1b[38;2;");
                            builder.Append(top.R);
                            builder.Append(';');
                            builder.Append(top.G);
                            builder.Append(';');
                            builder.Append(top.B);
                            builder.Append(";48;2;");
                            builder.Append(bottom.R);
                            builder.Append(';');
                            builder.Append(bottom.G);
                            builder.Append(';');
                            builder.Append(bottom.B);
                            builder.Append("m▀");

                            topColor = top;
                            bottomColor = bottom;
                        }
                    }

                    builder.Append('\n');
                }

                builder.Length--; //Remove last newline.
                return builder;
            }
        }

        /// <summary>
        /// Check if colors are equal,
        /// colorDifference is the max difference between 2 colors.
        /// So a lower value results in better quality.
        /// </summary>
        private bool ColorsEqual(Color c1, Color c2, int colorDifference)
            => Math.Abs(c1.R - c2.R) + Math.Abs(c1.G - c2.G) + Math.Abs(c1.B - c2.B) < colorDifference;

        /// <summary>
        /// Create an console image of the passed image.
        /// </summary>
        /// <param name="pixels">Pixels used to define black and white. Sorted on density</param>
        /// <returns>The consoleimage with colors</returns>
        public StringBuilder CreateBlackWhiteImage(Image plainImage, Size size, char[] pixels)
        {
            /*                              Resize image.                          */
            using (Bitmap image = new Bitmap(plainImage, size))
            {
                /*                          Convert image.                          */
                StringBuilder builder = new StringBuilder();

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        Color pixel = image.GetPixel(x, y);

                        //Get density of the pixel.
                        int densityIndex = ((pixel.R + pixel.G + pixel.B) / 3 * pixels.Length - 1) / 255;

                        //Add pixel with right density.
                        builder.Append(pixels[densityIndex]);
                    }

                    builder.Append('\n');
                }

                builder.Length--;//Remove last newline.
                return builder;
            }
        }

        /* DllImports used for rgb console colors.
         * See: https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
         */
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr handle, out int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int handle);
    }
}
