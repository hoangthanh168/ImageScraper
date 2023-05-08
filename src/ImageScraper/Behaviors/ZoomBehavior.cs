using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageScraper.Behaviors
{
    public class ZoomBehavior
    {
        public static readonly DependencyProperty EnableZoomProperty =
            DependencyProperty.RegisterAttached(
                "EnableZoom",
                typeof(bool),
                typeof(ZoomBehavior),
                new UIPropertyMetadata(false, OnEnableZoomChanged));

        public static bool GetEnableZoom(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableZoomProperty);
        }

        public static void SetEnableZoom(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableZoomProperty, value);
        }

        private static void OnEnableZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                bool isEnabled = (bool)e.NewValue;
                if (isEnabled)
                {
                    scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
                }
                else
                {
                    scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
                }
            }
        }

        private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (sender is ScrollViewer scrollViewer)
                {
                    if (scrollViewer.Content is Viewbox viewbox)
                    {
                        if (e.Delta > 0)
                        {
                            // Zoom in
                            viewbox.Stretch = Stretch.Uniform;
                            viewbox.Width = viewbox.ActualWidth * 1.1;
                            viewbox.Height = viewbox.ActualHeight * 1.1;
                        }
                        else
                        {
                            // Zoom out
                            viewbox.Stretch = Stretch.Uniform;
                            viewbox.Width = viewbox.ActualWidth * 0.9;
                            viewbox.Height = viewbox.ActualHeight * 0.9;
                        }

                        e.Handled = true;
                    }
                }
            }
        }
    }
}