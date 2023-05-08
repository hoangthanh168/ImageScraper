using System.Windows.Forms;

namespace ImageScraper.Services
{
    public static class MessageBoxService
    {
        public static void ShowError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowWarning(string warningMessage)
        {
            MessageBox.Show(warningMessage, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowInformation(string informationMessage)
        {
            MessageBox.Show(informationMessage, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static DialogResult ShowQuestion(string questionMessage)
        {
            return MessageBox.Show(questionMessage, "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
    }
}