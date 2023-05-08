using ImageScraper.Mvvm;
using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace ImageScraper.ViewModels.Base
{
    public abstract class Drawable : ViewModelBase
    {
        private double _top;
        private double _left;
        private bool _isSelected;
        private bool _isMultiSelect;
        private bool _isSelectable = true;
        private bool _isDraggable = true;
        private double _width;
        private double _height;
        private bool _shouldBringIntoView;
        private Point _directionPoint;
        private DelegateCommand<double> leftChangedCommand;
        private DelegateCommand<double> topChangedCommand;
        private double _angle = 0;
        private bool _hasCustomBehavior;

        public double Angle
        {
            get => _angle;
            set
            {
                SetProperty(ref _angle, value);
                OnRotationChanged();
            }
        }

        public double Top
        {
            get => _top;
            set 
            {
                SetProperty(ref _top, value);
                OnTopChanged(_top);
            }
        }

        public double Left
        {
            get => _left;
            set 
            { 
                SetProperty(ref _left, value);
                OnLeftChanged(_left);
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);
                OnIsSelectedChanged(value);
            }
        }

        public bool IsMultiSelect
        {
            get => _isMultiSelect;
            set
            {
                SetProperty(ref _isMultiSelect, value);
                OnIsSelectedChanged(value);
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                SetProperty(ref _width, Math.Round(value));
                OnWidthUpdated();
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                SetProperty(ref _height, Math.Round(value));
                OnHeightUpdated();
            }
        }

        public bool IsSelectable
        {
            get => _isSelectable;
            set => SetProperty(ref _isSelectable, value);
        }

        public bool IsDraggable
        {
            get => _isDraggable;
            set => SetProperty(ref _isDraggable, value);
        }

        public bool HasCustomBehavior
        {
            get => _hasCustomBehavior;
            set => SetProperty(ref _hasCustomBehavior, value);
        }

        public bool ShouldBringIntoView
        {
            get => _shouldBringIntoView;
            set => SetProperty(ref _shouldBringIntoView, value);
        }

        public Point Scale
        {
            get => _directionPoint;
            set => SetProperty(ref _directionPoint, value);
        }

        public ICommand LeftChangedCommand => leftChangedCommand ?? new DelegateCommand<double>(OnLeftChanged);

        public ICommand TopChangedCommand => topChangedCommand ?? new DelegateCommand<double>(OnTopChanged);

        public Drawable()
        {
            Scale = new Point(1, 1);
        }

        protected virtual void OnLeftChanged(double delta)
        { }

        protected virtual void OnTopChanged(double delta)
        { }

        protected virtual void OnWidthUpdated()
        { }

        protected virtual void OnHeightUpdated()
        { }

        protected virtual void OnRotationChanged()
        { }

        protected virtual void OnIsSelectedChanged(bool value)
        { }

        public virtual void OnDrawingEnded(Action<object> callback = default)
        { }
    }
}