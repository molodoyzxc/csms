using Extensions;
using FluentAssertions;

namespace ExtensionsTests;

public class AsyncEnumerableExtensionsTests
{
    public static IEnumerable<object[]> GetEqualLengthAsyncCollections()
    {
        yield return new object[]
        {
            AsyncEnumerable.Range(1, 3),
            new IAsyncEnumerable<int>[] { AsyncEnumerable.Range(4, 3), AsyncEnumerable.Range(7, 3) },
        };
    }

    [Fact]
    public async Task ZipManyAsync_NoAdditionalCollections_ReturnsSingleElementCollections()
    {
        IAsyncEnumerable<int> source = AsyncEnumerable.Range(1, 3);
        List<IEnumerable<int>> result = await source.ZipManyAsync().ToListAsync().ConfigureAwait(true);
        result.Should()
            .BeEquivalentTo(new List<IEnumerable<int>>
            {
#pragma warning disable CA1861
                new[] { 1 },
                new[] { 2 },
                new[] { 3 },
#pragma warning restore CA1861
            });
    }

    [Theory]
    [MemberData(nameof(GetEqualLengthAsyncCollections))]
    public async Task ZipManyAsync_EqualLengthCollections_ReturnsZippedCollections(
        IAsyncEnumerable<int> source,
        IAsyncEnumerable<int>[] others)
    {
        List<IEnumerable<int>> result = await source.ZipManyAsync(others).ToListAsync().ConfigureAwait(true);
        result.Should()
            .BeEquivalentTo(new List<IEnumerable<int>>
            {
#pragma warning disable CA1861
                new[] { 1, 4, 7 },
                new[] { 2, 5, 8 },
                new[] { 3, 6, 9 },
#pragma warning restore CA1861
            });
    }
}