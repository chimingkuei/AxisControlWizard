using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DeepWise.Devices
{
    public class Dimmer : ILogMessageProvider
    {
        protected void WriteMessage(string caption ,string message)
        {
            MessageWritten?.Invoke(this, new LogMessageEventArgs(caption,this,message));
        }
        protected void WriteMessage(Exception ex)
        {
            MessageWritten?.Invoke(this, new LogMessageEventArgs(new LogMessage(ex)));
        }
        public event EventHandler<LogMessageEventArgs> MessageWritten;

        public const int readingSleepTime = 10;

        public string PortName
        {
            get => ComPort?.PortName;
            set => ComPort.PortName = value;
        }
        public DimmerModel Model { get; } = DimmerModel.Hope_1CH12V30W_RS232;
        [JsonIgnore]
        public bool IsResponsive
        {
            get
            {
                try
                {
                    string command = $"0103000{0}0001";
                    command = ":" + command + GetLRC(command) + "\r\n";
                    this.ComPort.Write(command);
                    var resultStr = Read();
                    return true;
                }
                catch (TimeoutException ex)
                {
                    return false;
                }
            }
        }

        public bool Wait { get; set; } = false;

        [JsonIgnore]
        public int ChannelCount => _channelCount;

        // ---------------------------------------------------------------
        public Dimmer()
        {
            ComPort = new SerialPort();
            ComPort.BaudRate = 19200;
            ComPort.DataBits = 8;
            this.RegistEvent();
        }
        public Dimmer(string portName)
        {
            ComPort = new SerialPort();
            ComPort.PortName = portName;
            ComPort.BaudRate = 19200;
            ComPort.DataBits = 8;
        }
        ~Dimmer()
        {
            if (this.ComPort.IsOpen)
            {
                this.ComPort.Close();
            }
        }

        public int GetBrightness(int channel)
        {
            if (!ComPort.IsOpen) throw new Exception("COM Port is not opened");
            if (channel < chStrPos || channel >= ChannelCount + chStrPos) throw new ArgumentOutOfRangeException(nameof(channel), $"頻道必須介於{chStrPos}~{chStrPos + ChannelCount - 1}");
            switch (Model)
            {
                case DimmerModel.Hope_1CH12V30W_RS232:
                case DimmerModel.GLC_PD24V30W_4CH:
                    {
                        this.ComPort.Write(GetCommand($"0103000{channel}0001"));
                        string resultStr = Read();
                        while(!(resultStr.StartsWith(":01030200") && GetLRC(resultStr.Substring(1, 10)) == resultStr.Substring(11, 2)))
                        {
                            try
                            {

                                resultStr = Read();
                            }
                            catch (TimeoutException ex)
                            {
                                return 0;
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
                        }
                        return Convert.ToInt32(resultStr.Substring(9, 2), 16);
                    }
                default:
                    throw new NotImplementedException();
            }
        }
        public void SetBrightness(int channel, int brightness)
        {
            if (!ComPort.IsOpen) throw new Exception("ComPort is not open");
            if (channel < chStrPos || channel >= ChannelCount + chStrPos) throw new ArgumentOutOfRangeException(nameof(channel), $"頻道必須介於{chStrPos}~{chStrPos + ChannelCount - 1}");
            switch (Model)
            {
                case DimmerModel.Hope_1CH12V30W_RS232:
                case DimmerModel.GLC_PD24V30W_4CH:
                    {
                        if (brightness > 255 || brightness < 0)
                            throw new ArgumentOutOfRangeException("亮度必須介於0~255之間");
                        string hexBrightness = Convert.ToString(brightness, 16).ToUpper().PadLeft(2, '0');
                        ComPort.Write(GetCommand($"0106000{channel}00{hexBrightness}"));
                        try
                        {
                            if(Wait)
                            Read();
                        }
                        catch(Exception ex)
                        {
                            WriteMessage(ex);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// ex. "COM3"
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        public bool Connect(string portName)
        {
            ComPort.PortName = portName;
            return Connect();
        }
        public bool Connect()
        {
            try
            {
                ComPort.Open();
            }
            catch (System.IO.IOException ex)
            {
                WriteMessage($"調光器連線失敗", ex.Message);
                return false;
            }
            catch(Exception ex)
            {
                WriteMessage($"調光器連線失敗", ex.Message);
                return false;
            }
            if (IsResponsive)
            {
                int i = 1;
                for (; i <= 8; i++)
                {
                    if (!IsChannelAvalible(i))
                        break;
                }
                _channelCount = i - 1;
                return true;
            }
            else
            {
                ComPort.Close();
                WriteMessage($"調光器連線失敗", $"Port : { ComPort.PortName}");
                return false;
            }
        }

        //public bool Connect()
        //{
        //    if (ComPort.IsOpen) throw new Exception();

        //    foreach (string portName in SerialPort.GetPortNames())   // get all open port
        //    {
        //        ComPort.PortName = portName;
        //        try
        //        {
        //            ComPort.Open();
        //            if (IsResponsive)
        //            {
        //                int i = 1;
        //                for (; i <= 8; i++)
        //                {
        //                    if (!IsChannelAvalible(i))
        //                        break;
        //                }
        //                _channelCount = i - 1;
        //                Debug.WriteLine($"Dimmer connected : {ComPort.PortName} ,Channel :{ChannelCount}");
        //                return true;
        //            }
        //            else
        //            {

        //                ComPort.Close();
        //            }
        //        }
        //        catch (UnauthorizedAccessException ex)
        //        {
        //            continue;
        //        }
        //        catch (Exception ex)
        //        {

        //        }

        //    }

        //    return false;
        //}

        public void Close()
        {
            ComPort.Close();
        }

        public void LightOff()
        {
            switch (Model)
            {
                case DimmerModel.Hope_1CH12V30W_RS232:
                    for (int i = chStrPos; i < chStrPos + ChannelCount; i++)
                        SetBrightness(i, 0);
                    break;
                case DimmerModel.GLC_PD24V30W_4CH:
                    {
                        var cmd = $"01100001000{ChannelCount}";
                        cmd += Convert.ToString(ChannelCount, 16).ToUpper().PadLeft(2, '0');
                        for (int i = 0; i < ChannelCount; i++)
                            cmd += "00";
                        ComPort.Write(GetCommand(cmd));
                        try
                        {
                            Read();
                        }
                        catch (Exception ex)
                        {
                            WriteMessage(ex);
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            for (int i = 1; i <= ChannelCount; i++) SetBrightness(i, 0);
        }
        public void LightAll(int value = 255)
        {
            switch (Model)
            {
                case DimmerModel.Hope_1CH12V30W_RS232:
                    for (int i = chStrPos; i < chStrPos + ChannelCount; i++)
                        SetBrightness(i, value);
                    break;
                case DimmerModel.GLC_PD24V30W_4CH:
                    {
                        var cmd = $"01100001000{ChannelCount}";
                        cmd += Convert.ToString(ChannelCount, 16).ToUpper().PadLeft(2, '0');
                        string brightness = Convert.ToString(value, 16).ToUpper().PadLeft(2, '0');
                        for (int i = 0; i < ChannelCount; i++)
                            cmd += brightness;
                        ComPort.Write(GetCommand(cmd));
                        try
                        {
                            Read();
                        }
                        catch(Exception ex)
                        {
                            WriteMessage(ex);
                        }

                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

        }

        public void AddBrightnessSetting(string name, params int[] brightness)
        {
            if (brightness.Length != ChannelCount) throw new ArgumentException();
            Configs[name] = brightness;
        }
        public void SetBrightness(string name)
        {
            var ary = Configs[name];
            switch (Model)
            {
                case DimmerModel.Hope_1CH12V30W_RS232:
                    for (int i = 0; i < ary.Length; i++)
                        SetBrightness(i + chStrPos, ary[i]);
                    break;
                case DimmerModel.GLC_PD24V30W_4CH:
                    {
                        var cmd = $"01100001000{ChannelCount}";
                        cmd += Convert.ToString(ChannelCount, 16).ToUpper().PadLeft(2, '0');
                        for (int i = 0; i < ChannelCount; i++)
                            cmd += Convert.ToString(ary[i], 16).ToUpper().PadLeft(2, '0');
                        ComPort.Write(GetCommand(cmd));
                        Read();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        [JsonProperty]
        private Dictionary<string, int[]> Configs { get; } = new Dictionary<string, int[]>();

        #region Native Methods & members
        bool IsChannelAvalible(int channel)
        {
            switch (Model)
            {
                case DimmerModel.Hope_1CH12V30W_RS232:
                case DimmerModel.GLC_PD24V30W_4CH:
                    string command = $"0103000{channel}0001";
                    command = ":" + command + GetLRC(command) + "\r\n";
                    this.ComPort.Write(command);
                    try
                    {
                        var resultStr = Read();
                        var result = resultStr != ":010302FF00FB\r\n" && resultStr != ":01830379\r\n";
                        return result;
                    }
                    catch (TimeoutException ex)
                    {
                        return false;
                    }
                default:
                    throw new NotImplementedException();
            }

        }

        string Read(int number, char startWith = ':')
        {
            int crt = 0;
            char[] buffer = new char[20];
            lock (this.Receive_Lock)
            {
                string resultStr = "";
                while (true)
                {
                    if (crt == 0)
                    {
                        this.ComPort.Read(buffer, 0, 1);
                        if (buffer[0] == ':')
                        {
                            resultStr += buffer[0];
                            crt++;
                            continue;
                        }
                    }
                    else if (crt < number)
                    {
                        var toRead = ComPort.BytesToRead;
                        bool exceed = (crt + toRead) > number;
                        var count = exceed ? number - crt : toRead;
                        this.ComPort.Read(buffer, 0, count);
                        crt += count;
                        resultStr += new string(buffer, 0, count);
                        if (exceed)
                            break;
                    }
                    else
                        break;

                    Thread.Sleep(readingSleepTime);
                }
                return resultStr;
            }
        }
        string Read(char startWith = ':', string end = "\r\n", int timeout = 250)
        {
            int crt = 0;
            char[] buffer = new char[1];
            lock (Receive_Lock)
            {
                string resultStr = "";
                while (true)
                {
                    if (ComPort.BytesToRead > 0)
                    {
                        if (crt == 0)
                        {
                            this.ComPort.Read(buffer, 0, 1);
                            if (buffer[0] == ':')
                            {
                                resultStr += buffer[0];
                                crt++;
                            }
                        }
                        else
                        {
                            this.ComPort.Read(buffer, 0, 1);
                            resultStr += buffer[0];
                            if (resultStr.EndsWith(end))
                            {
                                return resultStr;
                            }
                        }
                        continue;
                    }
                    else
                    {
                        if (timeout < 0) throw new TimeoutException();
                        Thread.Sleep(readingSleepTime);
                        timeout -= readingSleepTime;
                    }
                }
            }
        }
        string GetCommand(string cmd) => ":" + cmd + GetLRC(cmd) + "\r\n";

        readonly object Receive_Lock = new object();
        int _channelCount = 0, chStrPos = 1;
        SerialPort ComPort;
        #endregion

        [DebuggerStepThrough]
        public static string GetLRC(string data)
        {
            List<int> calList = new List<int>();
            Regex re = new Regex(@"[^\w]");
            data = re.Replace(data, "");
            for (int i = 0; i + 1 <= data.Length; i = i + 2)
            {
                string handleStr = data.Substring(i, 2);
                calList.Add(Convert.ToInt32(handleStr, 16));
            }
            int accumulated = 0;
            foreach (int value in calList)
            {
                accumulated = accumulated + value;
            }
            int LRC = 255 - accumulated + 1;
            if (LRC < 0)
            {
                LRC = 256 + LRC;
            }
            string HeX_LRC = Convert.ToString(LRC, 16).ToUpper().PadLeft(2, '0');
            return HeX_LRC;
        }

        public static void Test()
        {
            //Initialize
            Dimmer dimmer = new Dimmer();

            if (dimmer.Connect())
            {

                switch (dimmer.ChannelCount)
                {
                    case 4:
                        dimmer.AddBrightnessSetting("Top", 200, 0, 0, 0);
                        dimmer.AddBrightnessSetting("Side", 0, 200, 0, 0);
                        break;
                    case 2:
                        dimmer.AddBrightnessSetting("Top", 200, 0);
                        dimmer.AddBrightnessSetting("Side", 0, 200);
                        break;
                }

                var interval = 300;
                //Manual Set
                dimmer.LightAll();
                Thread.Sleep(interval);

                dimmer.LightOff();
                Thread.Sleep(interval);

                dimmer.SetBrightness(1, 200);
                Thread.Sleep(interval);

                dimmer.SetBrightness(2, 100);
                Thread.Sleep(interval);

                dimmer.LightOff();
                Thread.Sleep(interval);

                //Set from settings
                dimmer.SetBrightness("Top");
                Thread.Sleep(interval);

                dimmer.SetBrightness("Side");
                Thread.Sleep(interval);
                //Turn Off
                dimmer.LightOff();
                dimmer.Close();
            }
            else
                MessageBox.Show("開啟失敗");
        }
    }

    public enum DimmerModel
    {
        [Display(Name = "1CH-12V30W-RS232")]
        Hope_1CH12V30W_RS232,
        [Display(Name = "GLC-PD24V30W-4CH")]
        GLC_PD24V30W_4CH,
    }
}


