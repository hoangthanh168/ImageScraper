using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;
using System;
using System.IO;
using System.Windows.Controls;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Input;

namespace ImageScraper.Views
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            InitializeComponent();
            DebugLogs.TextArea.TextView.LineTransformers.Add(new CustomDateTimeHighlighting());
        }
        private bool isAutoScrollEnabled = true;

        private void DebugLogs_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var textEditor = sender as TextEditor;

            if (textEditor != null)
            {
                // Check if the user is at the end of the text editor
                if (textEditor.VerticalOffset + textEditor.ViewportHeight >= textEditor.ExtentHeight)
                {
                    isAutoScrollEnabled = true;
                }
                else
                {
                    isAutoScrollEnabled = false;
                }
            }
        }
        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (isAutoScrollEnabled)
            {
                DebugLogs.ScrollToEnd();
            }
        }
    }
    public class CustomDateTimeHighlighting : DocumentColorizingTransformer
    {
        private static readonly Regex DateTimeRegex = new Regex(@"\[\d{1,2}/\d{1,2}/\d{4}\s\d{1,2}:\d{2}:\d{2}\s(?:AM|PM)\]", RegexOptions.Compiled);
        private static readonly Regex ProcessingRegex = new Regex(@"Đang xử lý", RegexOptions.Compiled);
        private static readonly Regex CreateRegex = new Regex(@"Tạo", RegexOptions.Compiled);
        private static readonly Regex CompleteRegex = new Regex(@"Hoàn thành", RegexOptions.Compiled);

        protected override void ColorizeLine(DocumentLine line)
        {
            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);

            ApplyHighlighting(DateTimeRegex, text, lineStartOffset, Brushes.Purple);
            ApplyHighlighting(ProcessingRegex, text, lineStartOffset, Brushes.Blue);
            ApplyHighlighting(CreateRegex, text, lineStartOffset, Brushes.Magenta);
            ApplyHighlighting(CompleteRegex, text, lineStartOffset, Brushes.Green);
        }

        private void ApplyHighlighting(Regex regex, string text, int lineStartOffset, SolidColorBrush color)
        {
            MatchCollection matches = regex.Matches(text);

            foreach (Match match in matches)
            {
                base.ChangeLinePart(
                    lineStartOffset + match.Index,
                    lineStartOffset + match.Index + match.Length,
                    (VisualLineElement element) => { element.TextRunProperties.SetForegroundBrush(color); });
            }
        }
    }

}