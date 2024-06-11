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
    public class RectROI : InteractiveROI
    {
        DeepWise.Shapes.Rect _region ;
        public DeepWise.Shapes.Rect Region
        {
            get => _region;
            set
            {
                _region = value;
                Rect.SetLocation(value.Left, value.Top);

                Rect.Width = value.Width;
                Rect.Height = value.Height;
            }
        }
        public RectROI(DeepWise.Shapes.Rect rect)
        {
            Rect.Stroke = new SolidColorBrush(Color.FromRgb(0, 120, 215));
            Rect.Fill = new SolidColorBrush(Color.FromArgb(80, 0, 120, 215));
            ShapeElements.Add(Rect);
            Region = rect;
        }
        public RectROI(System.Windows.Rect rect)
        {
            Rect.Stroke = new SolidColorBrush(Color.FromRgb(0, 120, 215));
            Rect.Fill = new SolidColorBrush(Color.FromArgb(80, 0, 120, 215));
            ShapeElements.Add(Rect);
            Region = new Shapes.Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public RectROI(double x, double y, double width, double height) : this(new System.Windows.Rect(x, y, width, height)) { }

        Point rb => new Point(_region.Right, _region.Bottom);
        Point lb => new Point(_region.Left, _region.Bottom);
        Point rt => new Point(_region.Right , _region.Top);
        Point lt => _region.Location;
        public override bool IsMouseOver(DisplayMouseEventArgs e)
        {
            var p = e.Location;
            if(Focused)
            {
                if ((p - lt).Length <= e.LineHitRadius)
                {
                    selected = Element.LeftTop;
                    return true;
                }
                else if ((p - rt).Length <= e.LineHitRadius)
                {
                    selected = Element.RightTop;
                    return true;
                }
                else if ((p - lb).Length <= e.LineHitRadius)
                {
                    selected = Element.LeftBottom;
                    return true;
                }
                else if ((p - rb).Length <= e.LineHitRadius)
                {
                    selected = Element.RightBottom;
                    return true;
                }
                else if (DeepWise.Shapes.Geometry.GetDistance(p, new DeepWise.Shapes.Segment(lt, lb), out _) <= e.LineHitRadius)//Left
                {
                    selected = Element.Edge_Left;
                    return true;
                }
                else if (DeepWise.Shapes.Geometry.GetDistance(p, new Shapes.Segment(lt, rt), out _) <= e.LineHitRadius)//Top
                {
                    selected = Element.Edge_Top;
                    return true;
                }
                else if (DeepWise.Shapes.Geometry.GetDistance(p, new Shapes.Segment(lb, rb), out _) <= e.LineHitRadius)//Bottom
                {
                    selected = Element.Edge_Bottom;
                    return true;
                }
                else if (DeepWise.Shapes.Geometry.GetDistance(p, new Shapes.Segment(rb, rt), out _) <= e.LineHitRadius)//Right
                {
                    selected = Element.Edge_Right;
                    return true;
                }
            }
            if (_region.Contains(p))
            {
                selected = Element.Center;
                return true;
            }
            return false;
        }
        public Element selected { get; protected set; }

        public override Cursor Cursor
        {
            get
            {
                switch (selected)
                {
                    case Element.Center:
                        return Cursors.SizeAll;
                    case Element.Edge_Top:
                    case Element.Edge_Bottom:
                        return Cursors.SizeNS;
                    case Element.Edge_Left:
                    case Element.Edge_Right:
                        return Cursors.SizeWE;
                    case Element.LeftTop:
                    case Element.RightBottom:
                        return Cursors.SizeNWSE;
                    case Element.RightTop:
                    case Element.LeftBottom:
                        return Cursors.SizeNESW;
                    default:
                        return base.Cursor;
                }
            }
        }

        public enum Element
        {
            Center,
            Edge_Right, 
            Edge_Left, 
            Edge_Top, 
            Edge_Bottom,
            LeftTop,
            RightTop,
            LeftBottom,
            RightBottom,
        }

        Shapes.Point anchor;
        public override void MouseDown(DisplayMouseEventArgs e)
        {
            base.MouseDown(e);
            switch (selected)
            {
                case Element.LeftTop:
                case Element.Edge_Left:
                case Element.Edge_Top:
                    anchor = new Shapes.Point(_region.Right, _region.Bottom); break;
                case Element.RightBottom: 
                case Element.Edge_Right: 
                case Element.Edge_Bottom: 
                case Element.Center: 
                    anchor = _region.Location; break;
                case Element.LeftBottom: anchor = new Shapes.Point(_region.Right, _region.Top); break;
                case Element.RightTop: anchor = new Shapes.Point(_region.Left, _region.Bottom); break;
            }
        }
        public override void MouseMove(DisplayMouseEventArgs e)
        {
            Point p = e.Location;
        Str:
            switch (selected)
            {
                case Element.RightBottom:
                case Element.LeftBottom:
                case Element.RightTop:
                case Element.LeftTop:
                    {
                        var r = p.X >= anchor.X;
                        var b = p.Y >= anchor.Y;
                        if (r & b)
                        {
                            if (selected == Element.RightBottom)
                            {
                                Region = Shapes.Rect.FromBoundary(anchor.X, anchor.Y, p.X, p.Y);
                                return;
                            }
                            selected = Element.RightBottom;
                            goto Str;
                        }
                        else if (b)
                        {
                            if (selected == Element.LeftBottom)
                            {
                                Region = Shapes.Rect.FromBoundary(p.X, anchor.Y, anchor.X, p.Y);
                                return;
                            }
                            selected = Element.LeftBottom;
                            goto Str;
                        }
                        else if (r)
                        {
                            if (selected == Element.RightTop)
                            {
                                Region = Shapes.Rect.FromBoundary(anchor.X, p.Y, p.X, anchor.Y);
                                return;
                            }
                            selected = Element.RightTop;
                            goto Str;
                        }
                        else
                        {
                            if (selected == Element.LeftTop)
                            {
                                Region = Shapes.Rect.FromBoundary(p.X, p.Y, anchor.X, anchor.Y);
                                return;
                            }
                            selected = Element.LeftTop;
                            goto Str;
                        }
                    }
                case Element.Edge_Bottom:
                    {
                        double lenth = p.Y - _region.Top;
                        var rect = _region;
                        if(lenth > 0)
                        {
                            rect.Height = lenth;
                            Region = rect;
                        }
                        else
                        {
                            rect.Height = -lenth;
                            rect.Y = rect.Y + lenth;
                            Region = rect;
                            selected = Element.Edge_Top;
                        }
                        break;
                    }
                case Element.Edge_Top:
                    {
                        var rect = _region;
                        double length = rect.Bottom - p.Y;
                        if (length>0)
                        {
                            rect.Y = p.Y;
                            rect.Height = length;
                        }
                        else
                        {
                            rect.Y = _region.Bottom;
                            rect.Height = -length;
                            selected = Element.Edge_Bottom;
                        }
                        Region = rect;
                        break;
                    }

                case Element.Edge_Left:
                    {
                        var rect = _region;
                        double length = rect.Right - p.X;
                        if (length > 0)
                        {
                            rect.X = p.X;
                            rect.Width = length;
                        }
                        else
                        {
                            rect.X = _region.Right;
                            rect.Width = -length;
                            selected = Element.Edge_Right;
                        }
                        Region = rect;
                        break;
                    }
                case Element.Edge_Right:
                    {
                        double lenth = p.X - _region.Left;
                        var rect = _region;
                        if (lenth > 0)
                        {
                            rect.Width = lenth;
                            Region = rect;
                        }
                        else
                        {
                            rect.Width = -lenth;
                            rect.X = rect.X + lenth;
                            Region = rect;
                            selected = Element.Edge_Left;
                        }
                        break;
                    }
                case Element.Center:
                    {
                        var rect = _region;
                        rect.Location = anchor + (e.Location - StartLocation);
                        Region = rect;
                        break;
                    }
            }
        }
        public override void MouseUp(DisplayMouseEventArgs e)
        {
            base.MouseUp(e);
        }

        public override Shapes.IShape GetRegion() => Region;

        public override void SetRegion(Shapes.IShape shape) => Region = (Shapes.Rect)shape;

        public override void SetRegion(Point from, Point to)
        {
            double left = Math.Min(from.X, to.X);
            double top = Math.Min(from.Y, to.Y);
            Region = new Shapes.Rect(left, top, Math.Abs(to.X - from.X), Math.Abs(to.Y - from.Y));
        }

        System.Windows.Shapes.Rectangle Rect = new System.Windows.Shapes.Rectangle();
    }
}
