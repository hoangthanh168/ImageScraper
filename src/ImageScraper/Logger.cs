using ICSharpCode.AvalonEdit.Document;
using System;
using System.Diagnostics;

namespace ImageScraper
{
    public class Logger : TraceListener
    {
        private TextDocument output;

        public Logger(TextDocument output)
        {
            this.Name = "Trace";
            this.output = output;
        }

        public override void Write(string message)
        {
            Action append = delegate ()
            {
                output.Insert(output.TextLength, string.Format("[{0}] ", DateTime.Now.ToString()));
                output.Insert(output.TextLength, message);
            };
            System.Windows.Application.Current.Dispatcher.Invoke(
        (Action)(() => { append(); }));
        }

        public override void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }
    }
}