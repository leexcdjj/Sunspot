namespace Sunspot.Core.Helper.ClassExtensions;

public static class TypeExtension
  {
    public static Type GetElementTypeEx(this Type type)
    {
      return GetElementType(type);
    }

    public static Type GetElementType(Type type)
    {
      if (type.HasElementType)
      {
        return type.GetElementType();
      }
      
      if (type.As<IEnumerable>())
      {
        foreach (Type type1 in type.GetInterfaces())
        {
          if (type1.IsGenericType && type1.GetGenericTypeDefinition() == typeof (IEnumerable<>))
            return type1.GetGenericArguments()[0];
        }
      }
      return (Type) null;
    }

    public static object Invoke(this object target, MethodBase method, params object[] parameters)
    {
      if (method == (MethodBase) null)
        throw new ArgumentNullException(nameof (method));
      if (!method.IsStatic && target == null)
        throw new ArgumentNullException(nameof (target));
      return method.Invoke(target, parameters);
    }

    public static object CreateInstance(this Type type, params object[] parameters)
    {
      try
      {
        return parameters == null || parameters.Length == 0 ? Activator.CreateInstance(type, true) : Activator.CreateInstance(type, parameters);
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
      }
    }

    public static TypeCode GetTypeCode(this Type type) => Type.GetTypeCode(type);

    public static bool IsDeriveClassFrom<TBaseType>(this Type type, bool canAbstract = false) => type.IsDeriveClassFrom(typeof (TBaseType), canAbstract);

    public static bool IsDeriveClassFrom(this Type type, Type baseType, bool canAbstract = false)
    {
      Check.NotNull<Type>(type, nameof (type));
      Check.NotNull<Type>(baseType, nameof (baseType));
      return type.IsClass && !canAbstract && !type.IsAbstract && type.IsBaseOn(baseType);
    }

    public static string ToLower(this bool value) => value.ToString().ToLower();

    public static bool IsWeekend(this DateTime dateTime) => ((IEnumerable<DayOfWeek>) new DayOfWeek[2]
    {
      DayOfWeek.Saturday,
      DayOfWeek.Sunday
    }).Contains<DayOfWeek>(dateTime.DayOfWeek);

    public static bool IsWeekday(this DateTime dateTime) => ((IEnumerable<DayOfWeek>) new DayOfWeek[5]
    {
      DayOfWeek.Monday,
      DayOfWeek.Tuesday,
      DayOfWeek.Wednesday,
      DayOfWeek.Thursday,
      DayOfWeek.Friday
    }).Contains<DayOfWeek>(dateTime.DayOfWeek);

    public static string ToUniqueString(this DateTime dateTime, bool milsec = false)
    {
      int num = dateTime.Hour * 3600 + dateTime.Minute * 60 + dateTime.Second;
      string str = string.Format("{0}{1}{2}", (object) dateTime.ToString("yy"), (object) dateTime.DayOfYear, (object) num);
      return milsec ? str + dateTime.ToString("fff") : str;
    }

    public static string ToDescription(this Enum value)
    {
      MemberInfo memberInfo = ((IEnumerable<MemberInfo>) value.GetType().GetMember(value.ToString())).FirstOrDefault<MemberInfo>();
      return memberInfo != (MemberInfo) null ? memberInfo.ToDescription() : value.ToString();
    }

    public static bool IsNullableType(this Type type) => type != (Type) null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);

    public static Type GetNonNummableType(this Type type) => type.IsNullableType() ? type.GetGenericArguments()[0] : type;

    public static Type GetUnNullableType(this Type type) => type.IsNullableType() ? new NullableConverter(type).UnderlyingType : type;

    public static bool HasAttribute<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute => memberInfo.IsDefined(typeof (T), inherit);

    public static T GetAttribute<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute => ((IEnumerable<object>) memberInfo.GetCustomAttributes(typeof (T), inherit)).FirstOrDefault<object>() as T;

    public static T[] GetAttributes<T>(this MemberInfo memberInfo, bool inherit = false) where T : Attribute => memberInfo.GetCustomAttributes(typeof (T), inherit).Cast<T>().ToArray<T>();

    public static string ToDescription(this Type type, bool inherit = false)
    {
      DescriptionAttribute attribute = type.GetAttribute<DescriptionAttribute>(inherit);
      return attribute == null ? type.FullName : attribute.Description;
    }

    public static string ToDescription(this MemberInfo memberInfo, bool inherit = false)
    {
      DescriptionAttribute attribute1 = memberInfo.GetAttribute<DescriptionAttribute>(inherit);
      if (attribute1 != null)
        return attribute1.Description;
      DisplayAttribute attribute2 = memberInfo.GetAttribute<DisplayAttribute>(inherit);
      return attribute2 != null ? attribute2.Name : memberInfo.Name;
    }

    public static bool IsEnumerable(this Type type) => !(type == typeof (string)) && typeof (IEnumerable).IsAssignableFrom(type);

    public static bool IsGenericAssignableFrom(this Type genericType, Type type)
    {
      genericType.CheckNotNull<Type>(nameof (genericType));
      type.CheckNotNull<Type>(nameof (type));
      if (!genericType.IsGenericType)
        throw new ArgumentException("该功能只支持泛型类型的调用，非泛型类型可使用 IsAssignableFrom 方法！");
      List<Type> typeList = new List<Type>()
      {
        type
      };
      if (genericType.IsInterface)
        typeList.AddRange((IEnumerable<Type>) type.GetInterfaces());
      foreach (Type type1 in typeList)
      {
        for (Type type2 = type1; type2 != (Type) null; type2 = type2.BaseType)
        {
          if (type2.IsGenericType)
            type2 = type2.GetGenericTypeDefinition();
          if (type2.IsSubclassOf(genericType) || type2 == genericType)
            return true;
        }
      }
      return false;
    }

    public static bool IsAsync(this MethodInfo methodInfo) => methodInfo.ReturnType == typeof (Task) || methodInfo.ReturnType.IsGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof (Task<>);

    public static bool IsBaseOn(this Type type, Type baseType) => baseType.IsGenericTypeDefinition ? baseType.IsGenericAssignableFrom(type) : baseType.IsAssignableFrom(type);

    public static bool IsBaseOn<TBaseType>(this Type type)
    {
      Type baseType = typeof (TBaseType);
      return type.IsBaseOn(baseType);
    }

    public static string GetFullNameWithModule(this Type type) => type.FullName + "," + type.Module.Name.Replace(".dll", "").Replace(".exe", "");

    public static string GetName(this Type type) => type.Name;

    public static string GetDescription(this Type type, bool inherit = false)
    {
      DescriptionAttribute attribute = type.GetAttribute<DescriptionAttribute>(inherit);
      return attribute == null ? type.FullName : attribute.Description;
    }

    public static string GetDescription(this MemberInfo member, bool inherit = false)
    {
      DescriptionAttribute attribute1 = member.GetAttribute<DescriptionAttribute>(inherit);
      if (attribute1 != null)
        return attribute1.Description;
      DisplayNameAttribute attribute2 = member.GetAttribute<DisplayNameAttribute>(inherit);
      if (attribute2 != null)
        return attribute2.DisplayName;
      DisplayAttribute attribute3 = member.GetAttribute<DisplayAttribute>(inherit);
      return attribute3 != null ? attribute3.Name : member.Name;
    }

    public static object To(this object value, Type destinationType) => To(value, destinationType, CultureInfo.InvariantCulture);

    public static object To(object value, Type destinationType, CultureInfo culture)
    {
      if (value != null)
      {
        Type type = value.GetType();
        TypeConverter converter1 = TypeDescriptor.GetConverter(destinationType);
        if (converter1.CanConvertFrom(value.GetType()))
          return converter1.ConvertFrom((ITypeDescriptorContext) null, culture, value);
        TypeConverter converter2 = TypeDescriptor.GetConverter(type);
        if (converter2.CanConvertTo(destinationType))
          return converter2.ConvertTo((ITypeDescriptorContext) null, culture, value, destinationType);
        if (destinationType.IsEnum && value is int num)
          return Enum.ToObject(destinationType, num);
        if (!destinationType.IsInstanceOfType(value))
          return Convert.ChangeType(value, destinationType, (IFormatProvider) culture);
      }
      return value;
    }

    public static T To<T>(this object value) => (T) value.To(typeof (T));
  }