namespace Build.Extensions;

public static class LinqExtensions
{
    public static T When<T>(this T obj, bool condition, Func<T, T> action)
    {
        return condition ? action.Invoke(obj) : obj;
    }
}