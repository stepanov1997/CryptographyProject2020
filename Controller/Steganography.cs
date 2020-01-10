using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptographyProject2019.Controller
{
    class Steganography
    {
        enum State
        {
            Hiding,
            FillWithZeros
        };

        /// <summary>
        /// Creates a bitmap from an image without indexed pixels
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static Bitmap CreateNonIndexedImage(Image src)
        {
            Bitmap newBmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics gfx = Graphics.FromImage(newBmp))
            {
                gfx.DrawImage(src, 0, 0);
            }

            return newBmp;
        }

        public static Bitmap MergeText(string text, Bitmap bmp)
        {
            State s = State.Hiding;

            int charIndex = 0;
            int charValue = 0;
            long colorUnitIndex = 0;

            int zeros = 0;

            int R = 0, G = 0, B = 0;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    pixel = Color.FromArgb(pixel.R - pixel.R % 2,
                        pixel.G - pixel.G % 2, pixel.B - pixel.B % 2);

                    R = pixel.R; G = pixel.G; B = pixel.B;

                    for (int n = 0; n < 3; n++)
                    {
                        if (colorUnitIndex % 8 == 0)
                        {
                            if (zeros == 8)
                            {
                                if ((colorUnitIndex - 1) % 3 < 2)
                                {
                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }

                                return bmp;
                            }

                            if (charIndex >= text.Length)
                            {
                                s = State.FillWithZeros;
                            }
                            else
                            {
                                charValue = text[charIndex++];
                            }
                        }

                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    if (s == State.Hiding)
                                    {
                                        R += charValue % 2;

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (s == State.Hiding)
                                    {
                                        G += charValue % 2;

                                        charValue /= 2;
                                    }
                                }
                                break;
                            case 2:
                                {
                                    if (s == State.Hiding)
                                    {
                                        B += charValue % 2;

                                        charValue /= 2;
                                    }

                                    bmp.SetPixel(j, i, Color.FromArgb(R, G, B));
                                }
                                break;
                        }

                        colorUnitIndex++;

                        if (s == State.FillWithZeros)
                        {
                            zeros++;
                        }
                    }
                }
            }

            return bmp;
        }

        public static string ExtractText(Bitmap bmp)
        {
            int colorUnitIndex = 0;
            int charValue = 0;

            string extractedText = String.Empty;

            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Color pixel = bmp.GetPixel(j, i);

                    for (int n = 0; n < 3; n++)
                    {
                        switch (colorUnitIndex % 3)
                        {
                            case 0:
                                {
                                    charValue = charValue * 2 + pixel.R % 2;
                                }
                                break;
                            case 1:
                                {
                                    charValue = charValue * 2 + pixel.G % 2;
                                }
                                break;
                            case 2:
                                {
                                    charValue = charValue * 2 + pixel.B % 2;
                                }
                                break;
                        }

                        colorUnitIndex++;

                        if (colorUnitIndex % 8 == 0)
                        {
                            charValue = reverseBits(charValue);

                            if (charValue == 0)
                            {
                                return extractedText;
                            }

                            char c = (char)charValue;

                            extractedText += c.ToString();
                        }
                    }
                }
            }

            return extractedText;
        }

        public static int reverseBits(int n)
        {
            int result = 0;

            for (int i = 0; i < 8; i++)
            {
                result = result * 2 + n % 2;

                n /= 2;
            }

            return result;
        }
    }
}
