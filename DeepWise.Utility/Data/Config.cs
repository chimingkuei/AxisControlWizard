using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DeepWise.Data
{
    public interface IConfig
    {
        string Path { get; }
        bool IsEncrypted { get; }
    }

    public abstract class Config : IConfig, INotifyPropertyChanged
    {
        public Config(string path, bool relativePath = true, bool encrypt = false)
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {
                return;
            }

            if (relativePath)
                this.path = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
            else
                this.path = path;
            isEncrypted = encrypt;
            if (File.Exists(path))
            {
                try
                {
                    this.LoadConfig();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"檔案讀取失敗:{path}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region IConfig
        bool IConfig.IsEncrypted => isEncrypted;
        string IConfig.Path => path;
        bool isEncrypted;
        string path;
        #endregion IConfig

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public bool CheckIsChanged()
        {
            var path = (this as IConfig).Path;
            if (File.Exists(path))
                return JsonConvert.SerializeObject(this) != File.ReadAllText(path);
            else
                return true;
        }

        #region 
        public void SaveConfig() => (this as IConfig).SaveConfig();
        public void LoadConfig() => (this as IConfig).LoadConfig();
        public void ExportConfig(string path) => (this as IConfig).ExportConfig(path);
        public void ImportConfig(string path) => (this as IConfig).ImportConfig(path);
        #endregion
    }

    public static class ConfigExtensions
    {
        public static void SaveConfig(this IConfig config)
        {
            var json = JsonConvert.SerializeObject(config);
            var dir = Path.GetDirectoryName(config.Path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            File.WriteAllText(config.Path, config.IsEncrypted ? Encrypt(json) : json);
        }
        public static void LoadConfig(this IConfig config)
        {
            config.ImportConfig(config.Path);
        }
        public static void ExportConfig(this IConfig config, string path)
        {
            var json = JsonConvert.SerializeObject(config);
            File.WriteAllText(path, config.IsEncrypted ? Encrypt(json) : json);
        }
        public static void ImportConfig(this IConfig config, string path)
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                Populate(config.IsEncrypted ? Decrypt(json) : json, config);
            }
            else
                MessageBox.Show($"找不到檔案{config.Path}");
        }

        private static void Populate(string jString, object target)
        {
            JToken token = JToken.Parse(jString);
            switch (token.Type)
            {
                case JTokenType.Array:
                    if (target is IList list)
                        list.Clear();
                    else if (target is IDictionary dic)
                        dic.Clear();
                    JsonConvert.PopulateObject(jString, target);
                    break;
                case JTokenType.Object:
                    {
                        JObject jObj = token as JObject;
                        var type = target.GetType();
                        foreach (var jProperty in jObj.Properties().ToArray())
                        {
                            var pInfo = type.GetProperty(jProperty.Name);
                            switch (jProperty.Value.Type)
                            {
                                case JTokenType.Object:
                                    {
                                        if (typeof(IDictionary).IsAssignableFrom(pInfo.PropertyType))
                                            (pInfo.GetValue(target) as IDictionary).Clear();
                                        else
                                        {
                                            if(pInfo.PropertyType.IsValueType && pInfo.CanWrite)
                                            {
                                                pInfo.SetValue(target, JsonConvert.DeserializeObject(jObj[jProperty.Name].ToString(), pInfo.PropertyType));
                                            }
                                           else
                                            {
                                                object value = pInfo.GetValue(target);
                                                if (value != null)
                                                {
                                                    var name = jProperty.Name;
                                                    Populate(jObj[name].ToString(), value);
                                                    jObj.Remove(name);
                                                }
                                                else if (jProperty.Value["__ObjectType"] is JToken typeToken)
                                                {
                                                    var valueType = typeToken.ToObject<Type>();
                                                    var fvalue = JsonConvert.DeserializeObject(jProperty.Value.ToString(), valueType);
                                                    pInfo.SetValue(target, fvalue);
                                                    jObj.Remove(jProperty.Name);
                                                }
                                                else
                                                {
                                                    var fvalue = JsonConvert.DeserializeObject(jProperty.Value.ToString(), pInfo.PropertyType);
                                                    pInfo.SetValue(target, fvalue);
                                                    jObj.Remove(jProperty.Name);
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case JTokenType.Array:
                                    {
                                        if (typeof(IList).IsAssignableFrom(pInfo.PropertyType))
                                            (pInfo.GetValue(target) as IList).Clear();
                                        break;
                                    }
                                default:
                                    break;
                            }
                        }
                        JsonConvert.PopulateObject(jObj.ToString(), target);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        //private static void Populate(string jString, object target)
        //{
        //    JToken token = JToken.Parse(jString);
        //    switch (token.Type)
        //    {
        //        case JTokenType.Array:
        //            if (target is IList list)
        //                list.Clear();
        //            else if (target is IDictionary dic)
        //                dic.Clear();
        //            JsonConvert.PopulateObject(jString, target);
        //            break;
        //        case JTokenType.Object:
        //            {

        //                JObject jObj = JObject.Parse(jString);
        //                var type = target.GetType();
        //                foreach (var jProperty in jObj.Properties())
        //                {
        //                    switch (jProperty.Value.Type)
        //                    {
        //                        case JTokenType.Object:
        //                            {
        //                                var pInfo = type.GetProperty(jProperty.Name);
        //                                if (typeof(IDictionary).IsAssignableFrom(pInfo.PropertyType))
        //                                    (pInfo.GetValue(target) as IDictionary).Clear();
        //                                else
        //                                {
        //                                    object value = pInfo.GetValue(target);
        //                                    if (value != null)
        //                                    {
        //                                        var name = jProperty.Name;
        //                                        Populate(jObj[name].ToString(), value);
        //                                        jObj.Remove(name);
        //                                    }
        //                                    else if (jProperty.Value["__ObjectType"] is JToken typeToken)
        //                                    {
        //                                        var valueType = typeToken.ToObject<Type>();
        //                                        var fvalue = JsonConvert.DeserializeObject(jProperty.Value.ToString(), valueType);
        //                                        pInfo.SetValue(target, fvalue);
        //                                        jObj.Remove(jProperty.Name);
        //                                    }
        //                                    else
        //                                    {
        //                                        var fvalue = JsonConvert.DeserializeObject(jProperty.Value.ToString(), pInfo.PropertyType);
        //                                        pInfo.SetValue(target, fvalue);
        //                                        jObj.Remove(jProperty.Name);
        //                                    }
        //                                }
        //                                break;
        //                            }
        //                        case JTokenType.Array:
        //                            {
        //                                var pInfo = type.GetProperty(jProperty.Name);
        //                                if (typeof(IList).IsAssignableFrom(pInfo.PropertyType))
        //                                    (pInfo.GetValue(target) as IList).Clear();
        //                                break;
        //                            }
        //                        default:
        //                            break;
        //                    }
        //                }
        //                JsonConvert.PopulateObject(jObj.ToString(), target);
        //            }
        //            break;
        //        default:
        //            throw new NotImplementedException();
        //    }
        //}


        [Obsolete]
        public static void ClearCollectionProperties(this IConfig config)
        {
            foreach (var pInfo in config.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    var value = pInfo.GetValue(config);
                    if (value is IList list)
                        list.Clear();
                    else if (value is IDictionary dic)
                        dic.Clear();
                }
                catch (Exception ex)
                {

                }
            }
        }

        static byte[] key = new byte[8] { 8, 2, 7, 8, 5, 6, 4, 9 };
        static byte[] iv = new byte[8] { 8, 2, 7, 8, 5, 6, 4, 9 };
        public static string Encrypt(string text)
        {
            using (SymmetricAlgorithm algorithm = DES.Create())
            {
                ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
                byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
                byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
                return Convert.ToBase64String(outputBuffer);
            }
        }
        public static string Decrypt(string text)
        {
            using (SymmetricAlgorithm algorithm = DES.Create())
            {
                ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
                byte[] inputbuffer = Convert.FromBase64String(text);
                byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
                return Encoding.Unicode.GetString(outputBuffer);
            }
        }
        public static void EncryptFile(string file)=> File.WriteAllText(file, Encrypt(File.ReadAllText(file)));
        public static void DecryptFile(string file)=> File.WriteAllText(file, Decrypt(File.ReadAllText(file)));

        #region Obsolete
        [Obsolete]
        public static void LoadConfig(this IConfig config, object sender, EventArgs e) => config.LoadConfig();
        [Obsolete]
        public static void SaveConfig(this IConfig config, object sender, EventArgs e) => config.SaveConfig();
        #endregion
    }
}
