using DeepWise.AccessControls;
using DeepWise.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace DeepWise
{
    public class AccessController :  Config , ILogMessageProvider , INotifyPropertyChanged
    {
        public AccessController(string path, bool relativePath = true) : base(path, relativePath, true)
        {
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {
                return;
                //in Design mode
            }


            this.RegistEvent();

            if (relativePath) path = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
            if (File.Exists(path))
            {
                var all = File.ReadAllText(path);
                var str = ConfigExtensions.Decrypt(all);
                var obj = JObject.Parse(str);
                if (obj.Property("CurrentUser") is JProperty property)
                {
                    var name = property.ToObject<string>();
                    if (name != null)
                        CurrentUser = Users.First(x => x.Name == name);
                }
            }

        }
        User _currentUser;
        [JsonIgnore]
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser == value) return;
                _currentUser = value;
                NotifyPropertyChanged();
                var user = value;
                SaveConfig();
                foreach (var item in LvExtension.Targets)
                {
                    var b = user != null ? user.AccessLevel >= item.Item3 : false;
                    if (item.Item2.PropertyType == typeof(bool))
                        item.Item1.SetValue(item.Item2, b);
                    else if (item.Item2.PropertyType == typeof(Visibility))
                        item.Item1.SetValue(item.Item2, b ? Visibility.Visible : Visibility.Collapsed);
                    else
                        throw new NotImplementedException();
                }
            }
        }

        [JsonProperty(nameof(CurrentUser))]
        string userName => CurrentUser != null ? CurrentUser.Name : null;
        public bool ShouldSerializeCurrentUser() => CurrentUser != null && !AutoLogOut;

        bool _autoLogOut = false;
        public bool AutoLogOut
        {
            get => _autoLogOut;
            set
            {
                if (_autoLogOut == value) return;
                    _autoLogOut = value;
                NotifyPropertyChanged();
                SaveConfig();
            }
        }

        public List<User> Users { get; } = new List<User>();

        public event EventHandler<LogMessageEventArgs> MessageWritten;

        public bool LogIn()
        {
            return new LoginWindow().ShowDialog() == true;
        }
        public void LogIn(string name, string passwords)
        {
            var user = Users.FirstOrDefault(x => x.Name == name);
            if(user == null)
            {
                throw new Exception("找不到該帳號");
                //MessageBox.Show("找不到該帳號", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                //WriteMessage("登入失敗", $"找不到使用者：{name}。");
                //return;
            }

            if(user.Passwords == passwords)
            {
                CurrentUser = user;
                //WriteMessage("登入成功", $"使用者名稱 : {name}");
            }
            else
            {
                //MessageBox.Show("輸入的密碼錯誤", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception("密碼錯誤");
                //WriteMessage("登入失敗", $"密碼輸入錯誤(使用者:{name})。");
            }
        }
        public void LogOut()
        {
            CurrentUser = null;
        }

        private void WriteMessage(string caption,string description = "")
        {
            MessageWritten?.Invoke(this, new LogMessageEventArgs(caption, this, description, MessageLevel.Info));
        }
        
        public static AccessController Default { get; } = new AccessController("acsctrl");


    

    }
}
