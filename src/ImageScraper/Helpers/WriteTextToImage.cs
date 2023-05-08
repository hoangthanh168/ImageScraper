using ImageScraper.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImageScraper.Helpers
{
    public class WriteTextToImage : IDisposable
    {
        private Image _image;
        private Bitmap _bitmap;
        private Graphics _graphics;
        private string _inputImagePath;
        private ImageEditorViewModel _imageEditorViewModel;

        public WriteTextToImage(string inputImagePath,ImageEditorViewModel imageEditorViewModel  )
        {
            _inputImagePath = inputImagePath;
            _imageEditorViewModel = imageEditorViewModel;
            Initialize();
        }
        private void Initialize()
        {
            _image = Image.FromFile(_inputImagePath);
            _bitmap = new Bitmap(_image.Width, _image.Height, PixelFormat.Format32bppArgb);
            _graphics = Graphics.FromImage(_bitmap);
            _graphics.SmoothingMode = SmoothingMode.HighQuality;
            _graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            _graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            _graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            _graphics.DrawImage(_image, 0, 0, _image.Width, _image.Height);
        }

        public void WriteTextOnImageAsync(string outputImagePath, string text, Color textColor, string fontWeight, ContentAlignment alignment, string fontName, RectangleF textRectangle = default(RectangleF), string author="",string domain ="")
        {
            if (text != "")
            {
                if (_imageEditorViewModel.IsAutoRandomColor)
                {
                    textColor = RandomColor(new Random());
                }
                string separatedText = SeparateSentences(text);
                StringFormat stringFormat = new StringFormat
                {
                    Alignment = GetStringAlignment(alignment),
                    LineAlignment = StringAlignment.Center,

                };
                int fontSize = 0;
                SizeF textSize;
                Font font;
                if (textRectangle == default(RectangleF))
                {
                    textRectangle = new RectangleF(0, 0, _image.Width, _image.Height);
                }
                do
                {
                    fontSize++;
                    font = new Font(fontName, fontSize, GetFontStyle(fontWeight));
                    textSize = _graphics.MeasureString(separatedText, font, new SizeF(textRectangle.Width, textRectangle.Height), stringFormat);
                } while (textSize.Width < textRectangle.Width * 0.9 && textSize.Height < textRectangle.Height * 0.5);
                if (fontSize > 1)
                {
                    fontSize--;
                    font = new Font(fontName, fontSize, GetFontStyle(fontWeight));
                    // Shadow color and offset defaults
                    var shadowColor = Color.FromArgb(128, 0, 0, 0); // Set default shadow color to semi-transparent black
                    float offsetX = font.Size * 0.05f; // Calculate offset X based on font size
                    float offsetY = font.Size * 0.05f; // Calculate offset Y based on font size
                    var shadowOffset = new PointF(offsetX, offsetY); // Set shadow offset
                    // Draw shadow
                    using (SolidBrush shadowBrush = new SolidBrush(shadowColor))
                    {
                        RectangleF shadowRectangle = new RectangleF(textRectangle.X + shadowOffset.X, textRectangle.Y + shadowOffset.Y, textRectangle.Width, textRectangle.Height);
                        _graphics.DrawString(separatedText, font, shadowBrush, shadowRectangle, stringFormat);
                    }
                    // Draw text
                    using (SolidBrush brush = new SolidBrush(textColor))
                    {
                        _graphics.DrawString(separatedText, font, brush, textRectangle, stringFormat);
                    }


                    // Draw author
                    if (author.Trim() != "")
                    {
                        string authorText = "- " + author + " -";
                        Font authorFont = new Font(fontName, 1, GetFontStyle(fontWeight));
                        int authorFontSize = CalculateFontSize(authorText, authorFont, stringFormat, _image.Width, 0.15f);
                        authorFont = new Font(fontName, authorFontSize, GetFontStyle(fontWeight));
                        SizeF authorSize = _graphics.MeasureString(authorText, authorFont);

                        RectangleF authorRectangle = new RectangleF((textRectangle.Width - authorSize.Width) / 2, textRectangle.Height - authorSize.Height - 10, authorSize.Width, authorSize.Height);
                        // Draw author
                        using (SolidBrush authorBrush = new SolidBrush(textColor))
                        {
                            _graphics.DrawString(authorText, authorFont, authorBrush, authorRectangle, stringFormat);
                        }
                    }

                    // Draw domain
                    if (domain != "")
                    {
                        string domainText = domain;
                        Font domainFont = new Font(fontName, 1, GetFontStyle(fontWeight));
                        int domainFontSize = CalculateFontSize(domainText, domainFont, stringFormat, _image.Width, 0.2f);
                        domainFont = new Font(fontName, domainFontSize, GetFontStyle(fontWeight));
                        SizeF domainSize = _graphics.MeasureString(domainText, domainFont);

                        float paddingPercentage = 0.01f;
                        float paddingHorizontal = _image.Width * paddingPercentage;
                        float paddingVertical = _image.Height * paddingPercentage;
                        RectangleF domainRectangle = new RectangleF(_image.Width - domainSize.Width - paddingHorizontal, paddingVertical, domainSize.Width + paddingHorizontal, domainSize.Height + paddingVertical); // Draw domain
                        using (SolidBrush domainBrush = new SolidBrush(textColor))
                        {
                            _graphics.DrawString(domainText, domainFont, domainBrush, domainRectangle, stringFormat);
                        }
                    }
                }
            }
            using (EncoderParameters encoderParameters = new EncoderParameters(1))
            using (EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, 80L)) // Set quality to 100
            {
                encoderParameters.Param[0] = qualityParam;
                ImageCodecInfo pngCodec = GetEncoderInfo("image/jpeg");
                _bitmap.Save(outputImagePath, pngCodec, encoderParameters);

            }
        }
        int CalculateFontSize(string text, Font font, StringFormat stringFormat, float maxWidth, float scaleFactor)
        {
            int fontSize = 1;
            SizeF textSize;
            do
            {
                fontSize++;
                font = new Font(font.FontFamily, fontSize, font.Style);
                textSize = _graphics.MeasureString(text, font, new SizeF(maxWidth, float.MaxValue), stringFormat);
            } while (textSize.Width < maxWidth * scaleFactor);

            return fontSize > 1 ? fontSize - 1 : fontSize;
        }

        public void Dispose()
        {
            _graphics?.Dispose();
            _bitmap?.Dispose();
            _image?.Dispose();
        }
        private string SeparateSentences(string inputText)
        {
            string[] sentences = Regex.Split(inputText, @"(?<=[.,])\s+");
            return string.Join(Environment.NewLine, sentences);
        }
        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            for (int i = 0; i < codecs.Length; i++)
            {
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];
            }
            return null;
        }
        private StringAlignment GetStringAlignment(ContentAlignment alignment)
        {
            switch (alignment)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.BottomLeft:
                    return StringAlignment.Near;

                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    return StringAlignment.Center;

                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    return StringAlignment.Far;

                default:
                    return StringAlignment.Near;
            }
        }
        private System.Drawing.FontStyle GetFontStyle(string fontWeightString)
        {
            if (Enum.TryParse(fontWeightString, true, out FontWeight fontWeight))
            {
                switch (fontWeight)
                {
                    case FontWeight.Regular:
                        return System.Drawing.FontStyle.Regular;

                    case FontWeight.Bold:
                        return System.Drawing.FontStyle.Bold;

                    case FontWeight.Italic:
                        return System.Drawing.FontStyle.Italic;

                    case FontWeight.BoldItalic:
                        return System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic;

                    default:
                        return System.Drawing.FontStyle.Regular;
                }
            }
            else
            {
                // Handle the case where the input string cannot be converted to a FontWeight value
                // You may return a default value or throw an exception, depending on your requirements
                return System.Drawing.FontStyle.Regular;
            }
        }
        Color RandomColor(Random random)
        {
            Color randomColor;
            int r = random.Next(0, 256); // Red component
            int g = random.Next(0, 256); // Green component
            int b = random.Next(0, 256); // Blue component
            randomColor = Color.FromArgb(r, g, b);
            return randomColor;
        }
        public static System.Drawing.Color ConvertMediaColorToDrawingColor(System.Windows.Media.Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }
    }
    public enum FontWeight
    {
        Regular,
        Bold,
        Italic,
        BoldItalic
    }
}