using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CS6613_Final
{
    [Flags]
    public enum TextAlignment
    {
        Left = 0x01,
        Center = 0x02,
        Right = 0x04
    }

    internal static class Utility
    {
        public static void DrawStringToFitBox(SpriteBatch g, SpriteFont f, Rectangle box, string text,
                                              TextAlignment align, int padding, Color c, Color bg)
        {
            int lcharIndex = 0, rcharIndex = (text.IndexOf(" "));
            string textToWrite = text.Substring(lcharIndex, rcharIndex);
            var position = new Vector2(box.X + padding, box.Y + padding);

            Vector2 fontDimensions = f.MeasureString(textToWrite);
            var newB = new Rectangle(box.Left, box.Top, box.Width, padding);

            //loop through strings' words until width is surpassed, once this happens, iterate backwards to last space found
            //go to next line in box
            var stringsToDraw = new List<string>();

            while (rcharIndex != text.Count())
            {
                int idxOfNewline = -1;

                int lastCharIndex = 0;
                while ((fontDimensions.X < box.Width - padding*2) && rcharIndex != text.Count())
                {
                    lastCharIndex = rcharIndex;

                    if ((rcharIndex = text.IndexOf(" ", rcharIndex + 1)) >= 0)
                        textToWrite = text.Substring(lcharIndex, rcharIndex - lcharIndex);
                    else //no more spaces, hit the end of the string
                    {
                        lastCharIndex = rcharIndex = text.Count();
                        textToWrite = text.Substring(lcharIndex);
                    }

                    idxOfNewline = textToWrite.IndexOf("\n");
                    if (idxOfNewline >= 0)
                    {
                        textToWrite = text.Substring(lcharIndex, idxOfNewline);
                        rcharIndex = lcharIndex + idxOfNewline + 1;

                        fontDimensions = f.MeasureString(textToWrite);
                        lastCharIndex = rcharIndex;
                        break;
                    }


                    fontDimensions = f.MeasureString(textToWrite);
                }

                rcharIndex = lastCharIndex;

                if (idxOfNewline < 0)
                    textToWrite = text.Substring(lcharIndex, rcharIndex - lcharIndex);

                stringsToDraw.Add(textToWrite);

                lcharIndex = rcharIndex;

                newB.Height += (int) fontDimensions.Y;
                fontDimensions = Vector2.Zero;
            }

            g.Draw(CheckersGame.BlankTexture, newB, bg);

            foreach (string s in stringsToDraw)
            {
                fontDimensions = f.MeasureString(s);
                position = new Vector2(box.X + padding, position.Y);
                if ((align & TextAlignment.Center) == TextAlignment.Center)
                    position += new Vector2((int) (box.Width*0.5f - fontDimensions.X*0.5f), 0);
                else if ((align & TextAlignment.Right) == TextAlignment.Right)
                    position += new Vector2(box.Width - fontDimensions.X, 0);

                g.DrawString(f, s, position, c);
                position.Y += fontDimensions.Y;
            }
        }
    }
}