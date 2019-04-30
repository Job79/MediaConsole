using System;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MediaConsole
{
    class ColoredConsoleImage
    {
        public ColoredConsoleImage()
        {
            //Hide cursor to stop flickering.
            Console.CursorVisible = false;

            //Enable rgb colors for this console window.
            var handle = GetStdHandle(-11);
            GetConsoleMode(handle, out int mode);
            SetConsoleMode(handle, mode | 0x4);
        }

        /// <summary>
        /// Create a new colored console image.
        /// </summary>
        /// <param name="maxColorDifference">Max difference between 2 colors</param>
        /// <returns>Created console image</returns>
        public StringBuilder Create(Bitmap image, int maxColorDifference)
        {
            StringBuilder builder = new StringBuilder();

            Color currentTop = new Color(),
                  currentBottom = new Color();

            for (int y = 0; y < image.Height - 1; y += 2)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color top = image.GetPixel(x, y),//Foreground of '▀' (38)
                          bottom = image.GetPixel(x, y + 1);//Background of '▀' (48)

                    /*                          Change no colors.                      */
                    if (colorsEqual(top, currentTop, maxColorDifference) &&
                        colorsEqual(bottom, currentBottom, maxColorDifference))//Use same pixels again.
                        builder.Append('▀');
                    else if (colorsEqual(top, currentBottom, maxColorDifference) &&
                             colorsEqual(bottom, currentTop, maxColorDifference))//Switch bottom and top.
                        builder.Append('▄');
                    /*                          Change 1 color.                         */
                    else if (colorsEqual(top, currentTop, maxColorDifference))//Only change the bottom.
                    {
                        builder.Append("\x1b[48;2;");
                        builder.Append(bottom.R);
                        builder.Append(';');
                        builder.Append(bottom.G);
                        builder.Append(';');
                        builder.Append(bottom.B);
                        builder.Append("m▀");

                        currentBottom = bottom;
                    }
                    else if (colorsEqual(bottom, currentBottom, maxColorDifference))//Only change the top.
                    {
                        builder.Append("\x1b[38;2;");
                        builder.Append(top.R);
                        builder.Append(';');
                        builder.Append(top.G);
                        builder.Append(';');
                        builder.Append(top.B);
                        builder.Append("m▀");

                        currentTop = top;
                    }
                    else if (colorsEqual(bottom, currentTop, maxColorDifference))//Switch bottom and top and change top.
                    {
                        builder.Append("\x1b[38;2;");
                        builder.Append(top.R);
                        builder.Append(';');
                        builder.Append(top.G);
                        builder.Append(';');
                        builder.Append(top.B);
                        builder.Append("m▄");

                        currentTop = top;
                    }
                    else if (colorsEqual(top, currentBottom, maxColorDifference))//Switch bottom and top and change bottom.
                    {
                        builder.Append("\x1b[48;2;");
                        builder.Append(bottom.R);
                        builder.Append(';');
                        builder.Append(bottom.G);
                        builder.Append(';');
                        builder.Append(bottom.B);
                        builder.Append("m▄");

                        currentBottom = bottom;
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

                        currentTop = top;
                        currentBottom = bottom;
                    }
                }

                builder.Append('\n');
            }

            builder.Length--;//Remove last newline.
            return builder;
        }

        /// <summary>
        /// Check if 2 colors are equal with a maxDifference value.
        /// </summary>
        /// <returns>true = colors equal</returns>
        private bool colorsEqual(Color c1, Color c2, int maxColorDifference)
            => Math.Abs(c1.R - c2.R) + Math.Abs(c1.G - c2.G) + Math.Abs(c1.B - c2.B) < maxColorDifference;


        /* DllImports used for rgb console colors.
         * See: https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences */
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetConsoleMode(IntPtr handle, out int mode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int handle);
    }
}
