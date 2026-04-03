namespace FEFF.Extensions.Tests;

public class CollectionsExtensionsTests
{
    [Fact]
    public void WhereNotNull_struct()
    {
        new int?[] {null, 1, null, 5}
            .WhereNotNull()
            .Should().BeEquivalentTo([1, 5]);
    }

    [Fact]
    public void WhereNotNull_class()
    {
        new string?[] {null, "1", null, "5"}
            .WhereNotNull()
            .Should().BeEquivalentTo(["1", "5"]);
    }
}