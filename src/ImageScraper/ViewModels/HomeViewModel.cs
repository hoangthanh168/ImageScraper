using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using DocumentFormat.OpenXml.Spreadsheet;
using ICSharpCode.AvalonEdit.Document;
using ImageScraper.Extensions;
using ImageScraper.Helpers;
using ImageScraper.Models;
using ImageScraper.Mvvm;
using ImageScraper.Services;
using ImageScraper.ViewModels.Base;
using ImageScraper.ViewModels.Draw;
using ImageScraper.Writers;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordPressPCL.Models;
using Task = System.Threading.Tasks.Task;
using Title = WordPressPCL.Models.Title;

namespace ImageScraper.ViewModels
{
    public class HomeViewModel : ViewModelBase
    {
        //private bool _isAutoCreateQuotes;

        //public bool IsAutoCreateQuotes
        //{
        //    get { return _isAutoCreateQuotes; }
        //    set
        //    {
        //        SetProperty(ref _isAutoCreateQuotes, value);
        //        _userSettings.IsAutoCreateQuotes = _isAutoCreateQuotes;
        //    }
        //}

        private int _numberOfImagesFrom;

        public int NumberOfImagesFrom
        {
            get
            {
                if (_numberOfImagesFrom < 1)
                {
                    _numberOfImagesFrom = 1;
                }
                return _numberOfImagesFrom;
            }
            set
            {
                SetProperty(ref _numberOfImagesFrom, value);
                _userSettings.NumberOfImagesFrom = _numberOfImagesFrom;
            }
        }

        private int _numberOfImagesTo;

        public int NumberOfImagesTo
        {
            get
            {
                if (_numberOfImagesTo < 1)
                {
                    _numberOfImagesTo = 1;
                }
                return _numberOfImagesTo;
            }
            set
            {
                SetProperty(ref _numberOfImagesTo, value);
                _userSettings.NumberOfImagesTo = _numberOfImagesTo;
            }
        }

        private int _numberOfThreads;

        public int NumberOfThreads
        {
            get
            {
                if (_numberOfThreads < 1)
                {
                    _numberOfThreads = 1;
                }
                return _numberOfThreads;
            }
            set
            {
                SetProperty(ref _numberOfThreads, value);
                _userSettings.NumberOfThreads = _numberOfThreads;
            }
        }

        private int _numberOfWpThreads;

        public int NumberOfWpThreads
        {
            get
            {
                if (_numberOfWpThreads < 1)
                {
                    _numberOfWpThreads = 1;
                }
                return _numberOfWpThreads;
            }
            set
            {
                SetProperty(ref _numberOfWpThreads, value);
                _userSettings.NumberOfWpThreads = _numberOfWpThreads;
            }
        }

        private int _numberOfWpPostThreads;

        public int NumberOfWpPostThreads
        {
            get
            {
                if (_numberOfWpPostThreads < 1)
                {
                    _numberOfWpPostThreads = 1;
                }
                return _numberOfWpPostThreads;
            }
            set
            {
                SetProperty(ref _numberOfWpPostThreads, value);
                _userSettings.NumberOfWpPostThreads = _numberOfWpPostThreads;
            }
        }

        private string _status;

        public string Status
        {
            get { return _status; }
            set
            {
                SetProperty(ref _status, value);
            }
        }

        private bool _isStartProgressActive;

        public bool IsStartProgressActive
        {
            get { return _isStartProgressActive; }
            set
            {
                SetProperty(ref _isStartProgressActive, value);
            }
        }

        private int _totalOfKeywords;

        public int TotalOfKeywords
        {
            get { return _totalOfKeywords; }
            set
            {
                SetProperty(ref _totalOfKeywords, value);
            }
        }

        private int _scrapedOfKeywords;

        public int ScrapedOfKeywords
        {
            get { return _scrapedOfKeywords; }
            set
            {
                SetProperty(ref _scrapedOfKeywords, value);
            }
        }

        private UserSettings _userSettings;
        private WordpressViewModel _wordpressViewModel;
        private SettingViewModel _settingViewModel;
        private TinsoftHelper _tinsoftHelper;
        private readonly OpenAIViewModel _openAIViewModel;
        private readonly ImageEditorViewModel _imageEditorViewModel;

        public TextDocument Keywords { get; set; } = new TextDocument();
        public TextDocument Logs { get; set; } = new TextDocument();

        private CancellationTokenSource cancellationTokenSource;
        private CancellationTokenSource captchaCancellationTokenSource;

        public DelegateCommand StartCommand { get; set; }
        public DelegateCommand StopCommand { get; set; }
        public DelegateCommand TextChangedCommand { get; set; }

        private object writeLock = new object();
        private Logger logger;

        public HomeViewModel(UserSettings userSettings, WordpressViewModel wordpressViewModel, SettingViewModel settingViewModel, TinsoftHelper tinsoftHelper, OpenAIViewModel openAIViewModel, ImageEditorViewModel imageEditorViewModel)
        {
            _userSettings = userSettings;
            _wordpressViewModel = wordpressViewModel;
            _settingViewModel = settingViewModel;
            _tinsoftHelper = tinsoftHelper;
            _openAIViewModel = openAIViewModel;
            _imageEditorViewModel = imageEditorViewModel;
            StartCommand = new DelegateCommand(Start);
            StopCommand = new DelegateCommand(Stop);

            logger = new Logger(Logs);
            LoadSetting();
        }

        private bool CanStart()
        {
            if (_settingViewModel.TemplateFiles.Count == 0)
            {
                MessageBoxService.ShowWarning("No templates available");
                return false;
            }
            if (_wordpressViewModel.isConnected == false)
            {
                MessageBoxService.ShowWarning("Please log in first");
                return false;
            }
            if (_wordpressViewModel.SelectedUsers.Count == 0)
            {
                MessageBoxService.ShowWarning("Please Select 1 user");
                return false;
            }
            if (NumberOfImagesFrom > NumberOfImagesTo)
            {
                MessageBoxService.ShowWarning("The number of images on the left should be smaller than the one on the right");
                return false;
            }
            if (IsStartProgressActive)
            {
                MessageBoxService.ShowWarning("Currently running, please stop before starting again");
                return false;
            }
            if (_settingViewModel.TagTitle == null)
            {
                MessageBoxService.ShowWarning("Tag Title not entered");
                return false;
            }
            if (_settingViewModel.PostTitle == null)
            {
                MessageBoxService.ShowWarning("Post Title not entered");
                return false;
            }
            if (_settingViewModel.CategoryTitle == null)
            {
                MessageBoxService.ShowWarning("Category Title not entered");
                return false;
            }
            if (_imageEditorViewModel.IsRandomImage)
            {
                if (!Directory.Exists(_imageEditorViewModel.ImagePath))
                {
                    MessageBoxService.ShowError("Đường dẫn không tồn tại.");
                    return false;
                }
                else if (File.Exists(_imageEditorViewModel.ImagePath))
                {
                    MessageBoxService.ShowError("Đường dẫn này là một tập tin, vui lòng cung cấp đường dẫn thư mục.");
                    return false;
                }
            }
            else
            {
                var imageVisual = _imageEditorViewModel.Items.OfType<Draw.ImageVisual>();
                if (imageVisual.Count() == 0)
                {
                    MessageBoxService.ShowError("Chưa thêm ảnh nào vào trong canvas");
                    return false;
                }
            }
            if (_openAIViewModel.IsLoginProgressActive)
            {
                MessageBoxService.ShowError("Vui lòng dừng tạo từ khóa trước khi bắt đầu chạy");
                return false;
            }

            return true;
        }
        //private string GetRandomImage(string folderPath)
        //{
        //    string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        //    string[] imageFiles = Directory.GetFiles(folderPath)
        //        .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
        //        .ToArray();
        //    if (imageFiles.Length > 0)
        //    {
        //        Random random = new Random();
        //        int randomIndex = random.Next(imageFiles.Length);
        //        string randomImage = imageFiles[randomIndex];
        //        return randomImage;
        //    }
        //    else
        //    {
        //        return "";
        //    }
        //}
        private string GetNextImage(string imagePath,ref int imageIndex)
        {
            string[] imageFiles = Directory.GetFiles(imagePath, "*.*", SearchOption.AllDirectories)
                .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".gif") | file.EndsWith(".bmp"))
                .ToArray();

            if (imageFiles.Length == 0)
            {
                return null;
            }

            string nextImage = imageFiles[imageIndex];

            imageIndex++;

            if (imageIndex >= imageFiles.Length)
            {
                imageIndex = 0;
            }

            return nextImage;
        }

        private static Random _random = new Random();

        private static IEnumerable<T> GetRandomElements<T>(IEnumerable<T> source, int count)
        {
            return source.OrderBy(x => _random.Next()).Take(count);
        }
        private void ClearImages(string folderPath)
        {
            try
            {
                // Get all files in the specified folder.
                string[] files = Directory.GetFiles(folderPath);

                // Loop through each file and delete it.
                foreach (string file in files)
                {
                    File.Delete(file);
                    Console.WriteLine($"Deleted file: {file}");
                }

            }
            catch (Exception ex)
            {
            }
        }
        private async void Start()
        {
            if (CanStart())
            {
                cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = cancellationTokenSource.Token;
                try
                {
                    Logs.Text = "";
                    await Task.Run(async () =>
                    {
                        ClearImages(FolderPaths.imageFiles);
                        Status = "Đang chạy";
                        IsStartProgressActive = true;
                        logger.WriteLine("Bắt đầu");
                        var quotes = _openAIViewModel.Quotes.Distinct();
                        var quoteGroups = quotes.GroupBy(x => x.Topic).ToList();
                        //TotalOfKeywords = quoteGroups.Count();
                        logger.WriteLine($"Tổng số nhóm chủ đề: {quoteGroups.Count()}");
                        SemaphoreSlim quoteSemaphore = new SemaphoreSlim(NumberOfThreads);
                        SemaphoreSlim groupSemaphore = new SemaphoreSlim(NumberOfThreads);
                        var quoteGroupTasks = new List<Task>();
                        foreach (var group in quoteGroups)
                        {
                            await groupSemaphore.WaitAsync(cancellationToken);
                            quoteGroupTasks.Add(Task.Run(async() =>
                            {
                                try
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    int randomQuoteCount = _random.Next(NumberOfImagesFrom, NumberOfImagesTo);
                                    var randomQuotes = GetRandomElements(group, GetRandomNumberOfImages());
                                    logger.WriteLine($"Đang xử lý nhóm chủ đề: {group.Key}. Số lượng trích dẫn ngẫu nhiên: {randomQuoteCount}");
                                    List<Task> tasks = new List<Task>();
                                    // Tạo danh sách để lưu outputPath và quote.Content theo mỗi chủ đề
                                    List<(string OutputPath, string QuoteContent)> quoteOutputPaths = new List<(string, string)>();
                                    int imageIndex = 0;
                                    foreach (var quote in randomQuotes)
                                    {
                                        cancellationToken.ThrowIfCancellationRequested();
                                        await quoteSemaphore.WaitAsync(cancellationToken);
                                        tasks.Add(Task.Run(() =>
                                        {
                                            try
                                            {
                                                logger.WriteLine($"Đang xử lý trích dẫn: {quote.Content}");
                                                string outputPath = Path.Combine(FolderPaths.imageFiles, quote.Content.CreateHash() + ".jpg");
                                                ContentAlignment alignment;
                                                if (_imageEditorViewModel.IsAlignLeftChecked == true)
                                                {
                                                    alignment = ContentAlignment.MiddleLeft;
                                                }
                                                else if (_imageEditorViewModel.IsAlignCenterChecked == true)
                                                {
                                                    alignment = ContentAlignment.MiddleCenter;
                                                }
                                                else if (_imageEditorViewModel.IsAlignRightChecked == true)
                                                {
                                                    alignment = ContentAlignment.MiddleRight;
                                                }
                                                else
                                                {
                                                    alignment = ContentAlignment.MiddleCenter; // Default value if no RadioButton is selected
                                                }
                                                var selectedFont = _imageEditorViewModel.SelectedFonts.GetRandomElement();
                                                var imageVisual = _imageEditorViewModel.Items.OfType<Draw.ImageVisual>().First();
                                                var recs = _imageEditorViewModel.Items.OfType<Draw.Rectangle>();
                                                string domain = new Uri(_wordpressViewModel.Url).Host;
                                                if (_imageEditorViewModel.IsRandomImage)
                                                {
                                                    string imagePath = GetNextImage(_imageEditorViewModel.ImagePath, ref imageIndex);
                                                    using (WriteTextToImage writeTextToImage = new WriteTextToImage(imagePath, _imageEditorViewModel))
                                                    {
                                                        writeTextToImage.WriteTextOnImageAsync(outputPath, quote.Content, WriteTextToImage.ConvertMediaColorToDrawingColor(_imageEditorViewModel.SelectedColor), _imageEditorViewModel.SelectedFontWeight, alignment, selectedFont, default(RectangleF), quote.Author, domain);
                                                    }
                                                }
                                                else
                                                {
                                                    if (recs.Count() > 0)
                                                    {
                                                        using (WriteTextToImage writeTextToImage = new WriteTextToImage(imageVisual.ImageSource, _imageEditorViewModel))
                                                        {
                                                            Draw.Rectangle rec = _imageEditorViewModel.Items.OfType<Draw.Rectangle>().First();
                                                            var imageTop = imageVisual.Top;
                                                            var imageLeft = imageVisual.Left;
                                                            var positionX = Math.Abs(imageLeft - rec.Left);
                                                            var positionY = Math.Abs(imageTop - rec.Top);
                                                            RectangleF textRectangle = new RectangleF((float)positionX, (float)positionY, (float)rec.Width, (float)rec.Height);
                                                            writeTextToImage.WriteTextOnImageAsync(outputPath, quote.Content, WriteTextToImage.ConvertMediaColorToDrawingColor(_imageEditorViewModel.SelectedColor), _imageEditorViewModel.SelectedFontWeight, alignment, selectedFont, textRectangle, quote.Author, domain);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        using (WriteTextToImage writeTextToImage = new WriteTextToImage(imageVisual.ImageSource, _imageEditorViewModel))
                                                        {
                                                            writeTextToImage.WriteTextOnImageAsync(outputPath, quote.Content, WriteTextToImage.ConvertMediaColorToDrawingColor(_imageEditorViewModel.SelectedColor), _imageEditorViewModel.SelectedFontWeight, alignment, selectedFont, default(RectangleF), quote.Author, domain);
                                                        }
                                                    }
                                                }
                                                quoteOutputPaths.Add((outputPath, quote.Content));
                                                logger.WriteLine($"Hoàn thành việc ghi chữ lên hình ảnh cho trích dẫn: {quote.Content}");
                                            }
                                            finally
                                            {
                                                quoteSemaphore.Release();
                                            }
                                        }));
                                    }
                                    await Task.WhenAll(tasks);
                                    string intro = "";
                                    string end = "";
                                    Task aiWriteTask = Task.Run(async() => 
                                    {
                                        if (_openAIViewModel.IsAutoGeneratedContentIntro)
                                        {
                                            intro = await _openAIViewModel.ProcessContent(quoteOutputPaths.Count.ToString(), group.Key, cancellationToken);
                                        }
                                        if (_openAIViewModel.IsAutoGeneratedContentEnd)
                                        {
                                            end = await _openAIViewModel.ProcessContent(quoteOutputPaths.Count.ToString(), group.Key, cancellationToken, false);
                                        }
                                    });
                                    logger.WriteLine($"Tạo media cho nhóm trích dẫn: {group.Key}");
                                    var wpTasks = new List<Task>();
                                    SemaphoreSlim semaphore1 = new SemaphoreSlim(NumberOfWpThreads);
                                    for (int i = 0; i < quoteOutputPaths.Count; i++)
                                    {
                                        cancellationToken.ThrowIfCancellationRequested();
                                        await semaphore1.WaitAsync(cancellationToken);
                                        int index = i;
                                        wpTasks.Add(Task.Run(async () =>
                                        {
                                            cancellationToken.ThrowIfCancellationRequested();
                                            try
                                            {
                                                var item = quoteOutputPaths[index];
                                                var media = await WordpressHelper.Instance.CreateMedia(item.OutputPath);
                                                logger.WriteLine($"Tạo media cho trích dẫn: {quoteOutputPaths[index].QuoteContent} Thành công Id: {media.Id}");
                                                // Cập nhật OutputPath trong quoteOutputPaths bằng thuộc tính link của media
                                                quoteOutputPaths[index] = (media.SourceUrl, item.QuoteContent);
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.WriteLine($"Lỗi khi tạo media cho trích dẫn: {quoteOutputPaths[index].QuoteContent}. Ngoại lệ: {ex.Message}");
                                            }
                                            finally
                                            {
                                                semaphore1.Release();
                                            }
                                        }, cancellationToken));
                                    }
                                    await Task.WhenAll(wpTasks);
                                    logger.WriteLine($"Hoàn thành việc tạo media cho nhóm trích dẫn: {group.Key}");
                                    WriteQuotePost writeQuotePost = new WriteQuotePost(_wordpressViewModel, _settingViewModel,_openAIViewModel);
                                    string postTitle = writeQuotePost.WritePostTitle(quoteOutputPaths.Count.ToString(), group.Key);
                                    var title = new Title(postTitle);
                                    string tagTitle = writeQuotePost.WriteTagTitle(quoteOutputPaths.Count.ToString(), group.Key);
                                    string categoryTitle = writeQuotePost.WriteCategoryTitle(quoteOutputPaths.Count.ToString(), group.Key);
                                    string internalTitle = postTitle;
                                    if (_settingViewModel.InternalTitle != null)
                                    {
                                        if (_settingViewModel.InternalTitle.Trim() != "")
                                        {
                                            internalTitle = writeQuotePost.WriteInternalTitle(quoteOutputPaths.Count.ToString(), group.Key);
                                        }
                                    }
                                    logger.WriteLine($"Đang xử lý content cho nhóm trích dẫn: {group.Key}");
                                    var startTime = DateTime.UtcNow;
                                    aiWriteTask.Wait();
                                    var endTime = DateTime.UtcNow;
                                    var content = new Content(writeQuotePost.WriteContent(quoteOutputPaths, group.Key, categoryTitle, tagTitle, postTitle, internalTitle, intro,end));
                                    var elapsedTime = endTime - startTime;
                                    logger.WriteLine($"Mất {elapsedTime:hh\\:mm\\:ss} đề hoàn thành content cho nhóm trích dẫn: {group.Key}");
                                    string header = File.ReadAllText(FilePaths.header);
                                    await PostToWordPressAsync(title, content, quoteOutputPaths.First().OutputPath, tagTitle, categoryTitle, header);
                                    logger.WriteLine($"Hoàn thành việc đăng bài lên WordPress cho nhóm trích dẫn: {group.Key}");
                                }
                                catch (OperationCanceledException)
                                {
                                    logger.WriteLine("Quá trình đã bị hủy");
                                }
                                catch (Exception ex)
                                {
                                    logger.WriteLine($"Có lỗi xảy ra: {ex.Message}");
                                }
                                finally
                                {
                                    groupSemaphore.Release();
                                }
                             
                            }));
                        }
                        await Task.WhenAll(quoteGroupTasks);
                        logger.WriteLine("Kết thúc");
                        IsStartProgressActive = false;
                        Status = "";
                    });
                }
                catch (Exception)
                {
                    logger.WriteLine("Kết thúc");
                    IsStartProgressActive = false;
                    Status = "";
                }
               
            }
        }

        private bool IsCaptchaException(Exception ex)
        {
            return ex.Message.Contains("Captcha") || ex.Message.Contains("ERR_PROXY_CONNECTION_FAILED");
        }

        private bool CanStop()
        {
            return (cancellationTokenSource != null) ? true : false;
        }

        private void Stop()
        {
            if (CanStop())
            {
                Status = "Stopping...";
                cancellationTokenSource.Cancel();
            }
        }

        private async Task PostToWordPressAsync(Title title, WordPressPCL.Models.Content content, string featuredImageUrl,string tagTitle,string CategoryTitle,string header)
        {
            try
            {
                WordPressPCL.Models.Status status = WordPressPCL.Models.Status.Draft;
                if (_wordpressViewModel.SelectedStatusTypeIndex == 0)
                {
                    status = WordPressPCL.Models.Status.Publish;
                }
                status = WordPressPCL.Models.Status.Publish;
                int authorId = WordpressHelper.users.First(x => x.data.display_name == _wordpressViewModel.SelectedUsers[0]).ID;
                List<int> tagIds = new List<int>();
                List<int> categoryIds = new List<int>();

                if (_settingViewModel.IsAutoCreateCategoryByTopic)
                {
                    int id = await WordpressHelper.Instance.CreateCategory(CategoryTitle);
                    categoryIds.Add(id);
                }
                if (_settingViewModel.IsAutoCreateTagByTopic)
                {
                    int id = await WordpressHelper.Instance.CreateTag(tagTitle);
                    tagIds.Add(id);
                }
                foreach (var selectedTag in _wordpressViewModel.SelectedTags)
                {
                    var tagId = WordpressHelper.categories.First(x => x.Name == selectedTag).Id;
                    tagIds.Add(tagId);
                }
                foreach (var selectedCategory in _wordpressViewModel.SelectedCategories)
                {
                    var categoryId = WordpressHelper.categories.First(x => x.Name == selectedCategory).Id;
                    categoryIds.Add(categoryId);
                }
                await WordpressHelper.Instance.PostAsync(title, content, featuredImageUrl, authorId, tagIds, categoryIds, status,header);
            }
            catch (Exception)
            {
            }
        }

        private int GetRandomNumberOfImages()
        {
            return new Random().Next(NumberOfImagesFrom, NumberOfImagesTo);
        }

        private void CountKeywords()
        {
            TotalOfKeywords = Keywords.Text.GetLines().Where(x => !string.IsNullOrEmpty(x)).Count();
        }

        private void CountKeywords(int totalOfKeywords)
        {
            TotalOfKeywords = totalOfKeywords;
        }

        private void TextChanged()
        {
            CountKeywords();
        }

        private void LoadSetting()
        {
            //IsAutoCreateQuotes = _userSettings.IsAutoCreateQuotes;
            NumberOfImagesFrom = _userSettings.NumberOfImagesFrom;
            NumberOfImagesTo = _userSettings.NumberOfImagesTo;
            NumberOfThreads = _userSettings.NumberOfThreads;
            NumberOfWpThreads = _userSettings.NumberOfWpThreads;
            NumberOfWpPostThreads = _userSettings.NumberOfWpPostThreads;
        }
    }

    public class OrderedTask
    {
        public int Order { get; set; }
        public Task Task { get; set; }
    }
}