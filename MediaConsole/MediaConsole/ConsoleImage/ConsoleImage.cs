using System;
using System.Text;
using System.Drawing;

namespace MediaConsole
{
    class ConsoleImage
    {
        public ConsoleImage() =>
            Console.CursorVisible = false;//Hide cursor to stop flickering.

        /// <summary>
        /// Create an new console image.
        /// </summary>
        /// <param name="pixels">Pixels sorted on density</param>
        /// <returns>Created console image</returns>
        public StringBuilder Create(Bitmap image, char[] pixels)
        {
            StringBuilder builder = new StringBuilder();

            for (int y = 0; y < image.Height - 1; y += 2)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y),
                          pixel2 = image.GetPixel(x, y);

                    //Get density of the pixel.
                    int densityIndex = ((pixel.R + pixel2.R + pixel.G + pixel2.G + pixel.B + pixel2.B) / 6 * pixels.Length - 1) / 255;

                    //Add pixel with right density.
                    builder.Append(pixels[densityIndex]);
                }

                builder.Append('\n');
            }

            builder.Length--;//Remove last newline.
            return builder;
        }
    }
}
