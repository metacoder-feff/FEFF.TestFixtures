namespace FEFF.Extensions.Tests;

public class EnumMatchExceptionTests
{
    enum A { a1 = 1, a2 = 2 };
    enum B : byte { b1 };

    [Fact]
    public void Create__default()
    {
        var e = EnumMatchException.From(A.a1);

        e.Should().BeOfType<EnumMatchException>();
        e.Message.Should().Be("Enum match error: Got enum type 'A' with value 'a1' (1).");
    }

    [Fact]
    public void Create__from_byte_enum()
    {
        var e = EnumMatchException.From(B.b1);

        e.Should().BeOfType<EnumMatchException>();
        e.Message.Should().Be("Enum match error: Got enum type 'B' with value 'b1' (0).");
    }

    [Fact]
    public void Create__from_unknown_int()
    {
        var e = EnumMatchException.From((A)100);

        e.Should().BeOfType<EnumMatchException>();
        e.Message.Should().Be("Enum match error: Got enum type 'A' with value '100' (100) - value is not defined in the enum type.");
    }

    [Fact]
    public void Create__from_flags()
    {
        var e = EnumMatchException.From(A.a1 | A.a2);

        e.Should().BeOfType<EnumMatchException>();
        e.Message.Should().Be("Enum match error: Got enum type 'A' with value 'a1, a2' (3) - value contains multiple flags.");
    }
}