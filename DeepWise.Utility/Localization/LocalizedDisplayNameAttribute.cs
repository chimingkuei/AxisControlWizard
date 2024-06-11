using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace DeepWise.Localization
{
    /// <summary>
    /// 支援地區化的名稱顯示屬性。
    /// </summary>
    public class LocalizedDisplayNameAttribute : Attribute
    {
        public LocalizedDisplayNameAttribute(string key, Type resourcesType)
        {
            Name = key;
            if (resourcesType != null && key != string.Empty)
            {
                string name = new ResourceManager(resourcesType).GetString(key);
                if (name != null) Name = name;
            }
        }
        public LocalizedDisplayNameAttribute(string key) : this(key, GetDefaultResourceType()) { }
        static Type GetDefaultResourceType()
        {
            var assembly = Assembly.GetEntryAssembly();
            return assembly.GetType(assembly.GetName().Name + ".Properties.Resources");
        }
        public string Name { get; }
    }

    [Obsolete("Use DisplayNameConverter in namespace DeepWise.Controls instead")]
    public class DisplayNameConverter : IValueConverter
    {
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (value is Enum enumValue)
                return LocalizedDisplayNameAttributeHelper.GetDisplayName(enumValue);
            else if (value is Type type)
                return LocalizedDisplayNameAttributeHelper.GetDisplayName(type);
            else if (parameter is string propertyName)
                return LocalizedDisplayNameAttributeHelper.GetDisplayName(value.GetType().GetProperty(propertyName));
            else
                throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class LocalizedDisplayNameAttributeHelper
    {
        public static string GetDisplayName(this Type type)
        {
            object[] attri = type.GetCustomAttributes(typeof(LocalizedDisplayNameAttribute), true);
            if (attri.Length > 0)
                return (attri[0] as LocalizedDisplayNameAttribute).Name;
            else
                return type.Name;
        }

        public static string GetDisplayName(this MemberInfo info)
        {
            var att = info.GetCustomAttribute<LocalizedDisplayNameAttribute>();
            if(att != null) return att.Name;
            var att2 = info.GetCustomAttribute<DisplayNameAttribute>();
            if (att2 != null) return att2.DisplayName;
            var att3 = info.GetCustomAttribute<DisplayAttribute>();
            if (att3 != null && !string.IsNullOrEmpty(att3.Name)) return att3.Name;
            return info.Name;
        }
        public static string GetPropertyDisplayName(this Type type, string propertyName)
        {
            if (type.GetProperty(propertyName) is PropertyInfo propertyInfo)
            {
                if (Attribute.GetCustomAttribute(propertyInfo, typeof(LocalizedDisplayNameAttribute)) is LocalizedDisplayNameAttribute displayNameAttribute)
                    return displayNameAttribute.Name;
                else
                    return propertyInfo.Name;
            }
            else
                throw new Exception($"{type.FullName} dosen't have property named {propertyName}!");
        }
        public static string GetDisplayName(this Enum enumValue)
        {
            try
            {
                string defaultName = enumValue.ToString();
                Type enumType = enumValue.GetType();
                var memberInfo = enumType.GetMember(defaultName)[0];
                if (memberInfo.TryGetCustomAttribute<LocalizedDisplayNameAttribute>(out var localized))
                    return localized.Name;
                if (memberInfo.TryGetCustomAttribute<DisplayAttribute>(out var display))
                    return display.Name;
                if (memberInfo.TryGetCustomAttribute<DisplayNameAttribute>(out var displayName))
                    return displayName.DisplayName;
                return defaultName;
            }
            catch (Exception ex)
            {

                return enumValue.ToString();
            }
        }


    }
}
