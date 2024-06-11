using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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
    using static Properties.Resources;
    using PixelFormat = System.Drawing.Imaging.PixelFormat;

    /// <summary>
    /// MessageLog.xaml 的互動邏輯
    /// </summary>
    public partial class MessageLogView : UserControl
    {
        static MessageLogView()
        {
            WarningIcon = SystemIcons.Warning.ToBitmap().ToBitmapSource();
            ErrorIcon = SystemIcons.Error.ToBitmap().ToBitmapSource();
            InfoIcon = SystemIcons.Information.ToBitmap().ToBitmapSource();
        }
        public static ImageSource WarningIcon { get; }
        public static ImageSource ErrorIcon { get; }
        public static ImageSource InfoIcon { get; }
        System.Windows.Media.Color ActiveColor => System.Windows.Media.Color.FromArgb((byte)(255 * 0.5), 255, 165, 50);
        public MessageLogView()
        {
            InitializeComponent();
            DataContextChanged += MessageLogView_DataContextChanged;
            try
            {
                this.DataContext = EventLog.Default;
            }
            catch (Exception ex)
            {

            }
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;
            if (ShowErrorMessage) BTN_ShowError.Background = new SolidColorBrush(ActiveColor);
            if (ShowInfoMessage) BTN_ShowInfo.Background = new SolidColorBrush(ActiveColor);
            if (ShowWarningMessage) BTN_ShowWarning.Background = new SolidColorBrush(ActiveColor);

            CollectionViewSource.Filter += CollectionViewSource_Filter;
            dataGrid.ItemsSource = CollectionViewSource.View;
            dgColum.Visibility = System.Diagnostics.Debugger.IsAttached ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MessageLogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(e.OldValue is EventLog oLog)
            {
                oLog.Messages.CollectionChanged -= UpdateNumbers;
            }

            if (e.NewValue is EventLog nLog)
            {
                nLog.Messages.CollectionChanged += UpdateNumbers;
                CollectionViewSource.Source = nLog.Messages;
            }
            else
                CollectionViewSource.Source = null;

            UpdateNumbers();
        }

        private void UpdateNumbers(object sender = null, NotifyCollectionChangedEventArgs e = null)
        {
  
            if (e != null && e.NewItems != null && e.NewItems.Count > 0)
                dataGrid.ScrollIntoView(e.NewItems[0]);
        }

        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (e.Item is LogMessage msg)
            {
                switch (msg.Level)
                {
                    case MessageLevel.Error:
                        e.Accepted = ShowErrorMessage;
                        break;
                    case MessageLevel.Info:
                        e.Accepted = ShowInfoMessage;
                        break;
                    case MessageLevel.Warning:
                        e.Accepted = ShowWarningMessage;
                        break;
                    default:
                        e.Accepted = true;
                        break;
                }
            }
        }
        bool ShowErrorMessage
        {
            get => Properties.Settings.Default.ShowError;
            set
            {
                Properties.Settings.Default.ShowError = value;
                Properties.Settings.Default.Save();
            }
        }

        bool ShowInfoMessage
        {
            get => Properties.Settings.Default.ShowInfo;
            set
            {
                Properties.Settings.Default.ShowInfo = value;
                Properties.Settings.Default.Save();
            }
        }
        bool ShowWarningMessage
        {
            get => Properties.Settings.Default.ShowWarning;
            set
            {
                Properties.Settings.Default.ShowWarning = value;
                Properties.Settings.Default.Save();
            }
        }

        CollectionViewSource CollectionViewSource { get; } = new CollectionViewSource();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            bool focus;
            switch (btn.Tag as string)
            {
                case "Error":
                    focus = ShowErrorMessage = !ShowErrorMessage;
                    break;
                case "Warning":
                    focus = ShowWarningMessage = !ShowWarningMessage;
                    break;
                case "Info":
                    focus = ShowInfoMessage = !ShowInfoMessage;
                    break;
                default:
                    throw new Exception();
            }
            
            btn.Background = focus ? new SolidColorBrush(ActiveColor) : null;
            CollectionViewSource.View.Refresh();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(e.Source is FrameworkElement fe && fe.DataContext is LogMessage msg)
            {
                if(msg.StackTrace!=null)
                {
                    MessageBox.Show(msg.StackTrace);
                }
            }
        }
    }

    public class StringIsNotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return Visibility.Collapsed;
            else if (value is string str)
                return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var lv = (MessageLevel)parameter ;
            var obj = value as IEnumerable<LogMessage>;
            return obj.Where(x => x.Level == lv).Count();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class MessageLevelToImageConverter : IValueConverter
    {
        static Dictionary<MessageLevel, ImageSource> icons;
        static MessageLevelToImageConverter()
        {
            icons = new Dictionary<MessageLevel, ImageSource>();
            Add(MessageLevel.Info, SystemIcons.Information.ToBitmap());
            Add(MessageLevel.Critical, SystemIcons.Shield.ToBitmap());
            Add(MessageLevel.Debug, SystemIcons.Question.ToBitmap());
            Add(MessageLevel.Warning, SystemIcons.Warning.ToBitmap());
            Add(MessageLevel.Error, SystemIcons.Error.ToBitmap());
            void Add(MessageLevel level, Bitmap icon)
            {
                System.Drawing.Size iconSize = new System.Drawing.Size(16, 16);
                Bitmap bitmap = new Bitmap(iconSize.Width, iconSize.Height);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(icon, new System.Drawing.Rectangle(System.Drawing.Point.Empty, iconSize));
                }
                icons.Add(level, bitmap.ToBitmapSource());
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is MessageLevel level && icons.ContainsKey(level))
                return icons[level];
            else
                return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
