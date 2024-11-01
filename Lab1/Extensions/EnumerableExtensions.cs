namespace Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> ZipMany<T>(this IEnumerable<T> source, params IEnumerable<T>[] others)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(others);
        var enumerators = new[] { source.GetEnumerator() }
            .Concat(others.Select(o => o.GetEnumerator()))
            .ToList();
        try
        {
            while (enumerators.All(e => e.MoveNext()))
            {
                yield return enumerators.Select(e => e.Current).ToList();
            }
        }
        finally
        {
            foreach (IEnumerator<T>? enumerator in enumerators)
            {
                enumerator.Dispose();
            }
        }
    }
}