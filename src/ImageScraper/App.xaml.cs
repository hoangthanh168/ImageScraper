using ImageScraper.Helpers;
using ImageScraper.Models;
using ImageScraper.Services;
using ImageScraper.ViewModels;
using ImageScraper.Views;
using Newtonsoft.Json;
using Prism.Ioc;
using System.IO;
using System.Windows;

namespace ImageScraper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            GenarateDefaultFiles();
            return Container.Resolve<MainWindow>();
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<TinsoftHelper, TinsoftHelper>();
            containerRegistry.RegisterSingleton<UserSettings, UserSettings>();
            containerRegistry.RegisterSingleton<MainWindowViewModel, MainWindowViewModel>();
            containerRegistry.RegisterSingleton<WordpressViewModel, WordpressViewModel>();
            containerRegistry.RegisterSingleton<SettingViewModel, SettingViewModel>();
            containerRegistry.RegisterSingleton<HomeViewModel, HomeViewModel>();
            containerRegistry.RegisterSingleton<ImageEditorViewModel, ImageEditorViewModel>();
            containerRegistry.RegisterSingleton<FileService, FileService>();
            containerRegistry.RegisterSingleton<OpenAIViewModel, OpenAIViewModel>();
            containerRegistry.RegisterSingleton<QuoteFiltersViewModel, QuoteFiltersViewModel>();
            containerRegistry.RegisterForNavigation<Home>();
            containerRegistry.RegisterForNavigation<Wordpress>();
            containerRegistry.RegisterForNavigation<Setting>();
            containerRegistry.RegisterForNavigation<ImageEditor>();
            containerRegistry.RegisterForNavigation<OpenAI>();
            containerRegistry.RegisterDialogWindow<CustomWindow>();
            containerRegistry.RegisterDialog<QuoteFilters>();
        }

        private UserSettings _userSettings;
        private OpenAIViewModel _openAIViewModel;
        private ImageEditorViewModel _imageEditorViewModel;

        protected override void OnInitialized()
        {
            _userSettings = Container.Resolve<UserSettings>();
            _userSettings.LoadSettings();
            _openAIViewModel = Container.Resolve<OpenAIViewModel>();
            _imageEditorViewModel = Container.Resolve<ImageEditorViewModel>();
            base.OnInitialized();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            _userSettings.SaveSettings();
            _openAIViewModel.SaveInstruction();
            _openAIViewModel.SaveContentInstruction();
            _openAIViewModel.SaveTopics();
            _openAIViewModel.SavePrompt();
            _openAIViewModel.SaveContentIntroPrompt();
            _openAIViewModel.SaveContentEndPrompt();
            _imageEditorViewModel.SaveDraw();
            _imageEditorViewModel.SaveTextDemo();
            base.OnExit(e);
        }
        private void GenarateDefaultFiles()
        {
            if (!File.Exists(FilePaths.wpApiSetting))
            {
                var data = new
                {
                    location_perpage = "1000",
                    features_perpage = "1000",
                    listing_category_perpage = "1000",
                    list_tags_perpage = "1000"
                };
                // Chuyển đối tượng sang định dạng JSON
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                // Lưu định dạng JSON vào tệp tin
                File.WriteAllText(FilePaths.wpApiSetting, json);
            }
            FolderAndFileHelper.CreateDirectory(FolderPaths.htmlTemplate);
            FolderAndFileHelper.CreateDirectory(FolderPaths.openAI);
            FolderAndFileHelper.CreateDirectory(FolderPaths.imageEditor);
            FolderAndFileHelper.CreateDirectory(FolderPaths.imageFiles);
            FolderAndFileHelper.CreateFile(FilePaths.textDemo);
            FolderAndFileHelper.CreateFile(FilePaths.inputExamples);
            FolderAndFileHelper.CreateFile(FilePaths.topics);
            FolderAndFileHelper.CreateFile(FilePaths.instruction);
            FolderAndFileHelper.CreateFile(FilePaths.contentInstrution);
            FolderAndFileHelper.CreateFile(FilePaths.prompt);
            FolderAndFileHelper.CreateFile(FilePaths.introContentPrompt);
            FolderAndFileHelper.CreateFile(FilePaths.endContentPrompt);
            FolderAndFileHelper.CreateFile(FilePaths.keywords);
            //FolderAndFileHelper.CreateFile(FilePaths.tagDescription);
            FolderAndFileHelper.CreateFile(FilePaths.index);
            FolderAndFileHelper.CreateFile(FilePaths.scrapedKeywords);
        }
    }
}