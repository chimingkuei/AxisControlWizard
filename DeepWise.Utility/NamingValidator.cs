using DeepWise.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DeepWise
{
    public static class NamingExtension
    {


        public static string GetNewName(this IEnumerable<string> names,string defaultName = null)
        {
            return new NamingValidator(names).GetNewDefaultName(defaultName);
        }
        public static string GetNewName(this IDictionary dic, string defaultName = null)
        {
            return new NamingValidator(dic).GetNewDefaultName(defaultName);
        }
    }

    public class NamingValidator : ValidationRule
    {
        public NamingValidator(IDictionary dic)
        {
            var type = dic.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            if (type != null)
            {
                if (type.GetGenericArguments()[0] == typeof(string))
                {
                    var keys = type.GetProperty("Keys").GetValue(dic) as ICollection<string>;
                    Collection = keys;

                    DefaultName = type.GetGenericArguments()[1].GetDisplayName();
                }
                else
                    throw new ArgumentException("the key type of IDictionary must be type of string.");
            }
        }
        public NamingValidator(IEnumerable<string> names)
        {
            Collection = names;
        }
        public string DefaultName { get; set; } = "default";
        public List<Func<string, ValidationResult>> Validations { get; } = new List<Func<string, ValidationResult>>();
        public List<string> ReservedWords { get; } = new List<string>();
        public IEnumerable<string> Collection { get; }

        public string GetNewDefaultName(string defaultName = null)
        {
            int count = 1;
            if(defaultName==null)
            {
                defaultName = DefaultName;

                
            }
            while (Collection.Contains(defaultName + count)) count++;
            return defaultName + count;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var name = value as string;
            if (string.IsNullOrWhiteSpace(name))
                return new ValidationResult(false,"名稱不可為空");
            else if (name.Trim() != name) return new ValidationResult(false,"名稱中不可包含空白字元");
            else if (ReservedWords != null && ReservedWords.Contains(name)) return new ValidationResult(false,"名稱[" + name + "]無法使用");
            else if (Collection.Contains(name)) return new ValidationResult(false,"集合中已經存在名稱為[" + name + "]的項目");
            else if (name.IndexOfAny(System.IO.Path.GetInvalidPathChars()) != -1) return new ValidationResult(false,"名稱不可包含以下任意字元:\n" + new string(System.IO.Path.GetInvalidPathChars()));
            else
            {
                try
                {
                    foreach (var v in Validations)
                    {
                        var result = v(name);
                        if (!result.IsValid)
                            return result;
                    }
                }
                catch (Exception ex)
                {
                    return new ValidationResult(false,ex.Message);
                }
            }
            return ValidationResult.ValidResult;
        }
    }
}