using ImageScraper.ViewModels.Base;
using System;
using System.Text.Json;
using System.Windows;

namespace ImageScraper.ViewModels.Draw
{
    public class Rectangle : Drawable, ICloneable
    {
        public delegate void RectangleEventHandler(object sender, EventArgs e);
        public event RectangleEventHandler RectangleChanged;

        //protected override void OnLeftChanged(double delta)
        //{
        //    RectangleChanged?.Invoke(this, EventArgs.Empty);
        //}

        //protected override void OnTopChanged(double delta)
        //{
        //    RectangleChanged?.Invoke(this, EventArgs.Empty);
        //}

        //protected override void OnWidthUpdated()
        //{
        //    RectangleChanged?.Invoke(this, EventArgs.Empty);
        //}

        //protected override void OnHeightUpdated()
        //{
        //    RectangleChanged?.Invoke(this, EventArgs.Empty);
        //}
        public object Clone() => JsonSerializer.Deserialize<Rectangle>(JsonSerializer.Serialize(this));
    }
}