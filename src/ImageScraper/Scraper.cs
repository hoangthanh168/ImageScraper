//using ImageScraper.Helpers;
//using ImageScraper.ViewModels;
//using PuppeteerSharp;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Web;

//namespace ImageScraper
//{
//    public class Scraper : IDisposable
//    {
//        private readonly HomeViewModel _homeViewModel;
//        private readonly SettingViewModel _settingViewModel;
//        private string _languageCode = "";
//        private readonly Logger _logger;

//        public Scraper(HomeViewModel homeViewModel, SettingViewModel settingViewModel, string languageCode, Logger logger)
//        {
//            _homeViewModel = homeViewModel;
//            _settingViewModel = settingViewModel;
//            _languageCode = languageCode;
//            _logger = logger;
//        }

//        private IBrowser _browser;
//        private IPage _page;

//        public async Task InitializeBrowserAsync(string proxy)
//        {
//            try
//            {
//                _browser = await Puppeteer.LaunchAsync(new LaunchOptions
//                {
//                    Headless = true,
//                    Product = Product.Chrome,
//                    Args = new[] {
//                            $"--proxy-server={proxy}",
//                             "--disable-gpu",
//                             "--disable-dev-shm-usage",
//                             "--disable-software-rasterizer",
//                             "--disable-setuid-sandbox",
//                             "--no-sandbox",
//                             "--disable-web-security",
//                    },
//                    ExecutablePath = "chrome-win\\chrome.exe"
//                });
//            }
//            catch (Exception e)
//            {
//                throw new Exception(e.Message);
//            }
//        }

//        public async Task InitiatePageAsync()
//        {
//            string userAgent = _settingViewModel.GetUserAgent();
//            _page = await _browser.NewPageAsync();
//            await _page.SetUserAgentAsync(userAgent);
//            await _page.SetCacheEnabledAsync(false);
//        }

//        public async Task CloseBrowserAsync()
//        {
//            try
//            {
//                if (_browser != null)
//                {
//                    await _browser.CloseAsync();
//                }
//            }
//            catch (Exception)
//            {
//            }
//        }

//        public async Task ClosePageAsync()
//        {
//            try
//            {
//                if (_page != null)
//                {
//                    await _page.CloseAsync();
//                }
//            }
//            catch (Exception)
//            {
//            }
//        }

//        public async Task<List<ImageModel>> ScrapeImagesAsync(string keyword, CancellationToken cancellation, CancellationToken captchaCancellation, int limitImages)
//        {
//            List<ImageModel> images = new List<ImageModel>();
//            try
//            {
//                await _page.GoToAsync($"https://www.google.com/search?q={keyword}&tbm=isch");
//                bool isCaptcha = await _page.EvaluateExpressionAsync<bool>("document.querySelector('#captcha-form') != null;");
//                bool isDisconnect = await _page.EvaluateExpressionAsync<bool>("document.querySelector('#main-frame-error') != null;");
//                if (isCaptcha || isDisconnect)
//                {
//                    throw new PuppeteerException("Captcha detected");
//                }
//                int tempScrollHeight = 0;
//                bool isScrollable = true;
//                var elements = await _page.QuerySelectorAllAsync(".bRMDJf.islir > img");
//                elements = await _page.QuerySelectorAllAsync(".bRMDJf.islir > img");
//                do
//                {
//                    if (cancellation.IsCancellationRequested || captchaCancellation.IsCancellationRequested)
//                    {
//                        return null;
//                    }
//                    (tempScrollHeight, isScrollable) = await ScrollAsync(_page, tempScrollHeight, keyword);
//                    elements = await _page.QuerySelectorAllAsync(".bRMDJf.islir > img");

//                    if (!isScrollable)
//                    {
//                        break;
//                    }
//                } while (elements.Count() < limitImages);
//                _logger.WriteLine($"{keyword} -> Click images");
//                await ClickAllImageAsync(_page, elements);
//                _logger.WriteLine($"{keyword} -> Parse images");
//                images = await ParseImagesAsync(_page);
//            }
//            catch (Exception e)
//            {
//                if (e.Message.Contains("Captcha") || e.Message.Contains("ERR_PROXY_CONNECTION_FAILED"))
//                {
//                    throw new PuppeteerException("Captcha detected");
//                }
//            }
//            return images;
//        }

//        private async Task<List<ImageModel>> ParseImagesAsync(IPage page)
//        {
//            var elements = await page.QuerySelectorAllAsync("a[href^='/imgres']");
//            var imageTasks = elements.Select(ParseImageAsync);
//            var imageModels = await Task.WhenAll(imageTasks);
//            return imageModels.Where(model => model != null && model.Width > _settingViewModel.ImageWidth && model.Height > _settingViewModel.ImageHeight).ToList();
//        }

//        private async Task<ImageModel> ParseImageAsync(IElementHandle element)
//        {
//            try
//            {
//                var lines = File.ReadAllLines(FilePaths.removeAfterCharacter);
//                string pattern = @"imgurl=([^&]+).*?imgrefurl=([^&]+)";
//                string titlePattern = @"([|-]\s)?((https?:\/\/)?(www\.)?[a-zA-Z0-9-_]+(\.[a-zA-Z0-9-_]+){1,2})([|-])?";
//                string hrefAtt = await element.EvaluateFunctionAsync<string>("(element)=>{return element.getAttribute('href')}", element);
//                string title = await element.EvaluateFunctionAsync<string>("(element)=>{return element.querySelector('div > img').getAttribute('alt')}", element);
//                if (_settingViewModel.IsLanguages)
//                {
//                    string targetLang = _languageCode;
//                    title = TranslateHelper.TranslateText(title, targetLang);
//                }
//                title = Regex.Replace(title, titlePattern, "");
//                title = Regex.Replace(title, @"\d", "");
//                foreach (var line in lines)
//                {
//                    var character = line[0];
//                    if (title.Contains(character))
//                    {
//                        title = title.Split(character)[0].TrimStart().TrimEnd();
//                    }
//                }
//                var removeTexts = File.ReadAllLines(FilePaths.removeText);
//                foreach (var removeText in removeTexts)
//                {
//                    title = title.Replace(removeText, "");
//                }
//                string url = Regex.Match(hrefAtt, pattern).Groups[1].Value;
//                string source = Regex.Match(hrefAtt, pattern).Groups[2].Value;
//                url = HttpUtility.UrlDecode(url);
//                source = HttpUtility.UrlDecode(source);
//                if (!source.Contains("facebook.com"))
//                {
//                    double imageSize = await GetImageSize(url);
//                    int imageWidth = 0, imageHeight = 0;
//                    bool isDuckDuckGoUrl = File.ReadAllText(FilePaths.duckDuckGo).Contains("https://external-content.duckduckgo.com/iu/?u=");
//                    bool isUrlBlock = false;
//                    string imgSrc = url;
//                    if (isDuckDuckGoUrl)
//                    {
//                        isUrlBlock = await ImageHelper.IsImageURLBlocked("https://external-content.duckduckgo.com/iu/?u=" + url);
//                        if (isUrlBlock)
//                        {
//                            isUrlBlock = await ImageHelper.IsImageURLBlocked(url);
//                            if (isUrlBlock)
//                            {
//                                return null;
//                            }
//                        }
//                        else
//                        {
//                            imgSrc = "https://external-content.duckduckgo.com/iu/?u=" + url;
//                        }
//                    }
//                    else
//                    {
//                        isUrlBlock = await ImageHelper.IsImageURLBlocked(url);
//                        if (isUrlBlock)
//                        {
//                            return null;
//                        }
//                    }
//                    GetImageWidthAndHeight(url, ref imageWidth, ref imageHeight);
//                    ImageModel imageModel = new ImageModel()
//                    {
//                        Title = title,
//                        Url = imgSrc,
//                        Source = source,
//                        Height = imageHeight,
//                        Width = imageWidth,
//                        Size = imageSize,
//                    };
//                    _logger.WriteLine($"{imageModel.Title} -> Parsed");
//                    return imageModel;
//                }
//            }
//            catch (Exception e)
//            {
//            }
//            return null;
//        }

//        private async Task<double> GetImageSize(string url)
//        {
//            using (var client = new HttpClient())
//            {
//                var response = await client.GetAsync(url);
//                if (response.IsSuccessStatusCode)
//                {
//                    if (response.Content.Headers.TryGetValues("Content-Length", out var values))
//                    {
//                        var sizeInBytes = long.Parse(values.First());
//                        var sizeInKB = (int)sizeInBytes / 1024;
//                        return sizeInKB;
//                    }
//                    else
//                    {
//                        return 0;
//                    }
//                }
//                else
//                {
//                    return 0;
//                }
//            }
//        }

//        private void GetImageWidthAndHeight(string url, ref int width, ref int height)
//        {
//            using (var client = new WebClient())
//            {
//                using (var stream = client.OpenRead(url))
//                {
//                    var image = Image.FromStream(stream);
//                    width = image.Width;
//                    height = image.Height;
//                }
//            }
//        }

//        private async Task RightClickAsync(IPage page, IElementHandle element)
//        {
//            try
//            {
//                await page.EvaluateFunctionAsync(@"(element)=>{
//                    let event = new MouseEvent('mousedown', {
//                    bubbles: true,
//                    cancelable: false,
//                    view: window,
//                    button: 2,
//                    buttons: 2,
//                    clientX: element.getBoundingClientRect().x,
//                    clientY: element.getBoundingClientRect().y,
//                    });
//                    element.dispatchEvent(event);
//            }", element);
//            }
//            catch (Exception)
//            {
//            }
//        }

//        private async Task ClickAllImageAsync(IPage page, IElementHandle[] elements)
//        {
//            var tasks = elements.Select(element => Task.Run(() => RightClickAsync(page, element)));
//            await Task.WhenAll(tasks);
//        }

//        private async Task<(int, bool)> ScrollAsync(IPage page, int tempScrollHeight, string keyword)
//        {
//            //scroll
//            await page.EvaluateFunctionAsync(@"() => { window.scrollTo(0,document.body.scrollHeight) }");
//            //wait
//            await page.WaitForExpressionAsync("document.querySelector('.hg3Lgc.qs41qe').getAttribute('data-loadingmessage') != ' '");
//            int breakCount = 0;
//            int scrollHeight;
//            do
//            {
//                await Task.Delay(300);
//                scrollHeight = await page.EvaluateExpressionAsync<int>("document.body.scrollHeight");
//                _logger.WriteLine($"{keyword} Scroll {scrollHeight} px");
//                breakCount++;
//                if (breakCount == 5)
//                {
//                    bool isShowMoreResultVisible = await page.EvaluateExpressionAsync<bool>("document.querySelector('#islmp .YstHxe').getAttribute('style') == '' ");
//                    if (isShowMoreResultVisible)
//                    {
//                        try
//                        {
//                            await page.ClickAsync("#islmp input[type='button']");
//                            await page.WaitForExpressionAsync("document.querySelector('.hg3Lgc.qs41qe').getAttribute('data-loadingmessage') != ' '");
//                        }
//                        catch (Exception)
//                        {
//                        }
//                    }
//                    else
//                    {
//                        break;
//                    }
//                }
//            } while (scrollHeight == tempScrollHeight);
//            bool isCrollable = tempScrollHeight < scrollHeight ? true : false;
//            if (isCrollable)
//            {
//                tempScrollHeight = scrollHeight;
//            }
//            return (tempScrollHeight, isCrollable);
//        }

//        public void Dispose()
//        {
//            _browser.Dispose();
//        }
//    }

//    public class ImageModel
//    {
//        public string Url { get; set; }
//        public string Title { get; set; }
//        public string Source { get; set; }
//        public int Width { get; set; }
//        public int Height { get; set; }
//        public double Size { get; set; }
//    }
//}