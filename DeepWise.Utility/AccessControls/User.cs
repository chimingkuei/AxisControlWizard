using DeepWise.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace DeepWise.AccessControls
{
    public class User 
    {
        [DisplayName("名稱")]
        public string Name { get; set; }
        [DisplayName("密碼")]
        public string Passwords { get; set; }
        [DisplayName("等級")]
        public AccessLevel AccessLevel { get; set; }
    }

    public enum AccessLevel
    {
        [Display(Name = "一般")]
        Operator = 0,
        [Display(Name = "工程")]
        Engineer = 5,
        [Display(Name = "系統管理員")]
        Administrator = 10,
    }

    public class AccessLevelAttribute : Attribute
    {
        public AccessLevelAttribute(AccessLevel level,DeepWise.Controls.DisableEffect effect = Controls.DisableEffect.Disable)
        {
            Level = level;
            Effect = effect;
        }
        public AccessLevel Level { get; }
        public Controls.DisableEffect Effect { get; }
    }

    public class AccessLevelToBooleanValueConverter : IValueConverter
    {
        public static AccessLevelToBooleanValueConverter Instance { get; } = new AccessLevelToBooleanValueConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result;
            if (value is User user)
            {
                result =  user.AccessLevel >= (AccessLevel)parameter;
            }
            else if (value is AccessLevel level)
                result = level >= (AccessLevel)parameter;
            else
                result = false;
            if (targetType == typeof(bool))
                return result;
            else if (targetType == typeof(Visibility))
                return result ? Visibility.Visible : Visibility.Collapsed;
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


}
