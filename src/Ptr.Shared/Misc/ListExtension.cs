namespace Ptr.Shared.Misc;

public static class ListExtension
{
    public static void ForEachCatchException<T>(this IReadOnlyList<T> list, Action<T> action, Action<Exception> ex)
    {
        foreach (var item in list)
        {
            try
            {
                action(item);
            }
            catch (Exception e)
            {
                ex(e);
            }
        }
    }
}