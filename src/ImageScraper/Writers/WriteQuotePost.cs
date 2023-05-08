using ImageScraper.Extensions;
using ImageScraper.Helpers;
using ImageScraper.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace ImageScraper.Writers
{
    public class WriteQuotePost
    {
        private WordpressViewModel _wordpressViewModel;
        private SettingViewModel _settingViewModel;
        private OpenAIViewModel _openAIViewModel;

        public WriteQuotePost(WordpressViewModel wordpressViewModel,SettingViewModel settingViewModel,OpenAIViewModel openAIViewModel)
        {
            _wordpressViewModel = wordpressViewModel;
            _settingViewModel = settingViewModel;
            _openAIViewModel = openAIViewModel;
        }

        public string WritePostTitle(string quoteCount, string topic)
        {
            string titleTemplate = _settingViewModel.PostTitle;
            titleTemplate = titleTemplate.Replace("[count]", quoteCount);
            titleTemplate = titleTemplate.Replace("[topic]", topic);
            return titleTemplate;
        }
        public string WriteInternalTitle(string quoteCount, string topic)
        {
            string titleTemplate = _settingViewModel.InternalTitle;
            titleTemplate = titleTemplate.Replace("[count]", quoteCount);
            titleTemplate = titleTemplate.Replace("[topic]", topic);
            return titleTemplate;
        }
        public string WriteTagTitle(string quoteCount, string topic)
        {
            string titleTemplate = _settingViewModel.TagTitle;
            titleTemplate = titleTemplate.Replace("[count]", quoteCount);
            titleTemplate = titleTemplate.Replace("[topic]", topic);
            return titleTemplate;
        }
        public string WriteCategoryTitle(string quoteCount, string topic)
        {
            string titleTemplate = _settingViewModel.CategoryTitle;
            titleTemplate = titleTemplate.Replace("[count]", quoteCount);
            titleTemplate = titleTemplate.Replace("[topic]", topic);
            return titleTemplate;
        }
        public string WriteContent(List<(string OutputPath, string QuoteContent)> imageAndQuote,string topic,string categoryTitle, string tagTitle,string postTitle, string internalTitle,string intro= "",string end = "")
        {
            string indexTemplate = File.ReadAllText(FilePaths.index);
            string introTemplate = File.ReadAllText(FilePaths.intro);
            string endTemplate = File.ReadAllText(FilePaths.end);
            if (intro != "")
            {
                introTemplate = intro;
            }
            if (end != "")
            {
                endTemplate = end;
            }
           
            var categories = _wordpressViewModel.SelectedCategories.ToList();
            var tags = _wordpressViewModel.SelectedTags.ToList();
            var categoryElems = string.Join(",", WordpressHelper.categories.Where(x => categories.Contains(x.Name)).Select(x => $"<a href=\"{x.Link}\">{x.Name}</a>"));
            var tagElems = string.Join(",", WordpressHelper.tags.Where(x => tags.Contains(x.Name)).Select(x => $"<a href=\"{x.Link}\">{x.Name}</a>"));
            var categoryElem = $"<a href=\"{_wordpressViewModel.Url+ "category/"+ categoryTitle.SanitizeTitleInCSharp()}/\">{categoryTitle}</a>";
            var tagElem = $"<a href=\"{_wordpressViewModel.Url + "tag/" + tagTitle.SanitizeTitleInCSharp()}\"/>{tagTitle}</a>";
            var user = $"<a href=\"{_wordpressViewModel.Url + "author/" + WordpressHelper.users.First(x => x.data.display_name.ToString() == _wordpressViewModel.SelectedUsers[0]).data.user_nicename.ToString()}/\"><strong>{_wordpressViewModel.SelectedUsers[0]}</strong></a>";
           
            var myPostElem = $"<a href=\"{_wordpressViewModel.Url +  WordpressHelper.Instance.GetSlugAsync(postTitle).Result}/\"><strong>{internalTitle}</strong></a>";
            string quotes = "";
            var myWebsiteElem = $"<a href=\"{_wordpressViewModel.Url}/\"><strong>{new Uri(_wordpressViewModel.Url).Host}</strong></a>";
            // Thay thế các tham số trong chuỗi HTML bằng giá trị tương ứng
            introTemplate = introTemplate.Replace("[count]", imageAndQuote.Count.ToString());
            introTemplate = introTemplate.Replace("[topic]", topic);
            introTemplate = introTemplate.Replace("[my_website_name_url]", myWebsiteElem);
            introTemplate = introTemplate.Replace("[category_name_urls]", categoryElems);
            introTemplate = introTemplate.Replace("[category_name_url]", categoryElem);
            introTemplate = introTemplate.Replace("[tag_name_urls]", tagElems);
            introTemplate = introTemplate.Replace("[tag_name_url]", tagElem);
            introTemplate = introTemplate.Replace("[author_name_url]", user);
            introTemplate = introTemplate.Replace("[post_name_url]", myPostElem);
            endTemplate = endTemplate.Replace("[count]", imageAndQuote.Count.ToString());
            endTemplate = endTemplate.Replace("[topic]", topic);
            endTemplate = endTemplate.Replace("[my_website_name_url]", myWebsiteElem);
            endTemplate = endTemplate.Replace("[category_name_urls]", categoryElems);
            endTemplate = endTemplate.Replace("[category_name_url]", categoryElem);
            endTemplate = endTemplate.Replace("[tag_name_urls]", tagElems);
            endTemplate = endTemplate.Replace("[tag_name_url]", tagElem);
            endTemplate = endTemplate.Replace("[author_name_url]", user);
            endTemplate = endTemplate.Replace("[post_name_url]", myPostElem);
            foreach (var item in imageAndQuote)
            {
                string imageAndQuotesTemplate = File.ReadAllText(FilePaths.quote);
                imageAndQuotesTemplate = imageAndQuotesTemplate.Replace("[image_src]", item.OutputPath);
                imageAndQuotesTemplate = imageAndQuotesTemplate.Replace("[quote_content]", item.QuoteContent);
                quotes += imageAndQuotesTemplate;
            }
            indexTemplate = indexTemplate.Replace("[quotes]", quotes);
            indexTemplate = indexTemplate.Replace("[intro]", $"<p>{introTemplate}</p>");
            indexTemplate = indexTemplate.Replace("[end]", $"<p>{endTemplate}</p>");
            return indexTemplate;
        }
    }
}
