using ImageScraper.Helpers;
using ImageScraper.Models;
using ImageScraper.Mvvm;
using ImageScraper.Services;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ImageScraper.ViewModels
{
    public class SettingViewModel : ViewModelBase, INavigationAware
    {
        private string _tagTitle;

        public string TagTitle
        {
            get { return _tagTitle; }
            set
            {
                SetProperty(ref _tagTitle, value);
                _userSettings.TagTitle = _tagTitle;
            }
        }

        private string _postTitle;

        public string PostTitle
        {
            get { return _postTitle; }
            set
            {
                SetProperty(ref _postTitle, value);
                _userSettings.PostTitle = _postTitle;
            }
        }
        private string _categoryTitle;
        public string CategoryTitle
        {
            get { return _categoryTitle; }
            set
            {
                SetProperty(ref _categoryTitle, value); 
                _userSettings.CategoryTitle = _categoryTitle;
            }
        }

        private string _tinsoftApi;

        public string TinsoftApi
        {
            get { return _tinsoftApi; }
            set
            {
                SetProperty(ref _tinsoftApi, value);
                _userSettings.TinsoftApi = _tinsoftApi;
            }
        }

        private int _selectedTemplateFileIndex;

        public int SelectedTemplateFileIndex
        {
            get { return _selectedTemplateFileIndex; }
            set
            {
                SetProperty(ref _selectedTemplateFileIndex, value);
                _userSettings.SelectedTemplateFileIndex = _selectedTemplateFileIndex;
            }
        }

        private ObservableCollection<string> _templateFiles;

        public ObservableCollection<string> TemplateFiles
        {
            get { return _templateFiles; }
            set
            {
                SetProperty(ref _templateFiles, value);
            }
        }

        public List<string> Platforms
        {
            get { return GetPlatforms(); }
        }

        private int _selectedPlatformsIndex;

        public int SelectedPlatformsIndex
        {
            get { return _selectedPlatformsIndex; }
            set
            {
                SetProperty(ref _selectedPlatformsIndex, value);
                _userSettings.SelectedPlatformsIndex = _selectedPlatformsIndex;
            }
        }

        public List<string> Devices
        {
            get => GetDevices();
        }

        private int _selectedDevicesIndex;

        public int SelectedDevicesIndex
        {
            get { return _selectedDevicesIndex; }
            set
            {
                SetProperty(ref _selectedDevicesIndex, value);
                _userSettings.SelectedDevicesIndex = _selectedDevicesIndex;
            }
        }

        private bool _isLanguages;

        public bool IsLanguages
        {
            get { return _isLanguages; }
            set
            {
                SetProperty(ref _isLanguages, value);
                _userSettings.IsLanguages = _isLanguages;
            }
        }
        private bool _isAutoCreateCategoryByTopic;

        public bool IsAutoCreateCategoryByTopic
        {
            get { return _isAutoCreateCategoryByTopic; }
            set 
            {
                SetProperty(ref _isAutoCreateCategoryByTopic, value);
                _userSettings.IsAutoCreateCategoryByTopic = _isAutoCreateCategoryByTopic;


            }
        }

        private bool _isAutoCreateTagByTopic;

        public bool IsAutoCreateTagByTopic
        {
            get { return _isAutoCreateTagByTopic; }
            set 
            {
                SetProperty(ref _isAutoCreateTagByTopic, value);
                _userSettings.IsAutoCreateTagByTopic = _isAutoCreateTagByTopic;


            }
        }
        public ObservableCollection<string> Languages
        {
            get
            {
                ObservableCollection<string> listLanguages = new ObservableCollection<string>();
                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                HashSet<string> uniqueCultures = new HashSet<string>();
                // Duyệt qua danh sách và in ra language code và language name
                foreach (CultureInfo culture in cultures)
                {
                    try
                    {
                        if (culture.Name != "")
                        {
                            string languageCode = culture.TwoLetterISOLanguageName;
                            if (uniqueCultures.Add(languageCode))
                            {
                                // Lấy language name
                                string languageName = culture.DisplayName;

                                listLanguages.Add($"{languageCode}-{languageName}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                return listLanguages;
            }
        }

        private string _otherLanguage = string.Empty;

        public string OtherLanguage
        {
            get { return _otherLanguage; }
            set
            {
                SetProperty(ref _otherLanguage, value);
                _userSettings.OtherLanguage = _otherLanguage;
            }
        }

        private int _selectedLanguageIndex;

        public int SelectedLanguageIndex
        {
            get { return _selectedLanguageIndex; }
            set
            {
                SetProperty(ref _selectedLanguageIndex, value);
                _userSettings.SelectedLanguageIndex = _selectedLanguageIndex;
            }
        }

        private int _imageWidth;

        public int ImageWidth
        {
            get
            {
                if (_imageWidth < 1)
                {
                    _imageWidth = 1;
                }
                return _imageWidth;
            }
            set
            {
                SetProperty(ref _imageWidth, value);
                _userSettings.ImageWidth = _imageWidth;
            }
        }

        private int _imageHeight;

        public int ImageHeight
        {
            get
            {
                if (_imageHeight < 1)
                {
                    _imageHeight = 1;
                }
                return _imageHeight;
            }
            set
            {
                SetProperty(ref _imageHeight, value);
                _userSettings.ImageHeight = _imageHeight;
            }
        }

        private int _imageSize;

        public int ImageSize
        {
            get { return _imageSize; }
            set
            {
                SetProperty(ref _imageSize, value);
                _userSettings.ImageSize = _imageSize;
            }
        }

        private string _openAIApiKey;

        public string OpenAIApiKey
        {
            get { return _openAIApiKey; }
            set
            {
                SetProperty(ref _openAIApiKey, value);
                _userSettings.OpenAIApiKey = _openAIApiKey;
            }
        }

        private string _internalTitle;

        public string InternalTitle
        {
            get { return _internalTitle; }
            set
            {
                SetProperty(ref _internalTitle, value);
                _userSettings.InternalTitle = _internalTitle;

            }
        }

        public DelegateCommand ReLoadTemplateFilesCommand { get; set; }
        public DelegateCommand TemplateFileSelectionChangedCommand { get; set; }
        public DelegateCommand ClearKeywordCommand { get; set; }
        public DelegateCommand ClearChromiumCommand { get; set; }
        public DelegateCommand CheckApiStatusCommand { get; set; }
        public DelegateCommand TinsoftApiTextChangeCommand { get; set; }

        private readonly UserSettings _userSettings;
        private readonly TinsoftHelper _tinsoftHelper;

        public SettingViewModel(UserSettings userSettings, TinsoftHelper tinsoftHelper)
        {
            _userSettings = userSettings;
            _tinsoftHelper = tinsoftHelper;
            LoadSetting();
            ReLoadTemplateFilesCommand = new DelegateCommand(LoadTemplateFiles);
            TemplateFileSelectionChangedCommand = new DelegateCommand(TemplateFile_SelectionChanged);
            ClearKeywordCommand = new DelegateCommand(ClearScrapedKeywords);
            ClearChromiumCommand = new DelegateCommand(ClearChromium);
            CheckApiStatusCommand = new DelegateCommand(CheckApiStatus);
            TinsoftApiTextChangeCommand = new DelegateCommand(TinsoftApiTextChange);
            LoadTemplateFiles();
            TemplateFile_SelectionChanged();
        }

        private void ClearChromium()
        {
            var result = MessageBoxService.ShowQuestion("Do you want to clean up all Chrome processes?");
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (Process process in Process.GetProcessesByName("chrome"))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception)
                    {
                    }
                }
                MessageBoxService.ShowInformation("Cleanup is complete!");
            }
        }

        private void TemplateFile_SelectionChanged()
        {
            if (TemplateFiles.Count > 0)
            {
                if (SelectedTemplateFileIndex == -1)
                {
                    SelectedTemplateFileIndex = 0;
                }
                if (SelectedTemplateFileIndex >= TemplateFiles.Count)
                {
                    SelectedTemplateFileIndex = 0;
                }
                FolderPaths.userFolder = TemplateFiles[SelectedTemplateFileIndex];
                FolderPaths.ReLoad();
                FilePaths.Reload();
            }
        }

        private void LoadTemplateFiles()
        {
            var templateFiles = Directory.GetDirectories("html_template")
                .Select(d => new DirectoryInfo(d).Name)
                .ToList();
            TemplateFiles = new ObservableCollection<string>(templateFiles);
        }

        private void LoadSetting()
        {
            TagTitle = _userSettings.TagTitle;
            PostTitle = _userSettings.PostTitle;
            CategoryTitle = _userSettings.CategoryTitle;
            TinsoftApi = _userSettings.TinsoftApi;
            IsLanguages = _userSettings.IsLanguages;
            SelectedTemplateFileIndex = _userSettings.SelectedTemplateFileIndex;
            SelectedPlatformsIndex = _userSettings.SelectedPlatformsIndex;
            SelectedDevicesIndex = _userSettings.SelectedDevicesIndex;
            SelectedLanguageIndex = _userSettings.SelectedLanguageIndex;
            OtherLanguage = _userSettings.OtherLanguage;
            ImageWidth = _userSettings.ImageWidth;
            ImageHeight = _userSettings.ImageHeight;
            OpenAIApiKey = _userSettings.OpenAIApiKey;
            IsAutoCreateCategoryByTopic = _userSettings.IsAutoCreateCategoryByTopic;
            IsAutoCreateTagByTopic = _userSettings.IsAutoCreateTagByTopic;
            InternalTitle = _userSettings.InternalTitle;
        }

        private void ClearScrapedKeywords()
        {
            if (MessageBoxService.ShowQuestion("Do you want to clean up all scraped keywords?") == System.Windows.Forms.DialogResult.Yes)
            {
                File.WriteAllText(FilePaths.scrapedKeywords, string.Empty);
                MessageBoxService.ShowInformation("Cleanup is complete!");
            }
        }

        private void CheckApiStatus()
        {
            if (TinsoftApi != "")
            {
                if (!_tinsoftHelper.CheckApiKeyStatus())
                {
                    MessageBoxService.ShowWarning(_tinsoftHelper.api_key_status);
                }
                else
                {
                    MessageBoxService.ShowInformation("OK");
                }
            }
            else
            {
                MessageBoxService.ShowInformation("Chưa nhập key");
            }
        }

        private void TinsoftApiTextChange()
        {
            _tinsoftHelper.api_key = TinsoftApi;
        }

        private List<string> GetDevices()
        {
            return new List<string>() { "desktop", "mobile", "tablet" };
        }

        private List<string> GetPlatforms()
        {
            return new List<string>() { "Win32" };
        }

        public string Getlanguage()
        {
            string lang = Languages[SelectedLanguageIndex].Split('-')[0];
            if (OtherLanguage != "" && OtherLanguage != null)
            {
                lang = OtherLanguage;
            }
            return lang;
        }

        public string GetUserAgent()
        {
            string command = "random-useragent.exe";
            string arguments = $"--device={Devices[SelectedDevicesIndex]} --platform={Platforms[SelectedPlatformsIndex]}";
            string userAgent = CommandExcuterHelper.RunCommand(command, arguments);
            return userAgent;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadSetting();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}