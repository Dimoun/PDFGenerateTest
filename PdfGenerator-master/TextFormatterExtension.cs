using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfGenerator
{
    public class TextFormatterExtension : XTextFormatter
    {
        public TextFormatterExtension(XGraphics gfx) : base(gfx) { }

        public override void DrawString(string text, XFont font, XBrush brush, XRect layoutRectangle, XStringFormat format)
        {
            int startIndex = 0;
            double currentWidth = 0;

            for (int i = 0; i < text.Length; i++)
            {
                string currentChar = text[i].ToString();
                double charWidth = gfx.MeasureString(currentChar, font).Width;

                if (currentWidth + charWidth > layoutRectangle.Width)
                {
                    string line = text.Substring(startIndex, i - startIndex);
                    base.DrawString(line, font, brush, layoutRectangle, format);

                    layoutRectangle.Y += font.Height;
                    startIndex = i;
                    currentWidth = 0;
                }

                currentWidth += charWidth;
            }

            if (startIndex < text.Length)
            {
                string remainingText = text.Substring(startIndex);
                base.DrawString(remainingText, font, brush, layoutRectangle, format);
            }
        }
    }
}
