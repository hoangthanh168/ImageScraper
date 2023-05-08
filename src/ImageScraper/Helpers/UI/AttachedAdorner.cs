using ImageScraper.Adorners;
using RichCanvas;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace ImageScraper.Helpers.UI
{
    public class AttachedAdorner
    {
        private static Adorner _currentAdorner;
        private static readonly List<ResizeAdorner> _resizeAdorner = new List<ResizeAdorner>();

        public static readonly DependencyProperty HasHoverAdornerProperty = DependencyProperty.RegisterAttached("HasHoverAdorner", typeof(bool), typeof(AttachedAdorner),
            new FrameworkPropertyMetadata(false, OnHasHoverChanged));

        public static void SetHasHoverAdorner(UIElement element, bool value) => element.SetValue(HasHoverAdornerProperty, value);

        public static bool GetHasHoverAdorner(UIElement element) => (bool)element.GetValue(HasHoverAdornerProperty);

        public static readonly DependencyProperty ShowResizeAdornerProperty = DependencyProperty.RegisterAttached("ShowResizeAdorner", typeof(bool), typeof(AttachedAdorner),
            new FrameworkPropertyMetadata(false, OnShowResizeAdornerChanged));

        public static void SetShowResizeAdorner(UIElement element, bool value) => element.SetValue(ShowResizeAdornerProperty, value);

        public static bool GetShowResizeAdorner(UIElement element) => (bool)element.GetValue(ShowResizeAdornerProperty);

        internal static void OnScrolling()
        {
            _currentAdorner?.InvalidateArrange();
            foreach (ResizeAdorner adorner in _resizeAdorner)
            {
                adorner.InvalidateArrange();
            }
        }

        private static void OnShowResizeAdornerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (RichItemContainer)d;
            var value = (bool)e.NewValue;
            if (value)
            {
                var layer = AdornerLayer.GetAdornerLayer(element);
                ResizeAdorner adorner = new ResizeAdorner(element);
                adorner.Container.Content = element;
                adorner.Container.ContentTemplate = (DataTemplate)element.FindResource("SelectedAdornerTemplate");
                layer.Add(adorner);
                _resizeAdorner.Add(adorner);
            }
            else
            {
                var layer = AdornerLayer.GetAdornerLayer(element);
                if (layer != null)
                {
                    foreach (ResizeAdorner adorner in _resizeAdorner)
                    {
                        layer.Remove(adorner);
                    }
                    _resizeAdorner.Clear();
                }
            }
        }

        private static void OnHasHoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (RichItemContainer)d;

            var value = (bool)e.NewValue;
            if (value)
            {
                element.MouseEnter += OnMouseEnter;
                element.MouseLeave += OnMouseLeave;
            }
            else
            {
                element.MouseEnter -= OnMouseEnter;
                element.MouseLeave -= OnMouseLeave;
            }
        }

        private static void OnMouseEnter(object sender, MouseEventArgs e)
        {
            var element = (RichItemContainer)sender;
            var template = (DataTemplate)element.FindResource("HoverAdornerTemplate");
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(element);
            HoverAdorner adorner = new HoverAdorner(element);
            adorner.Container.ContentTemplate = template;
            layer.Add(adorner);
            _currentAdorner = adorner;
        }

        private static void OnMouseLeave(object sender, MouseEventArgs e)
        {
            var element = (RichItemContainer)sender;
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(element);
            if (layer != null)
            {
                layer.Remove(_currentAdorner);
            }
        }
    }
}