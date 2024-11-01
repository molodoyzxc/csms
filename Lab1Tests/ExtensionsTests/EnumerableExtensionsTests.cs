using Extensions;
using FluentAssertions;

namespace ExtensionsTests;

public class EnumerableExtensionsTests
{
    public static IEnumerable<object[]> GetEqualLengthCollections()
    {
        yield return new object[]
        {
            new[] { 1, 2, 3 },
            new IEnumerable<int>[] { new[] { 4, 5, 6 }, new[] { 7, 8, 9 } },
        };
    }

    [Fact]
    public void ZipMany_NoAdditionalCollections_ReturnsSingleElementCollections()
    {
        int[] source = new[] { 1, 2, 3 };
        IEnumerable<IEnumerable<int>> result = source.ZipMany();
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
    [MemberData(nameof(GetEqualLengthCollections))]
    public void ZipMany_EqualLengthCollections_ReturnsZippedCollections(
        IEnumerable<int> source,
        IEnumerable<int>[] others)
    {
        IEnumerable<IEnumerable<int>> result = source.ZipMany(others);
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