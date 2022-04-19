using System.Text;

namespace Build.Extensions;

public static class StringExtensions
{
    public static T ToEnum<T>(this string value)
    {
        return (T)Enum.Parse(typeof(T), value, true);
    }
    public static Stream ToStream(this string value) => ToStream(value, Encoding.UTF8);

    public static Stream ToStream(this string value, Encoding encoding) 
        => new MemoryStream(encoding.GetBytes(value ?? string.Empty));
}
