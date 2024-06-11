using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace DeepWise.Controls.Interactivity
{
    public abstract class InteractiveObject : INotifyPropertyChanged
    {
        public static Color IdleColor => Color.FromRgb(0, 120, 215);
        public static Color HightlightColor => Color.FromRgb(173, 255, 47);
        public abstract bool IsMouseOver(DisplayMouseEventArgs p);
        public virtual bool Selectable { get; set; } = true;
        public bool Focused
        {
            get => _focused;
            set
            {
                if (_focused != value)
                {
                    _focused = value;
                    OnFocusedChanged();
                }
            }
        }
        public virtual void UpdateLayout(double zoomLevel) { }

        protected virtual void OnFocusedChanged()
        {
            var clr = Focused ? HightlightColor : IdleColor;
            var contourBrush  = new SolidColorBrush(clr);
            clr.A = 80;
            var fillBrush = new SolidColorBrush(clr);
            foreach (var item in ShapeElements)
            {
                switch(item.Tag)
                {
                    case "contour":
                        item.Stroke = contourBrush;
                        break;
                    case "fill":
                        item.Fill = fillBrush;
                        break;
                    default:
                        item.Stroke = contourBrush;
                        item.Fill = fillBrush;
                        break;
                }
        
     
            }
        }

        public List<System.Windows.Shapes.Shape> ShapeElements { get; } = new List<System.Windows.Shapes.Shape>();
        public virtual void MouseDown(DisplayMouseEventArgs e)
        {
            StartLocation = e.Location;
        }
        protected Point StartLocation { get; set; }
        public virtual System.Windows.Input.Cursor Cursor => Cursors.Hand;

        public virtual void MouseMove(DisplayMouseEventArgs e) { }
        public virtual void MouseUp(DisplayMouseEventArgs e) { }

        bool _focused = false;
        public object Tag { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public abstract class InteractiveROI : InteractiveObject
    {
        public static new Color IdleColor => Color.FromRgb(0, 120, 215);
        public static new Color HightlightColor => Color.FromRgb(173, 255, 47);
        public abstract DeepWise.Shapes.IShape GetRegion();
        public abstract void SetRegion(DeepWise.Shapes.IShape shape);
        public abstract void SetRegion(Point from,Point to);
        protected void InitializeDefaultColor()
        {
            foreach (var shape in ShapeElements)
            {
                switch(shape.Tag)
                {
                    case "contour":
                        shape.Stroke = new SolidColorBrush(Color.FromRgb(0, 120, 215));
                        shape.Fill = null;
                        break;
                    case "fill":
                        shape.Stroke = null;
                        shape.Fill = new SolidColorBrush(Color.FromArgb(80, 0, 120, 215));
                        break;
                    case "fill(contour)":
                        shape.Stroke = new SolidColorBrush(Color.FromArgb(80, 0, 120, 215));
                        shape.Fill = null;
                        break;
                    default:
                        shape.Stroke = new SolidColorBrush(Color.FromRgb(0, 120, 215));
                        shape.Fill = new SolidColorBrush(Color.FromArgb(80, 0, 120, 215));
                        break;
                }
                
            }
        }
        protected override void OnFocusedChanged()
        {
            foreach (var shape in ShapeElements)
            {
                var clr = Focused ? HightlightColor : IdleColor;
                var contourBrush = new SolidColorBrush(clr);
                clr.A = 80;
                var fillBrush = new SolidColorBrush(clr);
                switch (shape.Tag)
                {
                    case "contour":
                        shape.Stroke = contourBrush;
                        break;
                    case "fill":
                        shape.Fill = fillBrush;
                        break;
                    case "fill(contour)":
                        shape.Stroke = fillBrush;
                        break;
                    default:
                        shape.Stroke = contourBrush;
                        shape.Fill = fillBrush;
                        break;
                }
            }
        }
    }
}
