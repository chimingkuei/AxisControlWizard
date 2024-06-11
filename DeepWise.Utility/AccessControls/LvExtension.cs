using DeepWise.AccessControls;
using DeepWise.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace DeepWise
{
    public class LvExtension : MarkupExtension
    {
        public LvExtension() { }
        public LvExtension(string level)
        {
            Level = (AccessLevel)Enum.Parse(typeof(AccessLevel), level);
        }
        public LvExtension(AccessLevel level) { Level = level; }
        public AccessLevel Level { get; set; }
        public static List<(DependencyObject, DependencyProperty, AccessLevel)> Targets { get; } = new List<(DependencyObject, DependencyProperty, AccessLevel)>();
        public static Lazy<bool> IsInDesignMode = new Lazy<bool>(() => DesignerProperties.GetIsInDesignMode(new DependencyObject()));
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            var targetProperty = (DependencyProperty)target.TargetProperty;
            if (IsInDesignMode.Value)
            {
                if (targetProperty.PropertyType == typeof(bool))
                {
                    return false;
                }
                else if (targetProperty.PropertyType == typeof(Visibility))
                {
                    return Visibility.Visible;
                }
                else
                    throw new Exception();
            }
            else
            {
                Targets.Add(((DependencyObject)target.TargetObject, targetProperty, Level));
                var user = AccessController.Default.CurrentUser;
                if (user != null)
                {
                    var b = user.AccessLevel >= Level;
                    if (targetProperty.PropertyType == typeof(bool))
                    {
                        return b;

                    }
                    else if (targetProperty.PropertyType == typeof(Visibility))
                    {
                        return b ? Visibility.Visible : Visibility.Collapsed;
                    }
                    else
                        throw new Exception();
                }
                else
                {

                    if (targetProperty.PropertyType == typeof(bool))
                    {
                        return false;

                    }
                    else if (targetProperty.PropertyType == typeof(Visibility))
                    {
                        return Visibility.Collapsed;
                    }
                    else
                        throw new Exception();
                }
            }


        }
    }
}
