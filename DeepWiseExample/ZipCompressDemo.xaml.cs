using DeepWise.Controls;
using DeepWise.Data;
using DeepWise.Localization;
using DeepWise.Shapes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DeepWise.Test
{

    [Group("Application")]
    public partial class ZipCompressDemo : Window, IProgress<double>
    {
        //繼承至Config的物件位在實例化時會去自動讀取檔案。其中建構式引述代表其檔案路徑(相對或絕對)。

        public ZipCompressDemo()
        {
            
            InitializeComponent();
            try
            {
                TargetPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\source\repos";
            }
            catch (Exception ex)
            {
                MessageBox.ShowException(ex);
            }
        }

        static void OnPropertyChanged(DependencyObject sender,DependencyPropertyChangedEventArgs e)
        {
            var s = sender as ZipCompressDemo;
            try
            {
                s.listView.ItemsSource = Directory.GetDirectories(s.TargetPath).Select(x => new CompressInfo(x)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.ShowException(ex);
            }
        }

        public static readonly DependencyProperty TargetPathProperty = DependencyProperty.Register(nameof(TargetPath), typeof(string), typeof(ZipCompressDemo),new PropertyMetadata(OnPropertyChanged));

        public string TargetPath
        {
            get => (string)GetValue(TargetPathProperty);
            set => SetValue(TargetPathProperty, value);
        }

        public void Report(double value)
        {
            Dispatcher.Invoke(() => progressBar.Value = value*100);
        }
        bool compressing = false;
        public bool IgnoreBin { get; set; } = true;
        public bool IgnoreObj { get; set; } = true;
        public bool IgnoreVs { get; set; } = true;
        public bool IgnoreGit { get; set; } = true;
        bool Filter(string s)
        {
            if (IgnoreGit && s.Contains(@"\.git\")) return false;
            if (IgnoreVs && s.Contains(@"\.vs\")) return false;
            if (IgnoreBin && s.Contains(@"\bin\")) return false;
            if (IgnoreObj && s.Contains(@"\obj\")) return false;
            return true;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            using(var dlg = new SaveFileDialog())
            {
                dlg.Filter = "ZIP File|*.zip";
                dlg.Title = "Express as an .zip File";
                var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\source\repos";
                dlg.InitialDirectory = path;
                dlg.RestoreDirectory = true;
                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    (sender as System.Windows.Controls.Button).IsEnabled = false;
                    listView.IsEnabled = false;
                    compressing = true;
                    await Task.Run(() => System.IO.Compression.ZipFileExt.CreateFromDirectories(path, dlg.FileName, System.IO.Compression.CompressionLevel.Optimal, false, Encoding.UTF8, Filter, this, listView.ItemsSource.Cast<CompressInfo>().Where(x => x.Compress).Select(x => x.Directory).ToArray()));
                    listView.IsEnabled = true;
                    compressing = false;
                    (sender as System.Windows.Controls.Button).IsEnabled = true;
                    MessageBox.Show("compression finished!");
                    System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + dlg.FileName + "\"");
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if(compressing)
                e.Cancel = true;
            base.OnClosing(e);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using(var dlg = new FolderBrowserDialog())
            {
                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TargetPath = dlg.SelectedPath;
                }    
            }
        }
    }

    public class CompressInfo
    {
        public CompressInfo(string path)
        {
            Path = path;
        }
        public string Path { get; }
        public bool Compress { get; set; }
        public string Directory => System.IO.Path.GetFileName(Path);
    }

}
