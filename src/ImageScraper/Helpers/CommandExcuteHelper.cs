using System.Diagnostics;
using System.Text;

namespace ImageScraper.Helpers
{
    public static class CommandExcuterHelper
    {
        public static string RunCommand(string command, string arguments)
        {
            string output = string.Empty;
            StringBuilder stdoutStringBuilder = new StringBuilder();
            StringBuilder stderrStringBuilder = new StringBuilder();
            using (Process process = new Process())
            {
                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                process.OutputDataReceived += (sender, e) => stdoutStringBuilder.Append(e.Data);
                process.ErrorDataReceived += (sender, e) => stderrStringBuilder.Append(e.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                if (process.WaitForExit(3000))
                {
                    output = stdoutStringBuilder.ToString();
                    if (string.IsNullOrEmpty(output))
                    {
                        output = stderrStringBuilder.ToString();
                    }
                }
                else
                {
                    process.Kill();
                }
            }
            return output;
        }
    }
}