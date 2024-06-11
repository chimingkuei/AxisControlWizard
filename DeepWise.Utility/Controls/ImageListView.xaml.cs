using DeepWise.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeepWise.Controls
{
    /// <summary>
    /// ImageListView.xaml 的互動邏輯
    /// </summary>
    public partial class ImageListView : UserControl
    {
        public ImageListView()
        {
            InitializeComponent();
        }
        public ImageListView(ImageList list) : this()
        {
            ImageList = list;
        }

 

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        public ImageList ImageList
        {
            get => (ImageList)GetValue(ImageListProperty);
            set => SetValue(ImageListProperty, value);
        }
        public static readonly DependencyProperty ImageListProperty = DependencyProperty.Register(nameof(ImageList), typeof(ImageList), typeof(ImageListView));
    }

    public class ImageList : ObservableCollection<ImageListItem>
    {

        public void Add(string name,Bitmap value)
        {
            var match = this.FirstOrDefault(x => x.Name == name);
            if (match != null)
            {
                match.Image = value;
            }
            else
                this.Add(new ImageListItem(name, value));
        }

        private Window win;
        public void ShowWindow(Window owner = null)
        {
            if(win==null)
            {
                win = new Window()
                {
                    Owner = owner,
                };
                win.Content = new ImageListView(this);
            }
            win.Show();
        }
    }

    public class ImageListItem : INotifyPropertyChanged
    {
   
        public ImageListItem(string name,Bitmap image)
        {
            Name = name;
            _image = image;
        }
        public string Name { get; set; }
        public Bitmap Image
        {
            get => _image;
            set
            {
                _image = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Thumbnail));
            }
        }

        void NotifyPropertyChanged([CallerMemberName] string name ="")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource Thumbnail
        {
            get
            {
                if (_thumbnail == null) _thumbnail = new Bitmap(Image, new System.Drawing.Size(250, 200)).ToBitmapSource();
                return _thumbnail;
            }
        }

        public List<IDefect> Defects { get; } = new List<IDefect> { };

        Bitmap _image;
        ImageSource _thumbnail;
    }

    
}
