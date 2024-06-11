using DeepWise.Controls.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace DeepWise.Controls
{
    //public class HideLabelAttribute : Attribute
    //{
    //}

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    //public class ROIAttribute : Attribute
    //{
    //    public ROIAttribute(Type type)
    //    {
    //        if (!(type.IsSubclassOf(typeof(InteractiveObject)) || type.IsSubclassOf(typeof(InteractiveObjectGDI)))) throw new Exception("type 的類型必須是KI.ROIComponent.ROI的衍伸物");
    //        ROIType = type;
    //    }
    //    public Type ROIType { get; }
    //}

    public class ColorAttribute : Attribute
    {
        public ColorAttribute(string hex)
        {
            Color = System.Drawing.ColorTranslator.FromHtml(hex);
        }
        public ColorAttribute(int color)
        {
            Color = System.Drawing.Color.FromArgb(color);
        }
        public ColorAttribute(System.Drawing.Color color)
        {
            Color = color;
        }


        public System.Drawing.Color Color { get; }
        public string Hex => "#" + Color.R.ToString("X2") + Color.G.ToString("X2") + Color.B.ToString("X2");
        public int Argb => Color.ToArgb();
    }

    /// <summary>
    /// Create a mark behind the property name
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class MarkAttribute : Attribute
    {
        public MarkAttribute(Mark mark)
        {
            this.Mark = mark;
        }
        public Mark Mark { get; }
    }
    public enum Mark
    {
        Question,
        Warring,
        Exclamation,
    }


    /// <summary>
    /// Specifies the name of unit for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UnitAttribute : Attribute
    {
        public UnitAttribute(string unit)
        {
            Unit = unit;
        }
        public UnitAttribute(Type resourcesType,string key)
        {
            Unit = new ResourceManager(resourcesType).GetString(key);
        }
        public string Unit { get; }
    }

    /// <summary>
    /// Provides slider for property in <see cref="PropertyGrid"/> control.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SliderAttribute : Attribute
    {
        public object MinValue { get; }
        public object MaxValue { get; }
        public string ScrollEventHandler { get; }
        public SliderAttribute()
        {

        }
        public SliderAttribute(double minima, double maxima,string scrollEventHandler = null)
        {
            MinValue = minima;
            MaxValue = maxima;
            ScrollEventHandler = scrollEventHandler;
        }
        public SliderAttribute(string minima, string maxima, string scrollEventHandler = null)
        {
            MinValue = minima;
            MaxValue = maxima;
            ScrollEventHandler = scrollEventHandler;
        }

    }

    /// <summary>
    /// Set decimal places for float property in <see cref="PropertyGrid"/> control.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DecimalPlacesAttribute : Attribute
    {
        public int Number { get; }
        public DecimalPlacesAttribute(int number)
        {
            Number = number;
        }
    }

    /// <summary>
    /// Specifies using a group of radio buttons instead of a combobox for a enum property in <see cref="PropertyGrid"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Enum, AllowMultiple = false)]
    public class RadioButtonsAttribute : Attribute
    {
        public RadioButtonsAttribute(RadioButtonsDirection direction = RadioButtonsDirection.TopToBottom)
        {
            Direction = direction;
        }
        public RadioButtonsDirection Direction { get; }
    }

    public class WhiteListAttribute : Attribute
    {
        public WhiteListAttribute(params object[] elements)
        {
            Elements = elements;
        }
        public object[] Elements { get; }
    }

    public class IconAttribute : Attribute
    {
        string name { get; }
        Type resourcesType { get; }
        public IconAttribute(Type resourcesType, string name)
        {
            this.name = name;
            this.resourcesType = resourcesType;
        }

        public IconAttribute(string path)
        {
            var file = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
            _image = Image.FromFile(file);
            //Combine current path
        }

        Image _image;
        public Image Image
        {
            get
            {
                if (_image == null)
                    _image = (Image)new ResourceManager(resourcesType).GetObject(this.name);
                return _image;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UpDownButtonAttribute : Attribute
    {
        public UpDownButtonAttribute(double increment = 1)
        {
            Increment = increment;
        }
        public double Increment { get; }
    }

    /// <summary>
    /// Specifies a string property is a path for file or folder.
    /// </summary>
    public class PathAttribute : Attribute
    {
        public PathMode Mode { get; }
        public PathAttribute(PathMode mode)
        {

            Mode = mode;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class WhiteListedAttribute : Attribute
    {

    }

    /// <summary>
    /// Force a class property expand.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExpanderAttribute : Attribute
    {
        public ExpanderAttribute()
        {

        }

        public ExpanderAttribute(bool expanded)
        {
            Expanded = expanded;
        }

        public bool Expanded { get; }
    }


    public class CustomEditorAttribute : Attribute
    {
        
        public CustomEditorAttribute(Type userControlType,bool isSpanned = false,string contenProperty = null)
        {
            EditorType = userControlType;
            IsSpanned = isSpanned;
            ContentProperty = contenProperty;
        }
        public Type EditorType { get; }
        public bool IsSpanned { get; }
        public string ContentProperty { get; }
    }


    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    //public class EditableAttribute : Attribute
    //{
    //    public EditableAttribute(bool editable)
    //    {
    //        Editable = editable;
    //    }
    //    public bool Editable { get; }
    //}
    /// <summary>
    /// Create a button for a method or property in <see cref="PropertyGrid"/> or others dw-Controls.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method| AttributeTargets.Property, AllowMultiple = false)]
    public class ButtonAttribute : Attribute
    {
        /// <summary>
        /// Create a button for a method or property in <see cref="PropertyGrid"/> or others dw-Controls.
        /// </summary>
        /// <param name="targetMethodPath">聯繫的目標方法。可為相對或絕對路徑，若為相對路徑則由實例本身為起點，若為絕對路徑則必須為靜態成員的方法或是靜態方法。</param>
        /// <param name="textOnButton">按鈕的文字。</param>
        public ButtonAttribute(string targetMethodPath = "", string textOnButton = "Click")
        {
            Path = targetMethodPath;
            Text = textOnButton;
        }
        public string CallbackName => Path;
        public string Path { get; }
        public bool Relative { get; }
        public string Text { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class ItemTypesAttribute : Attribute
    {
        public ItemTypesAttribute(params Type[] itemTypes)
        {
            ItemTypes = itemTypes;
        }
        public Type[] ItemTypes { get; }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)] 
    public class ToolButtonsAttribute : Attribute
    {
        public ToolButtonsAttribute(params object[] buttons)
        {
            Buttons = buttons.Cast<Enum>().ToArray();
        }
        public ToolButtonsAttribute(params ToolButton[] buttons)
        {
            Buttons = buttons.Cast<Enum>().ToArray();
        }

        public Enum[] Buttons { get; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false),Obsolete("方法已遺棄,使用[Button]取代")]
    public class SmallButtonAttribute : Attribute
    {
        public SmallButtonAttribute(string handler, string buttonText = "")
        {
            Handler = handler;
            Text = buttonText;
        }
        public string Handler { get; }
        public string Text { get; }
    }

    [AttributeUsage(AttributeTargets.Property| AttributeTargets.Method, AllowMultiple = true)]
    public class ConditionalAttribute : Attribute
    {
        public ConditionalAttribute(string targetPath, DisableEffect effect = DisableEffect.Disable)
        {
            TargetPropertyName = targetPath;
            Value = true;
            Effect = effect;
        }
        public ConditionalAttribute(string targetPath,object value, DisableEffect effect = DisableEffect.Disable)
        {
            TargetPropertyName = targetPath;
            Value = value;
        }
        public string TargetPropertyName { get; }
        public object Value { get; }
        public DisableEffect Effect { get; }
    }

    public enum DisableEffect
    {
        Disable,
        Hide,
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field|AttributeTargets.Method,Inherited = true, AllowMultiple = false), ImmutableObject(true)]
    public sealed class OrderAttribute : Attribute
    {
        private readonly int order;
        public int Order { get { return order; } }
        public OrderAttribute(int order) { this.order = order; }
    }


    #region Enums
    public enum ToolButton
    {
        [Icon(typeof(Properties.Resources), "baseline_file_copy_black_24dp")]
        Copy,
        [Icon(typeof(Properties.Resources), "baseline_delete_black_24dp")]
        Delete,
        [Icon(typeof(Properties.Resources), "baseline_note_add_black_24dp")]
        Add,
        [Icon(typeof(Properties.Resources), "baseline_drive_file_rename_outline_black_24dp")]
        Remove,
    }

    public enum RadioButtonsDirection
    {
        LeftToRight,
        TopToBottom,
    }

    public enum PathMode
    {
        SelectFile, 
        SaveFile, 
        Directory
    }
    #endregion

    public static class IconAttributeHelper
    {
        /// <summary>
        /// 獲得由<see cref="IconAttribute"/>所提供的圖示。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Image GetIcon(this Type type)
        {
            object[] attri = type.GetCustomAttributes(typeof(IconAttribute), true);
            if (attri.Length > 0)
                return (attri[0] as IconAttribute).Image;
            else
                return null;
        }
        /// <summary>
        /// 獲得由<see cref="IconAttribute"/>所提供的圖示。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Image GetIcon(this Enum enumValue)
        {
            string defaultName = enumValue.ToString();
            Type enumType = enumValue.GetType();
            var memberInfo = enumType.GetMember(defaultName);
            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttributes(typeof(IconAttribute), false);
                if (attributes.Length > 0)
                    return ((IconAttribute)attributes[0]).Image;
                else return null;
            }
            else return null;
        }
    }

}
