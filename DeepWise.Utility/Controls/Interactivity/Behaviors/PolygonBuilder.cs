using DeepWise.Controls.Interactivity.ROI;
using DeepWise.Shapes;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Path = System.Windows.Shapes.Polyline;
namespace DeepWise.Controls.Interactivity.BehaviorControllers
{
    public class PolygonBuilder : DisplayBehavior 
    {
        public PolygonBuilder()
        {
            vertexs.CollectionChanged += Vertexs_CollectionChanged;
        }

        private void Vertexs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var p = (DeepWise.Shapes.Point)e.NewItems[0];
                    path.Points.Add(p);
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    path.Points.RemoveAt(e.OldStartingIndex);
                    break;
            }
            if(vertexs.Count>0)
            {
                var last = vertexs.Last();
                closeLine.X1 = last.X;
                closeLine.Y1 = last.Y;
                var first = vertexs.First();
                closeLine.X2 = first.X;
                closeLine.Y2 = first.Y;
            }
            closeLine.Visibility = vertexs.Count > 2 ?  System.Windows.Visibility.Visible: System.Windows.Visibility.Collapsed;
        }

        public override bool ShowButtons => true;

        InteractiveROI inavROI;
        public override void MouseDown(DisplayMouseEventArgs e)
        {
            switch(e.ChangedButton)
            {
                case System.Windows.Input.MouseButton.Left:
                    {
                        for (int i = 0; i < vertexs.Count; i++)
                        {
                            if (e.IsMouseOver(vertexs[i]))
                            {
                                selectedVertexIndex = i;
                                return;
                            }
                        }

                        selectedVertexIndex = -1;
                        vertexs.Add(e.Location);

                        BTN_Checked.IsEnabled = vertexs.Count > 2;
                        e.Cancel = true;
                        break;
                    }
                    break;
                case System.Windows.Input.MouseButton.Right:
                    foreach (var vertex in vertexs)
                    {
                        
                        if (e.IsMouseOver(vertex))
                        {
                            var menu = new ContextMenu();
                            var item = new MenuItem() { Header = "刪除" };
                            item.Click += (s2, e2) =>
                            {
                               vertexs.Remove(vertex);
                                BTN_Checked.IsEnabled = vertexs.Count > 2;
                            };
                            menu.Items.Add(item);
                            menu.IsOpen = true;
                            break;
                        }
                    }
                    break;
            }    
        }

        public override void MouseMove(DisplayMouseEventArgs e)
        {
            if (selectedVertexIndex != -1 && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                vertexs[selectedVertexIndex] = e.Location;
                path.Points[selectedVertexIndex] = e.Location;
                //=============================
                e.Cancel = true;
                return;
            }

            foreach (var p in vertexs)
            {
                if (e.IsMouseOver(p))
                {
                    Display.Cursor = Cursors.Hand;
                    e.Cancel = true;
                    return;
                }
            }


        }
        public override void MouseUp(DisplayMouseEventArgs e)
        {
            
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left && selectedVertexIndex != -1)
            {
                selectedVertexIndex = -1;
            }
        }
        public override void Enter(Display display)
        {
            base.Enter(display);
            //display.InteractiveObjects.Clear();
            path.StrokeThickness = 1 / display.ZoomLevel;
            closeLine.StrokeThickness = 1 / display.ZoomLevel;
            display.canvas.Children.Add(path);
            display.canvas.Children.Add(closeLine);
        }
        Path path = new Path() { Stroke = System.Windows.Media.Brushes.Red, StrokeThickness = 1 };
        System.Windows.Shapes.Line closeLine = new System.Windows.Shapes.Line() { Stroke = System.Windows.Media.Brushes.Magenta, StrokeThickness = 1,  Visibility =  System.Windows.Visibility.Collapsed};
        public override void Exist()
        {
            base.Exist();
            Display.canvas.Children.Remove(path);
            Display.canvas.Children.Remove(closeLine);
        }
        
        public Polygon Polygon => new Polygon(vertexs);
        //==================


        ObservableCollection<Point> vertexs { get; } = new ObservableCollection<Point>();
        int selectedVertexIndex = -1;


      
       
    }
}
