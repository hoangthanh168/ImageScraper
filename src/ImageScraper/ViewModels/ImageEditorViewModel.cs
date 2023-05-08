using ICSharpCode.AvalonEdit.Document;
using ImageScraper.Helpers;
using ImageScraper.Models;
using ImageScraper.Mvvm;
using ImageScraper.Services;
using ImageScraper.ViewModels.Base;
using ImageScraper.ViewModels.Draw;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using static ImageScraper.Helpers.WriteTextToImage;
using static System.Net.Mime.MediaTypeNames;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Windows.FontStyle;
using FontWeight = System.Windows.FontWeight;
using Point = System.Windows.Point;

namespace ImageScraper.ViewModels
{
    public class ImageEditorViewModel : ViewModelBase, INavigationAware
    {
        public bool DrawingEndedHandled { get; private set; }

        private Point _mousePosition;
        public Point MousePosition
        {
            get { return _mousePosition; }
            set
            {
                SetProperty(ref _mousePosition, value);
            }
        }


        private Point _translateOffset;
        public Point TranslateOffset
        {
            get { return _translateOffset; }
            set
            {
                SetProperty(ref _translateOffset, value);
                _userSettings.TranslateOffset = _translateOffset;
            }
        }


        private string _scale;
        public string Scale
        {
            get { return _scale; }
            set
            {
                SetProperty(ref _scale, value);
                _userSettings.Scale = _scale;
            }
        }


        private List<string> _selectedFonts = new List<string>() ;
        public List<string> SelectedFonts
        {
            get { return _selectedFonts; }
            set
            {
                SetProperty(ref _selectedFonts, value);
               
            }
        }


        private string _selectedFontWeight;
        public string SelectedFontWeight
        {
            get { return _selectedFontWeight; }
            set
            {
                SetProperty(ref _selectedFontWeight, value);
                if (!IsFirstNavigate)
                {
                    var rec = Items.OfType<Draw.Rectangle>();
                    if (rec.Count() > 0)
                    {
                        WriteTextOnImage(rec.First(), null);
                    }
                    else
                    {
                        WriteTextOnImage(null, null);

                    }
                }
                _userSettings.SelectedFontWeight = _selectedFontWeight;
            }
        }

        private Drawable _selectedItem;
        public Drawable SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
            }
        }


        private BitmapImage _imageSource;
        public BitmapImage ImageSource
        {
            get { return _imageSource; }
            set 
            { 
                SetProperty(ref _imageSource, value);
            }
        }


        private Color _selectedColor;
        public Color SelectedColor
        {
            get { return _selectedColor; }
            set 
            {
                SetProperty(ref _selectedColor, value);
                if (!IsFirstNavigate)
                {
                    var rec = Items.OfType<Draw.Rectangle>();
                    if (rec.Count() > 0)
                    {
                        WriteTextOnImage(rec.First(), null);
                    }
                    else
                    {
                        WriteTextOnImage(null, null);

                    }
                }
                _userSettings.SelectedColor = _selectedColor;
            }
        }

        private bool _isAlignLeftChecked;
        public bool IsAlignLeftChecked
        {
            get { return _isAlignLeftChecked; }
            set 
            {
                SetProperty(ref _isAlignLeftChecked, value);
                if (!IsFirstNavigate)
                {
                    var rec = Items.OfType<Draw.Rectangle>();
                    if (rec.Count() > 0)
                    {
                        WriteTextOnImage(rec.First(), null);
                    }
                    else
                    {
                        WriteTextOnImage(null, null);

                    }
                }
                _userSettings.IsAlignLeftChecked = _isAlignLeftChecked;
            }
        }

        private bool _isAlignCenterChecked;
        public bool IsAlignCenterChecked
        {
            get { return _isAlignCenterChecked; }
            set
            {
                SetProperty(ref _isAlignCenterChecked, value);
                if (!IsFirstNavigate)
                {
                    var rec = Items.OfType<Draw.Rectangle>();
                    if (rec.Count() > 0)
                    {
                        WriteTextOnImage(rec.First(), null);
                    }
                    else
                    {
                        WriteTextOnImage(null, null);

                    }
                }
                _userSettings.IsAlignCenterChecked = _isAlignCenterChecked;


            }
        }

        private bool _isAlignRightChecked;
        public bool IsAlignRightChecked
        {
            get { return _isAlignRightChecked; }
            set
            {
                SetProperty(ref _isAlignRightChecked, value);
                if (!IsFirstNavigate)
                {
                    var rec = Items.OfType<Draw.Rectangle>();
                    if (rec.Count() > 0)
                    {
                        WriteTextOnImage(rec.First(), null);
                    }
                    else
                    {
                        WriteTextOnImage(null, null);
                    }
                }
                _userSettings.IsAlignRightChecked = _isAlignRightChecked;
            }
        }

        private bool _isAutoRandomColor;
        public bool IsAutoRandomColor
        {
            get { return _isAutoRandomColor; }
            set 
            {
                SetProperty(ref _isAutoRandomColor, value);
                _userSettings.IsAutoRandomColor = _isAutoRandomColor;

            }
        }
        private bool _isFirstNavigate = true;
        public bool IsFirstNavigate
        {
            get { return _isFirstNavigate; }
            set { SetProperty(ref _isFirstNavigate, value); }
        }

        private bool _isRandomImage;

        public bool IsRandomImage
        {
            get { return _isRandomImage; }
            set 
            { 
                SetProperty(ref _isRandomImage, value); 
                _userSettings.IsRandomImage = _isRandomImage;
            }
        }
        private string _imagesPath;

        public string ImagePath
        {
            get { return _imagesPath; }
            set 
            {
                SetProperty(ref _imagesPath, value);
                _userSettings.ImagePath = _imagesPath;

            }
        }

        public ObservableCollection<Drawable> Items { get; set; }
        public ObservableCollection<Drawable> PreviewItems { get; set; }
        public ObservableCollection<string> Fonts { get; set; }
        public ObservableCollection<string> FontStyles { get; set; }
        public ObservableCollection<string> FontWeight { get; set; }
        public ObservableCollection<Drawable> SelectedItems { get; }
        public DelegateCommand<string> GoBackCommand { get; set; }
        public DelegateCommand ReloadFontsCommand { get; set; }
        public DelegateCommand AddImageCommand { get; set; }
        public DelegateCommand DrawRectCommand { get; set; }
        public DelegateCommand BringToFontCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand DeleteAllCommand { get; set; }
        public DelegateCommand<object> KeyDownCommand { get; set; }
        public DelegateCommand TextChangedCommand { get; set; }
        public DelegateCommand MouseUpCommand { get; set; }
        public DelegateCommand SelectedFontsChangedCommand { get; set; }
        
        public TextDocument TextDemo { get; set; } = new TextDocument();
        private IRegionManager _regionManager;
        private FileService _fileService;
        private UserSettings _userSettings;
        private readonly IDialogService _dialogService;

        public void SaveTextDemo()
        {
            File.WriteAllText(FilePaths.textDemo, TextDemo.Text);
        }
        private void LoadTextDemo()
        {
            var textDemo = File.ReadAllText(FilePaths.textDemo);
            TextDemo.Text = textDemo;
        }

        public ImageEditorViewModel(IRegionManager regionManager, FileService fileService, UserSettings userSettings, IDialogService dialogService)
        {
            _regionManager = regionManager;
            _fileService = fileService;
            _userSettings = userSettings;
            _dialogService = dialogService;
            Items = new ObservableCollection<Drawable>();
            PreviewItems = new ObservableCollection<Drawable>();
            SelectedItems = new ObservableCollection<Drawable>();
            Fonts = new ObservableCollection<string>();
            FontStyles = new ObservableCollection<string>();
            FontWeight = new ObservableCollection<string>();
            //events
            Items.CollectionChanged += ItemsChanged;
            //commands
            GoBackCommand = new DelegateCommand<string>(GoBack);
            ReloadFontsCommand = new DelegateCommand(LoadAllFontsAsync);
            AddImageCommand = new DelegateCommand(AddImage);
            DrawRectCommand = new DelegateCommand(DrawRect);
            DeleteCommand = new DelegateCommand(Delete);
            DeleteAllCommand = new DelegateCommand(DeleteAll);
            KeyDownCommand = new DelegateCommand<object>(KeyDown);
            BringToFontCommand = new DelegateCommand(BringToFront);
            TextChangedCommand = new DelegateCommand(TextChange);
            MouseUpCommand = new DelegateCommand(MouseUp);
            SelectedFontsChangedCommand = new DelegateCommand(SelectedFontsChanged);
            //Load Something
            LoadTextDemo();
            LoadAllFontsAsync();
            LoadFontStyles();
            LoadFontWeight();
            LoadDrawSaved();
            LoadSettings();
        }

        private void SelectedFontsChanged()
        {
            if (!IsFirstNavigate)
            {
                var rec = Items.OfType<Draw.Rectangle>();
                if (rec.Count() > 0)
                {
                    WriteTextOnImage(rec.First(), null);
                }
                else
                {
                    WriteTextOnImage(null, null);

                }
            }
            _userSettings.SelectedFonts = _selectedFonts.ToArray();
        }

        private void MouseUp()
        {
            var rec = Items.OfType<Draw.Rectangle>();
            if (rec.Count() > 0)
            {
                WriteTextOnImage(rec.First(), null);
            }
        }
        private void DeleteAll()
        {
            Items.Clear();
        }
        private void LoadRectangleEvents()
        {
            foreach (var item in Items.OfType<Draw.Rectangle>())
            {
                item.RectangleChanged -= WriteTextOnImage;
                item.RectangleChanged += WriteTextOnImage;
            }
        }
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public async void WriteTextOnImage(object sender, EventArgs e)
        {
            if (!await _semaphore.WaitAsync(0))
            {
                return;
            }
            var drawdable = sender as Drawable;
            try
            {
                if (Items.OfType<ImageVisual>().Count() > 0)
                {

                    var imageVisual = Items.OfType<ImageVisual>().First();
                    ContentAlignment alignment;
                    if (IsAlignLeftChecked == true)
                    {
                        alignment = ContentAlignment.MiddleLeft;
                    }
                    else if (IsAlignCenterChecked == true)
                    {
                        alignment = ContentAlignment.MiddleCenter;
                    }
                    else if (IsAlignRightChecked == true)
                    {
                        alignment = ContentAlignment.MiddleRight;
                    }
                    else
                    {
                        alignment = ContentAlignment.MiddleCenter; // Default value if no RadioButton is selected
                    }
                    string text = TextDemo.Text;
                    await Task.Run(() =>
                    {
                        if (SelectedFonts != null && SelectedFonts.Count != 0)
                        {
                            if (sender != null)
                            {
                                var imageTop = imageVisual.Top;
                                var imageLeft = imageVisual.Left;
                                var positionX = Math.Abs(imageLeft - drawdable.Left);
                                var positionY = Math.Abs(imageTop - drawdable.Top);
                                using (WriteTextToImage writeTextToImage = new WriteTextToImage(imageVisual.ImageSource, this))
                                {
                                    RectangleF textRectangle = new RectangleF((float)positionX, (float)positionY, (float)drawdable.Width, (float)drawdable.Height);
                                    writeTextToImage.WriteTextOnImageAsync(FilePaths.PreviewImage, text, WriteTextToImage.ConvertMediaColorToDrawingColor(SelectedColor), SelectedFontWeight, alignment, SelectedFonts.First(), textRectangle);
                                }
                            }
                            else
                            {
                                using (WriteTextToImage writeTextToImage = new WriteTextToImage(imageVisual.ImageSource, this))
                                {
                                    writeTextToImage.WriteTextOnImageAsync(FilePaths.PreviewImage, text, ConvertMediaColorToDrawingColor(SelectedColor), SelectedFontWeight, alignment, SelectedFonts.First());
                                }
                            }
                        }
                    });
                    LoadImage(FilePaths.PreviewImage);
                }

            }
            finally
            {
                _semaphore.Release();
            }
        }
     
        private void TextChange()
        {
            if (!IsFirstNavigate)
            {
                var rec = Items.OfType<Draw.Rectangle>();
                if (rec.Count() > 0)
                {
                    WriteTextOnImage(rec.First(), null);
                }
                else
                {
                    WriteTextOnImage(null, null);
                }
            }
        }
        private void LoadImage(string imagePath)
        {
            try
            {
                using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        fileStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                        ImageSource =  bitmapImage;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading image: " + ex.Message);
            }
        }
        private async void LoadAllFontsAsync()
        {
            await Task.Run(() =>
            {
                InstalledFontCollection installedFontCollection = new InstalledFontCollection();
                System.Drawing.FontFamily[] fontFamilies = installedFontCollection.Families;
                Fonts = new ObservableCollection<string>(fontFamilies.Select(x => x.Name));
            });
        }
        private void LoadFontStyles()
        {
            var fontStyles = new string[3] { "Normal", "Italic", "Oblique" };
            foreach (var style in fontStyles)
            {
                FontStyles.Add(style.ToString());
            }
        }
        private void LoadFontWeight()
        {
            foreach (ImageScraper.Helpers.FontWeight weight in Enum.GetValues(typeof(ImageScraper.Helpers.FontWeight)))
            {
                FontWeight.Add(weight.ToString());
            }
        }
        private void LoadSettings()
        {
            if (_userSettings.SelectedFonts != null)
            {
                SelectedFonts = _userSettings.SelectedFonts.ToList();
            }
            SelectedColor = _userSettings.SelectedColor;
            SelectedFontWeight = _userSettings.SelectedFontWeight;
            TranslateOffset = _userSettings.TranslateOffset;
            Scale = _userSettings.Scale;
            IsAlignLeftChecked = _userSettings.IsAlignLeftChecked;
            IsAlignCenterChecked = _userSettings.IsAlignCenterChecked;
            IsAlignRightChecked = _userSettings.IsAlignRightChecked;
            IsAutoRandomColor = _userSettings.IsAutoRandomColor;
            IsRandomImage = _userSettings.IsRandomImage;
            ImagePath = _userSettings.ImagePath;
        }
        public void SaveDraw()
        {
            var itemDataList = Items.Select(item =>
            {
                var baseData = new
                {
                    Type = item.GetType().Name,
                    Top = item.Top,
                    Left = item.Left,
                    Width = item.Width,
                    Height = item.Height
                };

                if (item is ImageVisual imageVisual)
                {
                    return (object)new
                    {
                        baseData.Type,
                        baseData.Top,
                        baseData.Left,
                        baseData.Width,
                        baseData.Height,
                        ImageSource = imageVisual.ImageSource
                    };
                }

                return (object)baseData;
            }).ToList();

            string jsonData = JsonConvert.SerializeObject(itemDataList, Formatting.Indented);
            File.WriteAllText(FilePaths.drawSaved, jsonData);
        }
        private void LoadDrawSaved()
        {
            FolderAndFileHelper.CreateFile(FilePaths.drawSaved);
            string jsonData = File.ReadAllText(FilePaths.drawSaved);
            var itemDataList = JsonConvert.DeserializeObject<List<dynamic>>(jsonData);
            if (itemDataList != null)
            {
                Items.Clear();
                foreach (var itemData in itemDataList)
                {
                    Drawable newItem = null;

                    if (itemData.Type == "ImageVisual")
                    {
                        newItem = new ImageVisual
                        {
                            Top = itemData.Top,
                            Left = itemData.Left,
                            ImageSource = itemData.ImageSource,
                            Width = itemData.Width,
                            Height = itemData.Height
                        };
                    }
                    else if (itemData.Type == "Rectangle")
                    {
                        newItem = new Draw.Rectangle
                        {
                            Top = itemData.Top,
                            Left = itemData.Left,
                            Width = itemData.Width,
                            Height = itemData.Height,
                        };
                    }

                    if (newItem != null)
                    {
                        Items.Add(newItem);
                        if (newItem.GetType() == typeof(Draw.Rectangle))
                        {
                            LoadRectangleEvents();
                        }
                        if (newItem.GetType() == typeof(Draw.ImageVisual))
                        {
                            var rec = Items.OfType<Draw.Rectangle>();
                            if (rec.Count() > 0)
                            {
                                WriteTextOnImage(rec.First(), null);
                            }
                            else
                            {
                                WriteTextOnImage(null, null);
                            }
                        }
                       
                    }
                }
            }
        }
        private void KeyDown(object sender)
        {
            if (sender is KeyEventArgs)
            {
                if ((sender as KeyEventArgs).Key == Key.Delete && SelectedItem != null)
                {
                    if (SelectedItem.GetType() == typeof(Draw.Rectangle))
                    {
                        WriteTextOnImage(null, null);
                    }
                    Items.Remove(SelectedItem);
                   
                }
            }
        }
        private void AddImage()
        {
            _fileService.OpenFileDialog(out string selectedImagePath);
            if (selectedImagePath != null)
            {
                bool hasImageVisual = Items.OfType<Draw.ImageVisual>().Any();
                System.Drawing.Size imageSize = System.Drawing.Image.FromFile(selectedImagePath).Size;
                if (!hasImageVisual)
                {
                    var image = new ImageVisual
                    {
                        Top = MousePosition.Y,
                        Left = MousePosition.X,
                        ImageSource = selectedImagePath,
                        Height = imageSize.Height,
                        Width = imageSize.Width
                    };
                    Items.Add(image);
                    var rec = Items.OfType<Draw.Rectangle>();
                    if (rec.Count() > 0)
                    {
                        WriteTextOnImage(rec.First(), null);
                    }
                    else
                    {
                        WriteTextOnImage(null, null);

                    }
                }
                else
                {
                    MessageBoxService.ShowWarning("Chỉ được tạo thêm một ảnh duy nhất");
                }
            }
        }
        private void DrawRect()
        {
            bool hasRectangle = Items.OfType<Draw.Rectangle>().Any();
            if (!hasRectangle)
            {
                Items.Add(new Draw.Rectangle()
                {
                    Top = MousePosition.Y,
                    Left = MousePosition.X,
                    Width = 400,
                    Height = 200,
                });
                LoadRectangleEvents();
            }
            else
            {
                MessageBoxService.ShowWarning("Chỉ được tạo một khu vực chọn");
            }
        }
        private void BringToFront()
        {
            if (Items.Count > 0)
            {
                Items.Move(Items.IndexOf(SelectedItem), Items.Count - 1);

            }
        }
        private void Delete() 
        {
            if (SelectedItem != null)
            {
                if (SelectedItem.GetType() == typeof(Draw.Rectangle))
                {
                    WriteTextOnImage(null, null);
                }
                Items.Remove(SelectedItem);
            }
            
        }
        private void GoBack(string navigatePath)
        {
            if (navigatePath != null)
                _regionManager.RequestNavigate("ContentRegion", navigatePath);
        }
        private void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                DrawingEndedHandled = false;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //TODO Multi select deletion
                if (e.OldItems[0] == SelectedItem)
                {
                    SelectedItem = null;
                }
            }
        }
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            LoadSettings();
            LoadDrawSaved();
            if (navigationContext.Parameters.ContainsKey("IsFirstNavigate"))
            {
                IsFirstNavigate = (bool)navigationContext.Parameters["IsFirstNavigate"];
            }

        }
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            SaveDraw();
        }
    }
}