using System.Collections;
using System.ComponentModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.CSharp.RuntimeBinder;

namespace Sunspot.Core.Helper.ClassExtensions;

public static class ObjectExtension
{
    private static DateTime _dt1970 = new DateTime(1970, 1, 1);

    private static readonly BindingFlags bf = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                              BindingFlags.NonPublic;
    
    private static Func<TIn, TOut> GetFunc<TIn, TOut>()
    {
        ParameterExpression parameterExpression = Expression.Parameter(typeof(TIn), "p");
        List<MemberBinding> memberBindingList = new List<MemberBinding>();

        foreach (PropertyInfo property in typeof(TOut).GetProperties())
        {
            if (property.CanWrite)
            {
                MemberExpression memberExpression = Expression.Property(parameterExpression,
                    typeof(TIn).GetProperty(property.Name));
                MemberBinding memberBinding =
                    Expression.Bind(property, memberExpression);
                memberBindingList.Add(memberBinding);
            }
        }

        return Expression
            .Lambda<Func<TIn, TOut>>(
                Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray()),
                parameterExpression).Compile();
    }
    
    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <param name="inT"></param>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    public static TOut Clone<TIn, TOut>(this TIn inT)
    {
        return GetFunc<TIn, TOut>()(inT);
    }
    
    private static object CastTo(this object value, Type conversionType)
    {
        if (value == null)
        {
            return null;
        }

        if (conversionType.IsNullableType())
        {
            conversionType = conversionType.GetUnNullableType();
        }

        if (conversionType.IsEnum)
        {
            return Enum.Parse(conversionType, value.ToString());
        }

        return conversionType == typeof(Guid) ? Guid.Parse(value.ToString()) : Convert.ChangeType(value, conversionType);
    }
    
    private static T CastTo<T>(this object value)
    {
        if (value == null && default(T) == null)
        {
            return default;
        }
            
        return value.GetType() == typeof(T) ? (T) value : (T) value.CastTo(typeof(T));
    }
    
    /// <summary>
    /// 类型转换
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T CastTo<T>(this object value, T defaultValue) where T : IConvertible
    {
        try
        {
            return value.CastTo<T>();
        }
        catch (Exception ex)
        {
            return defaultValue;
        }
    }
    
    /// <summary>
    /// 判断当前值类型是否介于两者之间
    /// </summary>
    /// <param name="value"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="leftEqual">是否左相等</param>
    /// <param name="rightEqual">是否右相等</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool IsBetween<T>(
        this IComparable<T> value,
        T start,
        T end,
        bool leftEqual = false,
        bool rightEqual = false)
        where T : IComparable
    {
        return (leftEqual ? value.CompareTo(start) >= 0 : value.CompareTo(start) > 0) &
               (rightEqual ? value.CompareTo(end) <= 0 : value.CompareTo(end) < 0);
    }
    
    /// <summary>
    /// 对象转为byte[]
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this object value)
    {
        if (value == null)
        {
            return null;
        }
        
        using (MemoryStream ms = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, value);
            return ms.GetBuffer();
        }
    }
    
    /// <summary>
    /// byte[]转对象
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T ToObject<T>(this byte[] value)
    {
        try
        {
            if (value == null || value.Length == 0)
            {
                return default;
            }
            
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream serializationStream = new MemoryStream(value);
            T obj = (T)binaryFormatter.Deserialize(serializationStream);
            serializationStream.Dispose();
            
            return obj;
        }
        catch
        {
            return default(T);
        }
    }
    
    /// <summary>
    /// 两个byte[]是否相等
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool EqualByte(this byte[] source, byte[] target)
    {
       return StructuralComparisons.StructuralEqualityComparer.Equals(source, target);
    }
    
    /// <summary>
    /// 获取对象属性对象的Display标签的Name
    /// </summary>
    /// <param name="member"></param>
    /// <param name="inherit">继承关系</param>
    /// <returns></returns>
    public static string GetDisplayName(this MemberInfo member, bool inherit = true)
    {
        DisplayNameAttribute customAttribute = member.GetCustomAttribute<DisplayNameAttribute>(inherit);

        return customAttribute != null && !string.IsNullOrWhiteSpace(customAttribute.DisplayName)
            ? customAttribute.DisplayName
            : null;
    }
    
    /// <summary>
    /// 获取第一个自定义标签的内容
    /// </summary>
    /// <param name="target"></param>
    /// <param name="inherit"></param>
    /// <typeparam name="TAttribute"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static TResult GetCustomAttributeValue<TAttribute, TResult>(
        this MemberInfo target,
        bool inherit = true)
        where TAttribute : Attribute
    {
        if (target == null)
        {
            return default;
        }

        try
        {
            IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(target);

            if (customAttributes != null && customAttributes.Count > 0)
            {
                foreach (CustomAttributeData customAttributeData in customAttributes)
                {
                    if (!(typeof(TAttribute).FullName != customAttributeData.Constructor.DeclaringType.FullName))
                    {
                        IList<CustomAttributeTypedArgument> constructorArguments =
                            customAttributeData.ConstructorArguments;

                        if (constructorArguments != null && constructorArguments.Count > 0)
                        {
                            return (TResult) constructorArguments[0].Value;
                        }
                    }
                }
            }

            if (inherit && target is Type)
            {
                target = (MemberInfo) (target as Type).BaseType;
                if (target != null && target != typeof(object))
                {
                    return target.GetCustomAttributeValue<TAttribute, TResult>(inherit);
                }
            }
        }
        catch
        {
            if (!target.Module.Assembly.ReflectionOnly)
            {
                TAttribute customAttribute = target.GetCustomAttribute<TAttribute>(inherit);

                if ((object) customAttribute != null)
                {
                    PropertyInfo property =
                        (typeof(TAttribute).GetProperties()).FirstOrDefault(
                            p => p.PropertyType == typeof(TResult));
                    if (property != null)
                        return (TResult) GetValue(customAttribute, property);
                }
            }
        }

        return default;
    }

    public static object GetValue(this object target, MemberInfo member)
    {
        if (member == null)
        {
            member = target as MemberInfo;
            target = (object) null;
        }

        switch (member)
        {
            case PropertyInfo _:
                return GetValue(target, member as PropertyInfo);
            case FieldInfo _:
                return GetValue(target, member as FieldInfo);
            default:
                throw new ArgumentOutOfRangeException(nameof(member));
        }
    }

    public static void SetValue(this object target, MemberInfo member, object value)
    {
        switch (member)
        {
            case PropertyInfo _:
                SetValue(target, member as PropertyInfo, value);
                break;
            case FieldInfo _:
                SetValue(target, member as FieldInfo, value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(member));
        }
    }

    private static object GetValue(this object target, PropertyInfo property) =>
        property.GetValue(target, (object[]) null);

    private static object GetValue(this object target, FieldInfo field) => field.GetValue(target);

    private static void SetValue(this object target, PropertyInfo property, object value) =>
        property.SetValue(target, value.ChangeType(property.PropertyType),null);

    private static void SetValue(this object target, FieldInfo field, object value) =>
        field.SetValue(target, value.ChangeType(field.FieldType));

    public static bool As<T>(this Type type) => type.As(typeof(T));

    private static bool As(this Type type, Type baseType)
    {
        if (type == (Type) null)
            return false;
        if (type == baseType)
            return true;
        if (baseType.IsGenericTypeDefinition && type.IsGenericType && !type.IsGenericTypeDefinition &&
            baseType is TypeInfo typeInfo && typeInfo.GenericTypeParameters.Length == type.GenericTypeArguments.Length)
            baseType = baseType.MakeGenericType(type.GenericTypeArguments);
        return type == baseType || baseType.IsAssignableFrom(type);
    }

    public static bool ToBoolean(this object value, bool defaultValue = false)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;
        if (value is string str1)
        {
            string str = str1.Trim();
            if (str.IsNullOrEmpty())
                return defaultValue;
            bool result1;
            if (bool.TryParse(str, out result1))
                return result1;
            if (string.Equals(str, bool.TrueString, StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(str, bool.FalseString, StringComparison.OrdinalIgnoreCase))
                return false;
            int result2;
            return int.TryParse(str.ToDBC(), out result2) ? result2 > 0 : defaultValue;
        }

        try
        {
            return Convert.ToBoolean(value);
        }
        catch
        {
            return defaultValue;
        }
    }

    public static double ToDouble(this object value, double defaultValue = 0.0)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;
        switch (value)
        {
            case string str:
                string s = str.ToDBC().Trim();
                double result;
                return s.IsNullOrEmpty() || !double.TryParse(s, out result) ? defaultValue : result;
            case byte[] src:
                if (src == null || src.Length < 1)
                    return defaultValue;
                switch (src.Length)
                {
                    case 1:
                        return (double) src[0];
                    case 2:
                        return (double) BitConverter.ToInt16(src, 0);
                    case 3:
                        return (double) BitConverter.ToInt32(new byte[4]
                        {
                            src[0],
                            src[1],
                            src[2],
                            (byte) 0
                        }, 0);
                    case 4:
                        return (double) BitConverter.ToInt32(src, 0);
                    default:
                        if (src.Length < 8)
                        {
                            byte[] dst = new byte[8];
                            Buffer.BlockCopy((Array) src, 0, (Array) dst, 0, src.Length);
                            src = dst;
                        }

                        return BitConverter.ToDouble(src, 0);
                }
            default:
                try
                {
                    return Convert.ToDouble(value);
                }
                catch
                {
                    return defaultValue;
                }
        }
    }

    public static int ToInt(this object value, int defaultValue = 0)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;
        int num;
        switch (value)
        {
            case string str:
                string s = str.Replace(",", (string) null).ToDBC().Trim();
                int result;
                return s.IsNullOrEmpty() || !int.TryParse(s, out result) ? defaultValue : result;
            case DateTime dateTime:
                num = 1;
                break;
            default:
                num = 0;
                break;
        }

        if (num != 0)
            return dateTime == DateTime.MinValue ? 0 : (int) (dateTime - _dt1970).TotalSeconds;
        if (value is byte[] numArray)
        {
            if (numArray == null || numArray.Length < 1)
                return defaultValue;
            switch (numArray.Length)
            {
                case 1:
                    return (int) numArray[0];
                case 2:
                    return (int) BitConverter.ToInt16(numArray, 0);
                case 3:
                    return BitConverter.ToInt32(new byte[4]
                    {
                        numArray[0],
                        numArray[1],
                        numArray[2],
                        (byte) 0
                    }, 0);
                case 4:
                    return BitConverter.ToInt32(numArray, 0);
            }
        }

        try
        {
            return Convert.ToInt32(value);
        }
        catch
        {
            return defaultValue;
        }
    }

    public static long ToLong(this object value, long defaultValue = 0)
    {
        if (value == null || value == DBNull.Value)
            return defaultValue;
        int num;
        switch (value)
        {
            case string str:
                string s = str.Replace(",", (string) null).ToDBC().Trim();
                int result;
                return s.IsNullOrEmpty() || !int.TryParse(s, out result) ? defaultValue : (long) result;
            case DateTime dateTime:
                num = 1;
                break;
            default:
                num = 0;
                break;
        }

        if (num != 0)
            return dateTime == DateTime.MinValue ? 0L : (long) (int) (dateTime - _dt1970).TotalMilliseconds;
        if (value is byte[] numArray)
        {
            if (numArray == null || numArray.Length < 1)
                return defaultValue;
            switch (numArray.Length)
            {
                case 1:
                    return (long) numArray[0];
                case 2:
                    return (long) BitConverter.ToInt16(numArray, 0);
                case 3:
                    return (long) BitConverter.ToInt32(new byte[4]
                    {
                        numArray[0],
                        numArray[1],
                        numArray[2],
                        (byte) 0
                    }, 0);
                case 4:
                    return (long) BitConverter.ToInt32(numArray, 0);
                case 8:
                    return BitConverter.ToInt64(numArray, 0);
            }
        }

        try
        {
            return Convert.ToInt64(value);
        }
        catch
        {
            return defaultValue;
        }
    }

    public static string ToDBC(this string str)
    {
        char[] charArray = str.ToCharArray();
        for (int index = 0; index < charArray.Length; ++index)
        {
            if (charArray[index] == '　')
                charArray[index] = ' ';
            else if (charArray[index] > '\uFF00' && charArray[index] < '｟')
                charArray[index] = (char) ((uint) charArray[index] - 65248U);
        }

        return new string(charArray);
    }

    public static MethodInfo GetMethodEx(
        this Type type,
        string name,
        params Type[] paramTypes)
    {
        if (name.IsNullOrEmpty())
            return (MethodInfo) null;
        return paramTypes.Length != 0 &&
               ((IEnumerable<Type>) paramTypes).Any<Type>((Func<Type, bool>) (e => e == (Type) null))
            ? ((IEnumerable<MethodInfo>) GetMethods(type, name, paramTypes.Length)).FirstOrDefault<MethodInfo>()
            : GetMethod(type, name, paramTypes);
    }

    public static MethodInfo GetMethod(Type type, string name, params Type[] paramTypes)
    {
        MethodInfo method;
        do
        {
            method = paramTypes != null && paramTypes.Length != 0
                ? type.GetMethod(name, bf, (System.Reflection.Binder) null, paramTypes, (ParameterModifier[]) null)
                : type.GetMethod(name, bf);
            if (!(method != (MethodInfo) null))
                type = type.BaseType;
            else
                goto label_1;
        } while (!(type == (Type) null) && !(type == typeof(object)));

        goto label_4;
        label_1:
        return method;
        label_4:
        return (MethodInfo) null;
    }

    public static MethodInfo[] GetMethods(Type type, string name, int paramCount = -1)
    {
        MethodInfo[] methods = type.GetMethods(bf);
        if (methods == null || methods.Length == 0)
            return methods;
        List<MethodInfo> methodInfoList = new List<MethodInfo>();
        foreach (MethodInfo methodInfo in methods)
        {
            if (methodInfo.Name == name && paramCount >= 0 && methodInfo.GetParameters().Length == paramCount)
                methodInfoList.Add(methodInfo);
        }

        return methodInfoList.ToArray();
    }
}