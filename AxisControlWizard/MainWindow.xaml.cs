using MotionControllers.Motion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using static AxisControlWizard.BaseLogRecord;
using OpenCvSharp.Dnn;
using Basler.Pylon;

namespace AxisControlWizard
{
    #region Config Class
    public class SerialNumber
    {
        [JsonProperty("Gain_val")]
        public string Gain_val { get; set; }
        [JsonProperty("ExposureTime_val")]
        public string ExposureTime_val { get; set; }
        [JsonProperty("Gamma_val")]
        public string Gamma_val { get; set; }

        [JsonProperty("Save_Image_Path_val")]
        public string Save_Image_Path_val { get; set; }
        [JsonProperty("Row_val")]
        public string Row_val { get; set; }
        [JsonProperty("Row_Gap_val")]
        public string Row_Gap_val { get; set; }
        [JsonProperty("Column_val")]
        public string Column_val { get; set; }
        [JsonProperty("Column_Gap_val")]
        public string Column_Gap_val { get; set; }
        [JsonProperty("Start_X_val")]
        public string Start_X_val { get; set; }
        [JsonProperty("Start_Y_val")]
        public string Start_Y_val { get; set; }
    }

    public class Model
    {
        [JsonProperty("SerialNumbers")]
        public SerialNumber SerialNumbers { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("Models")]
        public List<Model> Models { get; set; }
    }
    #endregion

    public partial class MainWindow : System.Windows.Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Function
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("請問是否要關閉？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void MotionInit()
        {
            Controller = new ADLINK_Motion("Test");
            try
            {
                Controller.Initial();
                try
                {
                    Controller.LoadParamFromFile(@"param_1110913_motionconfig.xml");
                }
                catch
                {
                    MessageBox.Show("讀取軸控初始設定失敗");
                }
                Controller.Monitor.Start();
                Controller.SetServoAll(false);
                Controller.SetServoAll(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                //throw ex;
            }
            //===========================================
            this.DataContext = Controller;

        }

        private Dictionary<string, double> GetCameraParameterRage()
        {
            Dictionary<string, double> camera_parameter = new Dictionary<string, double>();
            Gain.Minimum = Cam.camera.Parameters[PLCamera.Gain].GetMinimum();
            Gain.Maximum = Cam.camera.Parameters[PLCamera.Gain].GetMaximum();
            camera_parameter.Add("Gain Min", (double)Gain.Minimum);
            camera_parameter.Add("Gain Max", (double)Gain.Maximum);
            Gain_Tip.Text = "min:" + Gain.Minimum.ToString() + ", max:" + Gain.Maximum.ToString();
            ExposureTime.Minimum = Cam.camera.Parameters[PLCamera.ExposureTime].GetMinimum();
            ExposureTime.Maximum = Cam.camera.Parameters[PLCamera.ExposureTime].GetMaximum();
            camera_parameter.Add("ExposureTime Min", (double)ExposureTime.Minimum);
            camera_parameter.Add("ExposureTime Max", (double)ExposureTime.Maximum);
            ExposureTime_Tip.Text = "min:" + ExposureTime.Minimum.ToString() + ", max:" + ExposureTime.Maximum.ToString();
            Gamma.Minimum = Cam.camera.Parameters[PLCamera.Gamma].GetMinimum();
            Gamma.Maximum = Cam.camera.Parameters[PLCamera.Gamma].GetMaximum();
            camera_parameter.Add("Gamma Min", (double)Gamma.Minimum);
            camera_parameter.Add("Gamma Max", (double)Gamma.Maximum);
            Gamma_Tip.Text = "min:" + Gamma.Minimum.ToString() + ", max:" + Gamma.Maximum.ToString();
            return camera_parameter;
        }

        private void CameraParameterInit()
        {
            Dictionary<string, double> camera_parameter = GetCameraParameterRage();
            // Load Camera Parameter
            if (Convert.ToDouble(Gain.Text) >= camera_parameter["Gain Min"] && Convert.ToDouble(Gain.Text) <= camera_parameter["Gain Max"])
            {
                Cam.camera.Parameters[PLCamera.Gain].SetValue(Convert.ToDouble(Gain.Text));
            }
            else
            {
                Cam.camera.Parameters[PLCamera.Gain].SetValue(camera_parameter["Gain Min"]);
            }
            if (Convert.ToDouble(ExposureTime.Text) >= camera_parameter["ExposureTime Min"] && Convert.ToDouble(ExposureTime.Text) <= camera_parameter["ExposureTime Max"])
            {
                Cam.camera.Parameters[PLCamera.ExposureTime].SetValue(Convert.ToDouble(ExposureTime.Text));
            }
            else
            {
                Cam.camera.Parameters[PLCamera.ExposureTime].SetValue(camera_parameter["ExposureTime Min"]);
            }
            if (Convert.ToDouble(Gamma.Text) >= camera_parameter["Gamma Min"] && Convert.ToDouble(Gamma.Text) <= camera_parameter["Gamma Max"])
            {
                Cam.camera.Parameters[PLCamera.Gamma].SetValue(Convert.ToDouble(Gamma.Text));
            }
            else
            {
                Cam.camera.Parameters[PLCamera.Gamma].SetValue(camera_parameter["Gamma Min"]);
            }
        }

        private void CameraInit()
        {
            Cam.Display = Display_Windows;
            bool state;
            Cam.Init(out state);
            if (state)
            {
                Cam.OpenCamera();
            }
        }

        #region Config
        private SerialNumber SerialNumberClass()
        {
            SerialNumber serialnumber_ = new SerialNumber
            {
                Gain_val = Gain.Text,
                ExposureTime_val = ExposureTime.Text,
                Gamma_val = Gamma.Text,
                Save_Image_Path_val = Save_Image_Path.Text,
                Row_val = Row.Text,
                Row_Gap_val = Row_Gap.Text,
                Column_val = Column.Text,
                Column_Gap_val = Column_Gap.Text,
                Start_X_val = Start_X.Text,
                Start_Y_val = Start_Y.Text,
            };
            return serialnumber_;
        }

        private void LoadConfig(int model, int serialnumber, bool encryption = false)
        {
            List<RootObject> Parameter_info = Config.Load(encryption);
            if (Parameter_info != null)
            {
                Gain.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Gain_val;
                ExposureTime.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.ExposureTime_val;
                Gamma.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Gamma_val;
                Save_Image_Path.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Save_Image_Path_val;
                Row.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Row_val;
                Row_Gap.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Row_Gap_val;
                Column.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Column_val;
                Column_Gap.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Column_Gap_val;
                Start_X.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Start_X_val;
                Start_Y.Text = Parameter_info[model].Models[serialnumber].SerialNumbers.Start_Y_val;
            }
            else
            {
                // 結構:2個Models、Models下在各2個SerialNumbers
                SerialNumber serialnumber_ = SerialNumberClass();
                List<Model> models = new List<Model>
                {
                    new Model { SerialNumbers = serialnumber_ },
                    new Model { SerialNumbers = serialnumber_ }
                };
                List<RootObject> rootObjects = new List<RootObject>
                {
                    new RootObject { Models = models },
                    new RootObject { Models = models }
                };
                Config.InitSave(rootObjects, encryption);
            }
        }
       
        private void SaveConfig(int model, int serialnumber, bool encryption = false)
        {
            Config.Save(model, serialnumber, SerialNumberClass(), encryption);
        }
        #endregion
        #endregion

        #region Parameter and Init
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MotionInit();
            CameraInit();
            LoadConfig(0, 0);
        }
        BaseConfig<RootObject> Config = new BaseConfig<RootObject>();
        #region Log
        BaseLogRecord Logger = new BaseLogRecord();
        //Logger.WriteLog("儲存參數!", LogLevel.General, richTextBoxGeneral);
        #endregion
        public ADLINK_Motion Controller;
        Basler Cam = new Basler();
        bool Cam_IsOpen = false;
        #endregion

        #region Main Screen
        private async void Main_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(Start):
                    {
                        // Go Home
                        Controller.Axes[0].MoveHome();
                        await Task.Delay(150);
                        await Controller.Axes[0].WaitMotionStatus(MotionStatus.MDN, true);
                        Controller.Axes[1].MoveHome();
                        await Task.Delay(150);
                        await Controller.Axes[1].WaitMotionStatus(MotionStatus.MDN, true);
                        // Go startpoint
                        int start_x_value = Convert.ToInt32(Start_X.Text);
                        int start_y_value = Convert.ToInt32(Start_Y.Text);
                        DeepWise.Shapes.Point startpoint = new DeepWise.Shapes.Point(start_x_value, start_y_value);
                        Controller.MoveLineAbsolute(new int[] { 1, 0 }, startpoint);
                        await Task.Delay(150);
                        await Controller.Axes[0].WaitMotionStatus(MotionStatus.MDN, true);
                        // Move and Continue 
                        Cam.ContinueAcquisition();
                        int row_num = Convert.ToInt32(Row.Text);
                        int col_num = Convert.ToInt32(Column.Text);
                        int x_step = Convert.ToInt32(Row_Gap.Text);
                        int y_step = Convert.ToInt32(Column_Gap.Text);
                        Cam.image_storage_path = Save_Image_Path.Text;
                        for (int col = 0; col < col_num; col++)
                        {
                            for (int row = 0; row < row_num; row++)
                            {
                                Controller.MoveLineAbsolute(new int[] { 1, 0 }, new DeepWise.Shapes.Point(startpoint.X - col * x_step, startpoint.Y - row * y_step));
                                await Task.Delay(150);
                                await Controller.Axes[0].WaitMotionStatus(MotionStatus.MDN, true);
                                Thread.Sleep(500);
                                Cam.OneShot();
                                Thread.Sleep(200);
                            }
                        }
                        Cam.StopAcquisition();
                        break;
                    }
                case nameof(Open_Image_Storage_Folder):
                    {
                        System.Windows.Forms.FolderBrowserDialog image_storage_path = new System.Windows.Forms.FolderBrowserDialog();
                        image_storage_path.Description = "Choose Image Storage Path";
                        image_storage_path.ShowDialog();
                        Save_Image_Path.Text = image_storage_path.SelectedPath;
                        break;
                    }
            }
        }
        #endregion

        #region Camera Screen
        private void Camera_Btn_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case nameof(Set_Camera_Parameter):
                    {
                        if (Cam_IsOpen)
                        {
                            Cam.camera.Parameters[PLCamera.Gain].SetValue(Convert.ToDouble(Gain.Text));
                            Cam.camera.Parameters[PLCamera.ExposureTime].SetValue(Convert.ToDouble(ExposureTime.Text));
                            Cam.camera.Parameters[PLCamera.Gamma].SetValue(Convert.ToDouble(Gamma.Text));
                        }
                        else
                        {
                            MessageBox.Show("請打開相機!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        break;
                    }
                case nameof(Continue_Acquisition):
                    {
                        try
                        {
                            Cam.OpenCamera();
                            CameraParameterInit();
                            Cam.ContinueAcquisition();
                            Cam_IsOpen = true;
                        }
                        catch
                        {
                            MessageBox.Show("Camera initializing failed!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        break;
                    }
                case nameof(Stop_Continue_Acquisition):
                    {
                        Cam.StopAcquisition();
                        Cam_IsOpen = false;
                        break;
                    }
                case nameof(Save_Image):
                    {
                        if (Cam_IsOpen)
                        {
                            if (!string.IsNullOrEmpty(Save_Image_Path.Text))
                            {
                                if (Directory.Exists(Save_Image_Path.Text))
                                {
                                    Cam.image_storage_path = Save_Image_Path.Text;
                                    Cam.OneShot();
                                }
                                else
                                {
                                    MessageBox.Show("Image storage path is invalid!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Image storage path is empty!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Camera doesn't turn on!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        break;
                    }
                case nameof(Save_Config):
                    {
                        SaveConfig(0,0);
                        break;
                    }
            }
        }
        #endregion
    }
}
