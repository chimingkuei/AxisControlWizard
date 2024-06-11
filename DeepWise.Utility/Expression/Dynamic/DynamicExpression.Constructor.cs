using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DeepWise.Shapes;
namespace DeepWise.Expression
{
    public enum Constructor
    {
        [ParametersInfo(0, 0)]
        None = 0,

        #region Boolean
        [Display(Name = "布林常值"),ParametersInfo(0, 1)]
        BooleanValue,
        [Display(Name = "布林相等"), ConstructorInfo("{0}={1}", typeof(DVFuncs), nameof(DVFuncs.BooleanEquality)), ParametersInfo(2,0)]
        BooleanEqual,
        [Display(Name = "布林不相等"), ConstructorInfo("{0}≠{1}", typeof(DVFuncs), nameof(DVFuncs.BooleanInequality)), ParametersInfo(2,0)]
        BooleanNotEqual,
        [Display(Name = "A且B"), ConstructorInfo("{0} 且 {1}", typeof(DVFuncs), nameof(DVFuncs.BooleanAnd)), ParametersInfo(2,0)]
        BooleanAnd,
        [Display(Name = "A或B"), ConstructorInfo("{0} 或 {1}", typeof(DVFuncs), nameof(DVFuncs.BooleanOr)), ParametersInfo(2,0)]
        BooleanOr,
        [Display(Name = "字串相等"), ConstructorInfo("{0}={1}", typeof(String), "op_Equality"), ParametersInfo(2,0)]
        StringEqual,
        [Display(Name = "字串不相等"), ConstructorInfo("{0}≠{1}", typeof(String), "op_Inequality"), ParametersInfo(2, 0)]
        StringNotEqual,
        [Display(Name = "數字相等"), ConstructorInfo("{0}={1}", typeof(Double), "op_Equality"), ParametersInfo(2, 0)]
        RealEqual,
        [Display(Name = "數字不相等"), ConstructorInfo("{0}≠{1}", typeof(Double), "op_Inequality"), ParametersInfo(2, 0)]
        RealNotEqual,
        [Display(Name = "數字大於"), ConstructorInfo("{0}>{1}", typeof(Double), "op_GreaterThan"), ParametersInfo(2, 0)]
        RealGreater,
        [Display(Name = "數字大於或等於"), ConstructorInfo("{0}≥{1}", typeof(Double), "op_GreaterThanOrEqual"), ParametersInfo(2, 0)]
        RealGreaterOrEqual,
        [Display(Name = "數字小於"), ConstructorInfo("{0}<{1}", typeof(Double), "op_LessThan"), ParametersInfo(2, 0)]
        RealLess,
        [Display(Name = "數字小於或等於"), ConstructorInfo("{0}≤{1}", typeof(Double), "op_LessThanOrEqual"), ParametersInfo(2, 0)]
        RealLessOrEqual,
        [Display(Name = "數字介於"), ConstructorInfo("{0}介於{1},{2}之間", typeof(DVFuncs), nameof(DVFuncs.RealInRange)), ParametersInfo(3, 0)]
        RealInRange,
        [Display(Name = "數字不介於"), ConstructorInfo("{0}不介於{1},{2}之間", typeof(DVFuncs), nameof(DVFuncs.RealNotInRange)), ParametersInfo(3, 0)]
        RealNotInRange,
        [Display(Name = "Point相等"), ConstructorInfo("{0}={1}", typeof(Point), "op_Equality"), ParametersInfo(2, 0)]
        PointEqual,
        [Display(Name = "Point不相等"), ConstructorInfo("{0}≠{1}", typeof(Point), "op_Inequality"), ParametersInfo(2, 0)]
        PointNotEqual,
        [Display(Name = "Point3D相等"), ConstructorInfo("{0}={1}", typeof(Point3D), "op_Equality"), ParametersInfo(2, 0)]
        Point3DEqual,
        [Display(Name = "Point3D不相等"), ConstructorInfo("{0}≠{1}", typeof(Point3D), "op_Inequality"), ParametersInfo(2, 0)]
        Point3DNotEqual,
        [Display(Name = "Point6相等"), ConstructorInfo("{0}={1}", typeof(Point6), "op_Equality"), ParametersInfo(2, 0)]
        Point6Equal,
        [Display(Name = "Point6不相等"), ConstructorInfo("{0}≠{1}", typeof(Point6), "op_Inequality"), ParametersInfo(2, 0)]
        Point6NotEqual,

        [Display(Name = "列表的成員"), ConstructorInfo("{0}中索引值為{1}的成員", typeof(DVFuncs), nameof(DVFuncs.BooleanElementOfList)), ParametersInfo(2, 0)]
        BooleanListElement,

        [Display(Name ="列表包含指定的項目"), ParametersInfo(2, 0)]
        ListContains,

        #endregion Boolean

        #region String
        [Display(Name = "字串"), ParametersInfo(0,1)]
        String,
        [Display(Name = "合併字串"), ConstructorInfo("{0}+{1}", typeof(String), "Concat", typeof(string), typeof(string)), ParametersInfo(2, 0)]
        JoinString,
        [Display(Name = "Boolean的值"), ConstructorInfo("{0}", typeof(DVFuncs), nameof(DVFuncs.GetString), typeof(Boolean)), ParametersInfo(1, 0)]
        BooleanToString,
        [Display(Name = "實數的值"), ConstructorInfo("{0}", typeof(DVFuncs), nameof(DVFuncs.GetString), typeof(Double)), ParametersInfo(1, 0)]
        RealToString,
        [Display(Name = "Point的值"), ConstructorInfo("{0}", typeof(DVFuncs), nameof(DVFuncs.GetString), typeof(Point)), ParametersInfo(1, 0)]
        PointToString,
        [Display(Name = "Point3D的值"), ConstructorInfo("{0}", typeof(DVFuncs), nameof(DVFuncs.GetString), typeof(Point3D)), ParametersInfo(1, 0)]
        Point3DToString,
        [Display(Name = "Point6的值"), ConstructorInfo("{0}", typeof(DVFuncs), nameof(DVFuncs.GetString), typeof(Point6)), ParametersInfo(1, 0)]
        Point6ToString,
        [Display(Name = "列表的成員"), ConstructorInfo("{0}中索引值為{1}的成員", typeof(DVFuncs), nameof(DVFuncs.StringElementOfList)), ParametersInfo(2, 0)]
        StringListElement,
        #endregion String

        #region Real
        [Display(Name = "常數"),  ParametersInfo(0, 1)]
        RealNumber,
        //Arithmetic
        [Display(Name = "數字相加"), ConstructorInfo("{0}+{1}", typeof(DVFuncs), nameof(DVFuncs.DoubleAddition), ShowParentheses = true), ParametersInfo(2, 0)]
        RealPus,
        [Display(Name = "數字相減"), ConstructorInfo("{0}-{1}", typeof(DVFuncs), nameof(DVFuncs.DoubleSubtraction), ShowParentheses = true), ParametersInfo(2, 0)]
        RealMinus,
        [Display(Name = "數字相乘"), ConstructorInfo("{0}*{1}", typeof(DVFuncs), nameof(DVFuncs.DoubleMultiply), ShowParentheses = true), ParametersInfo(2, 0)]
        RealMuliply,
        [Display(Name = "數字相除"), ConstructorInfo("{0}/{1}", typeof(DVFuncs), nameof(DVFuncs.DoubleDivision), ShowParentheses = true), ParametersInfo(2, 0)]
        RealDevide,
        [Display(Name = "同餘"), ConstructorInfo("{0}%{1}", typeof(DVFuncs), nameof(DVFuncs.DoubleModulus), ShowParentheses = true), ParametersInfo(2, 0)]
        RealModulo,

        [Display(Name = "隨機數字"), ConstructorInfo("介於{0}至{1}之間的隨機整數", typeof(DVFuncs), nameof(DVFuncs.DoubleRandom), ShowParentheses = true), ParametersInfo(2, 0)]
        Random,

        [Display(Name = "正弦函數(Sinθ)"), ConstructorInfo("Sin({0})", typeof(Math), "Sin"), ParametersInfo(1, 0)]
        Sin,
        [Display(Name = "餘弦函數(Cosθ)"), ConstructorInfo("Cos({0})", typeof(Math), "Cos"), ParametersInfo(1, 0)]
        Cos,
        [Display(Name = "正切函數(Tanθ)"), ConstructorInfo("Tan({0})", typeof(Math), "Tan"), ParametersInfo(1, 0)]
        Tan,
        [Display(Name = "餘切函數(Atanθ)"), ConstructorInfo("Atan({0})", typeof(Math), "Atan"), ParametersInfo(1, 0)]
        Atan,
        [Display(Name = "餘切函數(Atan(Y,X))"), ConstructorInfo("Atan(Y={0},X={1})", typeof(Math), "Atan2"), ParametersInfo(2, 0)]
        Atan2,
        [Display(Name = "指數"), ConstructorInfo("{0}^{1}", typeof(Math), "Pow"), ParametersInfo(2, 0)]
        Pow,
        [Display(Name = "平方根"), ConstructorInfo("Sqrt({0})", typeof(Math), "Sqrt"), ParametersInfo(1, 0)]
        Sqrt,
        [Display(Name = "對數(log(X))"), ConstructorInfo("log({0})", typeof(Math), "Log10"), ParametersInfo(1, 0)]
        Log,
        [Display(Name = "對數(log(X,Y))"), ConstructorInfo("log({0},{1})", typeof(Math), "Log", typeof(double), typeof(double)), ParametersInfo(2, 0)]
        LogAB,
        [Display(Name = "自然對數(lnX)"), ConstructorInfo("ln({0})", typeof(Math), "Log",typeof(double)), ParametersInfo(1, 0)]
        Ln,

        //Point
        [Display(Name = "Point的X"), ConstructorInfo("{0}的X座標", typeof(Point), "X"), ParametersInfo(1, 0)]
        Point_X,
        [Display(Name = "Point的Y"), ConstructorInfo("{0}的Y座標", typeof(Point), "Y"), ParametersInfo(1, 0)]
        Point_Y,
        [Display(Name = "Point的方向"), ConstructorInfo("{0}朝向{1}的角度", typeof(Point), nameof(Point.Direction)), ParametersInfo(2, 0)]
        AngleBetweenPoint,
        [Display(Name = "Point的距離"), ConstructorInfo("{0}與{1}之間的距離", typeof(Point), nameof(Point.Distance)), ParametersInfo(2, 0)]
        DistanceBetweenPoint,
        [Display(Name = "Point的內積"), ConstructorInfo("{0}⋅{1}", typeof(Point), nameof(Point.Dot)), ParametersInfo(2, 0)]
        PointDot,

        //Point3D
        [Display(Name = "Point3D的X"), ConstructorInfo("{0}的X座標", typeof(Point3D), "X"), ParametersInfo(1, 0)]
        Point3D_X,
        [Display(Name = "Point3D的Y"), ConstructorInfo("{0}的Y座標", typeof(Point3D), "Y"), ParametersInfo(1, 0)]
        Point3D_Y,
        [Display(Name = "Point3D的Z"), ConstructorInfo("{0}的Z座標", typeof(Point3D), "Z"), ParametersInfo(1, 0)]
        Point3D_Z,
        [Display(Name = "Point3D的距離"), ConstructorInfo("{0}與{1}之間的距離", typeof(Point3D), nameof(Point3D.Distance)), ParametersInfo(2, 0)]
        DistanceBetweenPoint3D,
        [Display(Name = "Point3D的內積"), ConstructorInfo("{0}⋅{1}", typeof(Point3D), nameof(Point3D.Dot)), ParametersInfo(2, 0)]
        Point3DDot,

        //Point6
        [ConstructorInfo("{0}的X座標", typeof(Point6), "X"), ParametersInfo(1, 0)]
        [Display(Name = "Point6的X")]
        Point6_X,
        [ConstructorInfo("{0}的Y座標", typeof(Point6), "Y"), ParametersInfo(1, 0)]
        [Display(Name = "Point6的Y")]
        Point6_Y,
        [ConstructorInfo("{0}的Z座標", typeof(Point6), "Z"), ParametersInfo(1, 0)]
        [Display(Name = "Point6的Z")]
        Point6_Z,
        [ConstructorInfo("{0}的A", typeof(Point6), "A"), ParametersInfo(1, 0)]
        [Display(Name = "Point6的A")]
        Point6_A,
        [ConstructorInfo("{0}的B", typeof(Point6), "B"), ParametersInfo(1, 0)]
        [Display(Name = "Point6的B")]
        Point6_B,
        [ConstructorInfo("{0}的C", typeof(Point6), "C"), ParametersInfo(1, 0)]
        [Display(Name = "Point6的C")]
        Point6_C,

        [Display(Name = "集合的項目數量"), ParametersInfo(0, 1)]
        ListCount,

        [Display(Name = "集合的成員"), ConstructorInfo("{0}中索引值為{1}的成員", typeof(DVFuncs), nameof(DVFuncs.DoubleElementOfList)), ParametersInfo(2, 0)]
        RealListElement,
        #endregion Real

        #region Point
        [Display(Name = "點(X,Y)"), ConstructorInfo("X = {0},\nY = {1}", typeof(Point), typeof(double), typeof(double), ShowParentheses = true), ParametersInfo(2, 0)]
        PointConstructor,

        //Arithmetic
        [Display(Name = "Point相加"), ConstructorInfo("{0}位移{1}所獲得的新點", typeof(Point), nameof(Point.Offset), typeof(Point), typeof(Point)), ParametersInfo(2, 0)]
        PointOffset,

        [Display(Name = "Point相減"), ConstructorInfo("{0}減去{1}所獲得的新點", typeof(Point), nameof(Point.OffsetNeg)), ParametersInfo(2, 0)]
        PointMinus,

        [Display(Name = "Point的倍數"), ConstructorInfo("點{0}乘以實數{1}後所獲得的新點", typeof(Point), nameof(Point.Scale)), ParametersInfo(2, 0)]
        PointScale,

        [Display(Name = "Point位移方向"), ConstructorInfo("{0}向{1}方向移動{2}距離所獲得的新點", typeof(Point), nameof(Point.Offset), typeof(Point), typeof(double), typeof(double)), ParametersInfo(3, 0)]
        PointMoveDistanceTowardAngle,

        [Display(Name = "列表的成員"), ConstructorInfo("{0}中索引值為{1}的成員", typeof(DVFuncs), nameof(DVFuncs.PointElementOfList)), ParametersInfo(2, 0)]
        PointListElement,
        #endregion Point

        #region Point3D
        [Display(Name = "點(X,Y,Z)")]
        [ConstructorInfo("X = {0},\nY = {1},\nZ = {2}", typeof(Point3D), typeof(double), typeof(double), typeof(double), ShowParentheses = true), ParametersInfo(3, 0)]
        Point3DConstruct,

        //Arithmetic
        [Display(Name = "Point3D位移Point3D")]
        [ConstructorInfo("{0}位移{1}所獲得的新點", typeof(Point3D), nameof(Point3D.Offset), typeof(Point3D), typeof(Point3D)), ParametersInfo(2, 0)]
        Point3DOffset,

        [Display(Name = "Point3D的倍數")]
        [ConstructorInfo("點{0}乘以實數{1}後所獲得的新點", typeof(Point3D), nameof(Point3D.Scale)), ParametersInfo(2, 0)]
        Point3DScale,

        [Display(Name = "Point3D的外積")]
        [ConstructorInfo("{0}×{1}", typeof(Point3D), nameof(Point3D.Cross)), ParametersInfo(2, 0)]
        Point3DCross,

        [Display(Name = "座標變換")]
        [ConstructorInfo("使用轉換器{0}將點{1}轉換為世界座標", typeof(DVFuncs), nameof(DVFuncs.CoordinateTransformation)), ParametersInfo(2, 0)]
        CoordinateTransformation,

        [Display(Name = "Point6的世界座標(Point3D)")]
        [ConstructorInfo("{0}的三維座標", typeof(Point6), nameof(Point6.Location)), ParametersInfo(1, 0)]
        Point6D_Location,

        [Display(Name = "列表的成員"), ConstructorInfo("{0}中索引值為{1}的成員", typeof(DVFuncs), nameof(DVFuncs.Point3DElementOfList)), ParametersInfo(2, 0)]
        Point3DListElement,
        #endregion Point3D

        #region List<Point3D>
        [Display(Name = "座標變換")]
        [ConstructorInfo("使用轉換器{0}將點群{1}轉換為世界座標", typeof(DVFuncs), nameof(DVFuncs.CoordinateTransformationList)), ParametersInfo(2, 0)]
        CoordinateTransformationList,
        #endregion List<Point3D>

        #region Point6
        [Display(Name = "(X,Y,Z,A,B,C)"), ConstructorInfo("X = {0},\nY = {1},\nZ = {2},\nA = {3},\nB = {4},\nC = {5}", typeof(Point6), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), typeof(double), ShowParentheses = true), ParametersInfo(6, 0)]
        Point6Construct,
        [Display(Name = "Point3D以及A、B、C"), ConstructorInfo("Location = {0},\nA = {1},\nB = {2},\nC = {3}", typeof(Point6), typeof(Point3D), typeof(double), typeof(double), typeof(double), ShowParentheses = true), ParametersInfo(4, 0)]
        Point6FormPoint3D,
        [Display(Name = "Point6位移Point3D"), ConstructorInfo("{0}位移{1}所獲得的新點", typeof(DVFuncs), nameof(DVFuncs.Point6OffsetPoint3)), ParametersInfo(2, 0)]
        Point6OffsetPoint3,

        [Display(Name = "Point6以及新的Z值"), ConstructorInfo("{0}更改Z值為{1}所獲得的新點", typeof(DVFuncs), nameof(DVFuncs.Point6_And_Z)), ParametersInfo(2, 0)]
        Point6_And_Z,

        [Display(Name = "列表的成員"), ConstructorInfo("{0}中索引值為{1}的成員", typeof(DVFuncs), nameof(DVFuncs.Point6ElementOfList)), ParametersInfo(2, 0)]
        Point6ListElement,
        #endregion Point6

        #region List
        [Display(Name = "加入項目"), ParametersInfo(1, 0)]
        Add,
        [Display(Name = "加入集合的項目"), ParametersInfo(1, 0)]
        AddRange,
        [Display(Name = "插入項目"), ParametersInfo(2, 0)]
        Insert,
        [Display(Name = "移除指定索引的項目"), ParametersInfo(1, 0)]
        RemoveAt,
        [Display(Name = "移除項目"), ParametersInfo(1, 0)]
        Remove,
        [Display(Name = "清空列表"), ParametersInfo(0, 0)]
        Clear,
        #endregion List

        [Display(Name = "變數值"),ParametersInfo(0, 1)]
        Variable,

        [Display(Name = "依條件賦值"), ParametersInfo(3, 0)]
        Conditional,

        [Display(Name = "公式"), ParametersInfo(0,1)]
        Expression
    }

    internal static class DVFuncs
    {
        public static bool BooleanEquality(bool a, bool b) => a == b;
        public static bool BooleanInequality(bool a, bool b) => a != b;
        public static bool BooleanAnd(bool a, bool b) => a && b;
        public static bool BooleanOr(bool a, bool b) => a || b;
        public static bool RealInRange(double v, double a, double b) => v >= a && v <= b;
        public static bool RealNotInRange(double v, double a, double b) => v < a || v > b;
        public static bool BooleanElementOfList(List<bool> list, double index)
        {
            if (index % 1 != 0) throw new Exception("索引值必須為整數");
            return list[Convert.ToInt32(index)];
        }
        public static string GetString(object obj) => obj.ToString();
        public static string GetString(double a) => a.ToString();
        public static string GetString(Boolean a) => a.ToString();
        public static string GetString(Point a) => a.ToString();
        public static string GetString(Point3D a) => a.ToString();
        public static string GetString(Point6 a) => a.ToString();
        public static string StringElementOfList(List<string> list, double index)
        {
            if (index % 1 != 0) throw new Exception("索引值必須為整數");
            return list[Convert.ToInt32(index)];
        }

        public static double DoubleAddition(double x, double y) => x + y;
        public static double DoubleSubtraction(double x, double y) => x - y;
        public static double DoubleMultiply(double x, double y) => x * y;
        public static double DoubleDivision(double x, double y) => x / y;
        public static double DoubleModulus(double x, double y) => x % y;
        public static double DoubleRandom(double min, double max)
        {
            lock (syncLock)
            { 
                return random.Next((int)(min + 0.5), (int)max);
            }
        }
        public static double DoubleElementOfList(List<double> list, double index)
        {
            if (index % 1 != 0) throw new Exception("索引值必須為整數");
            return list[Convert.ToInt32(index)];
        }

        public static Point PointElementOfList(List<Point> list, double index)
        {
            if (index % 1 != 0) throw new Exception("索引值必須為整數");
            return list[Convert.ToInt32(index)];
        }

        public static Point3D CoordinateTransformation(CoordinateTransformator transformator, Point p) => transformator.T(p);
        public static IEnumerable<Point3D> CoordinateTransformationList(CoordinateTransformator transformator, IEnumerable<Point> ps) => ps.Select(x=>transformator.T(x));

        public static Point3D Point3DElementOfList(List<Point3D> list, double index)
        {
            if (index % 1 != 0) throw new Exception("索引值必須為整數");
            return list[Convert.ToInt32(index)];
        }

        public static Point6 Point6OffsetPoint3(Point6 p, Point3D offset) => new Point6(new Point3D(p.X + offset.X, p.Y + offset.Y, p.Z + offset.Z), p.A, p.B, p.C);
        public static Point6 Point6_And_Z(Point6 p, double z) => new Point6(p.X, p.Y, z, p.A, p.B, p.C);
        public static Point6 Point6ElementOfList(List<Point6> list, double index)
        {
            if (index % 1 != 0) throw new Exception("索引值必須為整數");
            return list[Convert.ToInt32(index)];
        }

        #region Private
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        #endregion Private
    }


    public class ConstructorInfoAttribute : Attribute
    {
        /// <summary>
        /// 使用自訂的編輯器
        /// </summary>
        /// <param name="resultType"></param>
        /// <param name="userControl"></param>
        public ConstructorInfoAttribute(Type resultType, Type userControl)
        {
            ResultType = resultType;
            CustomEditorType = userControl;
        }

        public string Description { get; }
        public string Detail { get; }
        public Type[] Parameters { get; }
        public Type ResultType { get; }

        public bool IsTypeAvilible(Type type) => ResultType.IsAssignableFrom(type);

        public bool UseCustomEditor => CustomEditorType != null;
        public Type CustomEditorType { get; }

        public bool ShowParentheses { get; set; } = false;
        //==================================
        MethodInfo mehodInfo;
        //ConstructorInfo constructorInfo;
        bool IsGetMethod = false;
        bool IsConstructor = false;

        public ConstructorInfoAttribute(Type resultType, Type customEditor, Type resource, string method, params Type[] parameters)
        {
            Parameters = parameters;
            ResultType = resultType;
            CustomEditorType = customEditor;
            mehodInfo = resource.GetMethod(method, parameters);
            if (mehodInfo == null) throw new Exception($"{resource.FullName}中找不到對應的方法{method}");
        }
        public ConstructorInfoAttribute(string decription, Type resource, string method, Type[] method_filter, Type resultType, Type[] parameters)
        {
            Parameters = parameters;
            ResultType = resultType;
            Description = decription;
            mehodInfo = resource.GetMethod(method, method_filter);
            if (mehodInfo == null) throw new Exception($"{resource.FullName}中找不到對應的方法{method}");
        }

        

        /// <summary>
        /// 指定類別的方法或成員
        /// </summary>
        /// <param name="description"></param>
        /// <param name="resource"></param>
        /// <param name="method_or_property"></param>
        public ConstructorInfoAttribute(string description, Type resource, string method_or_property)
        {
            Description = description;
            mehodInfo = resource.GetMethod(method_or_property, BindingFlags.Public | BindingFlags.Static);
            if (mehodInfo != null)
            {
                ResultType = mehodInfo.ReturnType;
                Parameters = mehodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
                return;
            }
            else
            {
                var property = resource.GetProperty(method_or_property);
                if (property != null)
                {
                    mehodInfo = property.GetGetMethod();
                    ResultType = property.PropertyType;
                    Parameters = new Type[] { resource };
                    IsGetMethod = true;
                    return;
                }
            }
            throw new Exception($"{resource.FullName}中找不到對應的方法或屬性{method_or_property}");
        }
        /// <summary>
        /// 指定類別的建構式
        /// </summary>
        /// <param name="description"></param>
        /// <param name="resource"></param>
        /// <param name="parameters"></param>
        public ConstructorInfoAttribute(string description, Type resource, params Type[] parameters)
        {
            var constructorInfo = resource.GetConstructor(parameters);
            if (constructorInfo == null) throw new Exception($"{resource.FullName}中找不到對應的建構式{parameters}");
            Description = description;
            ResultType = resource;
            Parameters = parameters;
            IsConstructor = true;
        }
        /// <summary>
        /// 指定類別的方法
        /// </summary>
        /// <param name="description"></param>
        /// <param name="resource"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        public ConstructorInfoAttribute(string description, Type resource, string method, params Type[] parameters)
        {
            Description = description;
            if (parameters == null) parameters = new Type[0];
             mehodInfo = resource.GetMethod(method, parameters);
            if (mehodInfo == null) throw new Exception($"{resource.FullName}中找不到對應的方法{method}");
            ResultType = mehodInfo.ReturnType;
            Parameters = parameters;
        }
        public object Func(params object[] parameters)
        {
            if (IsGetMethod)
                return mehodInfo?.Invoke(parameters[0], null);
            else if (IsConstructor)
                return Activator.CreateInstance(ResultType, parameters);
            else
                return mehodInfo?.Invoke(null, parameters);
        }
    }

    public class ParametersInfoAttribute : Attribute
    {
        public ParametersInfoAttribute(int number_of_references,int number_of_args)
        {
            ReferencesCount = number_of_references;
            ArgumentsCount = number_of_args;
            
        }
        public int ReferencesCount { get; }
        public int ArgumentsCount { get; }


    }

    public class FunctionAttribute : Attribute
    {
        public FunctionAttribute( string functionName)
        {
        }
        public FunctionAttribute(Type type,string functionName)
        {
        }

        public FunctionAttribute(Type type, string functionName,Type returnType,params Type[] parameterTypes)
        {

        }
    }

    public class CurrentType
    {

    }



}
