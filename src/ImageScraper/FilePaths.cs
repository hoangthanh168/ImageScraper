using System.IO;

namespace ImageScraper
{
    public static class FilePaths
    {
        public static string scrapedKeywords = "scraped_keywords.txt";
        public static string keywords = "keywords.txt";
        public static string removeAfterCharacter = "remove_after_characters.txt";
        public static string removeText = "remove_text.txt";
        public static string wpApiSetting = "wp_api_setting.json";

        //public static string tagDescription = Path.Combine(FolderPaths.htmlTemplate, "tag_description.txt");
        public static string index = Path.Combine(FolderPaths.htmlTemplate, "index.txt");
        public static string intro = Path.Combine(FolderPaths.htmlTemplate, "intro.txt");
        public static string end = Path.Combine(FolderPaths.htmlTemplate, "end.txt");
        public static string quote = Path.Combine(FolderPaths.htmlTemplate, "quote.txt");
        public static string header = Path.Combine(FolderPaths.htmlTemplate, "header.txt");
        //public static string duckDuckGo = Path.Combine(FolderPaths.htmlTemplate, "duckduckgo.txt");


        public static string inputExamples = Path.Combine(FolderPaths.openAI, "input_examples.txt");
        public static string topics = Path.Combine(FolderPaths.openAI, "topics.txt");
        public static string instruction = Path.Combine(FolderPaths.openAI, "instruction.txt");
        public static string contentInstrution = Path.Combine(FolderPaths.openAI, "content_instrution.txt");
        public static string prompt = Path.Combine(FolderPaths.openAI, "prompt.txt");
        public static string introContentPrompt = Path.Combine(FolderPaths.openAI, "intro_content_prompt.txt");
        public static string endContentPrompt = Path.Combine(FolderPaths.openAI, "end_content_prompt.txt");
        public static string quoteFiltersCsv = Path.Combine(FolderPaths.openAI, "quote_filters.csv");
        public static string quotesCsv = Path.Combine(FolderPaths.openAI, "quotes.csv");

        public static string drawSaved = Path.Combine(FolderPaths.imageEditor, "draw_saved.json");
        public static string textDemo = Path.Combine(FolderPaths.imageEditor, "text_demo.txt");
        public static string PreviewImage = Path.Combine(FolderPaths.imageEditor, "preview.jpg");
        public static string imageSource = Path.Combine(FolderPaths.imageEditor, "image_source.jpg");

        public static void Reload()
        {
            //tagDescription = Path.Combine(FolderPaths.htmlTemplate, "tag_description.txt");
            index = Path.Combine(FolderPaths.htmlTemplate, "index.txt");
            intro = Path.Combine(FolderPaths.htmlTemplate, "intro.txt");
            end = Path.Combine(FolderPaths.htmlTemplate, "end.txt");
            quote = Path.Combine(FolderPaths.htmlTemplate, "quote.txt");
            header = Path.Combine(FolderPaths.htmlTemplate, "header.txt");

        //duckDuckGo = Path.Combine(FolderPaths.htmlTemplate, "duckduckgo.txt");
    }
}

    public static class FolderPaths
    {
        public static string userFolder = "";
        public static string htmlTemplate = Path.Combine("html_template", userFolder);
        public static string openAI = "open_ai";
        public static string imageEditor = "image_editor";
        public static string imageFiles = Path.Combine(imageEditor, "image_files");

        public static void ReLoad()
        {
            htmlTemplate = Path.Combine("html_template", userFolder);
        }
    }
}