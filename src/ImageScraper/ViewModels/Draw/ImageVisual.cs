using ImageScraper.ViewModels.Base;
using System;

namespace ImageScraper.ViewModels.Draw
{
    public class ImageVisual : Drawable
    {
       

        private string _imageSource;

        public string ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }
    }
}