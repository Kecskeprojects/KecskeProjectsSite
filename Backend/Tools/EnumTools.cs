using System.Reflection;

namespace Backend.Tools;

public static class EnumTools
{
    public static string? GetDescription<T>(this T value) where T : struct, IConvertible
    {
        FieldInfo? fieldInfo = value.GetType().GetField(value.ToString()!);
        if (fieldInfo == null)
        {
            return value.ToString();
        }
        object[] attributes = fieldInfo.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
        return attributes.Length > 0
            ? ((System.ComponentModel.DescriptionAttribute) attributes[0]).Description
            : value.ToString();
    }

    public static T ParseEnum<T>(string value) where T : struct, IConvertible
    {
        return Enum.TryParse(value, true, out T result)
            ? result
            : throw new ArgumentException($"'{value}' is not a valid value for enum '{typeof(T).Name}'.");
    }
}
