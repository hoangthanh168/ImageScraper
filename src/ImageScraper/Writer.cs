//using ImageScraper.Extensions;
//using ImageScraper.Helpers;
//using ImageScraper.ViewModels;
//using System;
//using System.Linq;

//namespace ImageScraper
//{
//    public class Writer
//    {
//        private WordpressViewModel _wordpressViewModel;

//        public Writer(WordpressViewModel wordpressViewModel)
//        {
//            _wordpressViewModel = wordpressViewModel;
//        }

//        public string WritePostContent(string imageTitle, string imageSrc, string imageSource, int imageWidth, int imageHeight, double imageSize, string tagTitle, int imagesCount)
//        {
//            string categoryElems = "";
//            string tagElem = "";
//            string htmlContent = System.IO.File.ReadAllText(FilePaths.index);
//            // Thay thế các tham số trong chuỗi HTML bằng giá trị tương ứng
//            htmlContent = htmlContent.Replace("[img_src]", imageSrc);
//            htmlContent = htmlContent.Replace("[image_title]", imageTitle);
//            htmlContent = htmlContent.Replace("[image_width]", imageWidth.ToString());
//            htmlContent = htmlContent.Replace("[image_height]", imageHeight.ToString());
//            htmlContent = htmlContent.Replace("[image_size]", imageSize.ToString());
//            htmlContent = htmlContent.Replace("[image_source]", imageSource);
//            categoryElems = string.Join(",", WordpressHelper.categories.Where(x => _wordpressViewModel.SelectedCategories.Contains(x.Name)).Select(x => $"<a href=\"{x.Link}\">{x.Name}</a>"));
//            tagElem = $"<a href=\"{_wordpressViewModel.Url + "tag/" + tagTitle.SanitizeTitleInCSharp()}\">{tagTitle}</a>";
//            htmlContent = htmlContent.Replace("[my_website_url]", _wordpressViewModel.Url);
//            htmlContent = htmlContent.Replace("[my_website_domain]", new Uri(_wordpressViewModel.Url).Host);
//            htmlContent = htmlContent.Replace("[tags]", tagElem);
//            htmlContent = htmlContent.Replace("[categories]", categoryElems);
//            htmlContent = htmlContent.Replace("[count]", imagesCount.ToString());
//            // Trả về nội dung HTML đã được thay thế các giá trị tham số
//            return htmlContent;
//        }

//        //public string WriteTagDescription(string keyword, int count)
//        //{
//        //    // Đọc nội dung từ tập tin tagDescription.txt
//        //    string htmlContent = System.IO.File.ReadAllText(FilePaths.tagDescription);
//        //    // Thay thế các tham số trong chuỗi HTML bằng giá trị tương ứng
//        //    htmlContent = htmlContent.Replace("[count]", count.ToString());
//        //    htmlContent = htmlContent.Replace("[keyword]", keyword);
//        //    htmlContent = htmlContent.Replace("[my_website_url]", _wordpressViewModel.Url);
//        //    htmlContent = htmlContent.Replace("[my_website_domain]", new Uri(_wordpressViewModel.Url).Host);
//        //    // Trả về nội dung HTML đã được thay thế các giá trị tham số
//        //    return htmlContent;
//        //}

//        public string WriteTagTitle(string title, string keyword, int count)
//        {
//            return title.Replace("[keyword]", keyword).Replace("[count]", count.ToString());
//        }

//        public string WritePostTitle(string title, string imageTitle)
//        {
//            return title.Replace("[image_title]", imageTitle);
//        }
//    }
//}