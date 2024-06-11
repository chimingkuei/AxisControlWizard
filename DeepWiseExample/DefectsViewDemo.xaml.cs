using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace DeepWise.Test
{
    using DeepWise.Controls;
    using DeepWise.Controls.Interactivity;
    using DeepWise.Controls.Interactivity.BehaviorControllers;
    using DeepWise.Data;
    using DeepWise.Devices;
    using DeepWise.Metrology;
    using Newtonsoft.Json;
    using OpenCvSharp.Extensions;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>
    /// DisplayDemo.xaml 的互動邏輯
    /// </summary>
    [Group("互動")]
    [Description("此範例提展示如何檢視以及標註瑕疵。")]
    public partial class DefectsViewDemo : Window
    {
        public DefectsViewDemo()
        {
            InitializeComponent();
            InitializeDefects();
        }

        void InitializeDefects()
        {
            for (int i = 0; i < 6; i++)
            {
                var img = new Bitmap(2048, 2048);
                Random random = new Random(Guid.NewGuid().GetHashCode());
                var count = random.Next(0, 20);
                var ps = new int[count].Select(x => new PointF((float)random.NextDouble(), (float)random.NextDouble())).ToArray();
                var size = new int[count].Select(x => (float)random.NextDouble()).ToArray();
                var type = new int[count].Select(x => random.Next(2)).ToArray();
                using (var g = Graphics.FromImage(img))
                {
                    g.Clear(Color.Black);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    var bkBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new PointF(0, 0), new PointF(2048, 2048), System.Drawing.Color.AliceBlue, System.Drawing.Color.Pink);
                    g.FillRectangle(bkBrush, new Rectangle(0, 0, 2048, 2048));
                    for (int j = 0; j < count; j++)
                    {
                        float w = size[j] * 100 + 50;
                        var p = ps[j];
                        System.Drawing.Color c;
                        if (type[j] == 0)
                            c = System.Drawing.Color.FromArgb(140, 0, 0, 0);
                        else
                            c = System.Drawing.Color.FromArgb(200, 0, 100, 0);

                        g.FillEllipse(new System.Drawing.SolidBrush(c), new RectangleF(p.X * 2048 - w / 2, p.Y * 2048 - w / 2, w, w));
                    }
                }
                var item = new ResultImageInfo(img, i.ToString());
                this.Images.Add(item);

                var box = new Shapes.Rect(0, 0, 2048, 2048);
                for (int j = 0; j < count; j++)
                {
                    float w = size[j] * 100 + 50;
                    var p = ps[j];
                    item.Defects.Add(new Defect(type[j] == 0 ? "A" : "B", Shapes.Rect.Intersect(box, new Shapes.Rect(p.X * 2048 - w / 2, p.Y * 2048 - w / 2, w, w))));
                }
        
            }

            display.Behavior = (defectBrowser = new DefectBrowser());
            listView.ItemsSource = Images;
            listView.SelectedIndex = 0;
        }
        DefectBrowser defectBrowser;
       
        List<ResultImageInfo> Images { get; } = new List<ResultImageInfo>();
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

        }

        public Dictionary<string, System.Windows.Media.Brush> DefectColor { get; } = new Dictionary<string, System.Windows.Media.Brush>()
        {
            {"A", System.Windows.Media.Brushes.Red },
            {"B", System.Windows.Media.Brushes.Blue },
            {"C", System.Windows.Media.Brushes.Green },
            {"D", System.Windows.Media.Brushes.Magenta },
        };

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                var item = (listView.SelectedItems[0] as ResultImageInfo);
                defectBrowser.Defects = item.Defects;
#if MAT
                display.Image = item.Image.ToMat();
#else
                display.Image = item.Image;
#endif
            }
        }

        private async void AddDefect(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            (btn.Parent as FrameworkElement).IsEnabled = false;
            try
            {
                switch (btn.Name)
                {
                    case nameof(btn_AddA):
                        await defectBrowser.Add("A");
                        break;
                    case nameof(btn_AddB):
                        await defectBrowser.Add("B");
                        break;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            (btn.Parent as FrameworkElement).IsEnabled = true;
        }
    }

    public class DefectBrowser : DisplayBehavior
    {
        public Dictionary<string, System.Windows.Media.Brush> DefectColor { get; } = new Dictionary<string, System.Windows.Media.Brush>()
        {
            {"A", System.Windows.Media.Brushes.Red },
            {"B", System.Windows.Media.Brushes.Blue },
            {"C", System.Windows.Media.Brushes.Green },
            {"D", System.Windows.Media.Brushes.Magenta },
        };

        public override void Enter(Display display)
        {
            base.Enter(display);
            InitailizeMarks();
        }
        void InitailizeMarks()
        {
            Display.InteractiveObjects.Clear();
            if (Defects != null)
            {
                foreach (var defect in Defects)
                {
                    var mark = new RectMark(defect.Region, $"{defect.Name} : {defect.Region.Width}x{defect.Region.Height}") { Tag = defect };
                    if (DefectColor.ContainsKey(defect.Name))
                        mark.Stroke = DefectColor[defect.Name];
                    Display.InteractiveObjects.Add(mark);
                }
            }
        }
        List<Defect> _defects;
        public List<Defect> Defects
        {
            get => _defects;
            set
            {
                if (_defects == value) return;
                _defects = value;
                InitailizeMarks();
            }
        }

        public override void Exist()
        {
            base.Exist();
        }

        public override void MouseDown(DisplayMouseEventArgs e)
        {
            //throw new NotImplementedException();
            if(e.RightButton == MouseButtonState.Pressed)
            {
                foreach (var defect in Defects ?? new List<Defect>())
                {
                    if(defect.Region.Contains(e.Location))
                    {
                        var menu = new ContextMenu();
                        var item = new MenuItem() { Header = "刪除" };
                        item.Click += (s2, e2) =>
                        {
                            Defects.Remove(defect);
                            Display.InteractiveObjects.Remove(Display.InteractiveObjects.FirstOrDefault(x => x.Tag == defect));
                        };
                        menu.Items.Add(item);
                        menu.IsOpen = true;
                        break;
                    }
                }
            }
        }

        public override void MouseMove(DisplayMouseEventArgs e)
        {
            //throw new NotImplementedException();
        }

        public override void MouseUp(DisplayMouseEventArgs e)
        {
            //throw new NotImplementedException();
        }


        public async Task Add(string defectType = null)
        {
            var cropper = new ShapeCropper<DeepWise.Shapes.Rect>();
            Display.Behavior = cropper;
            if(await cropper.WaitResult())
            {
                var rect = cropper.Region;
                Defects.Add(new Defect(defectType, rect));
            }
            Display.Behavior = this;
        }
    }

    public class ResultImageInfo
    {
        public ResultImageInfo()
        {
            Defects = new List<Defect>();
        }
        public ResultImageInfo(Bitmap image, string name, List<Defect> defects = null)
        {
            Name = name;
            Image = image;
            Defects = defects ?? new List<Defect>();
        }
        public string Name { get; set; }
        [JsonConverter(typeof(BitmapJsonConverter))]
        public Bitmap Image { get; set; }

        ImageSource _thumbnail;
        [Browsable(false), JsonIgnore]
        public ImageSource Thumbnail
        {
            get
            {
                if (Image == null) return null;
                if (_thumbnail == null) _thumbnail = (new System.Drawing.Bitmap(Image, new Size(Image.Width / 10, Image.Height / 10))).ToBitmapSource();
                return _thumbnail;
            }
        }

        public List<Defect> Defects { get; }
    }
}
