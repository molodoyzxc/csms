namespace Extensions;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<IEnumerable<T>> ZipManyAsync<T>(
        this IAsyncEnumerable<T> source,
        params IAsyncEnumerable<T>[] others)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(others);
        var enumerators = new[] { source.GetAsyncEnumerator() }
            .Concat(others.Select(o => o.GetAsyncEnumerator()))
            .ToList();
        try
        {
            while (await Task.WhenAll(enumerators.Select(e => e.MoveNextAsync().AsTask())).ConfigureAwait(false) is { } results && results.All(m => m))
            {
                yield return enumerators.Select(e => e.Current).ToList();
            }
        }
        finally
        {
            foreach (IAsyncEnumerator<T>? enumerator in enumerators)
            {
                await enumerator.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}