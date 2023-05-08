using System.IO;

namespace ImageScraper.Helpers
{
    public static class FolderAndFileHelper
    {
        public static void CreateDirectory(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public static void CreateFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
        }
    }
}