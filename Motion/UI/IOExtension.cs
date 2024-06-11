using MotionControllers.Motion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace MotionControllers.UI
{
    public class IOExtension : MarkupExtension
    {
        static Dictionary<string, IOPortInfo> Dic { get; } = ((Func<Dictionary<string, IOPortInfo>>)(() =>
        {
            try
            {
                var dic = JsonConvert.DeserializeObject<Dictionary<string, IOPortInfo>>(System.IO.File.ReadAllText(@"D:\ioConfig"));
                return dic;
            }
            catch (Exception ex)
            {
                return null;
            }

        })).Invoke();
        public IOExtension() { }
        public IOExtension(string key)
        {
            Key = key;
        }
        public string Key { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Dic != null)
            {
                var target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
                var targetProperty = (DependencyProperty)target.TargetProperty;
                return Dic.ContainsKey(Key) ? Dic[Key] : default(IOPortInfo);
            }
            else
                return default(IOPortInfo);
        }
    }

}
