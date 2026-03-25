namespace FEFF.Extentions.Tests;

public class ThrowHelperTests
{
    #region ThrowHelper.Assert(bool)
    [Fact]
    public void InvalidOperation_Assert__positive()
    {
        Action act = () => ThrowHelper.Assert(true);

        act.Should().NotThrow();
    }

    [Fact]
    public void InvalidOperation_Assert__should_throw()
    {
        Action act = () => ThrowHelper.Assert(false);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Assertion violated: 'false'");
    }

    [Fact]
    public void InvalidOperation_Assert__should_throw__with_complex_message()
    {
        var a = 7;
        Action act = () => ThrowHelper.Assert(a > 8);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Assertion violated: 'a > 8'");
    }

    [Fact]
    public void InvalidOperation_Assert__should_NOT_throw_if_ok()
    {
        var a = 7;
        ThrowHelper.Assert(true);
        ThrowHelper.Assert(a < 8);
    }
    #endregion
    
    #region ThrowHelper.Argument.ThrowIfNullOrEmpty(string?)
    private static Action ThrowIfNullOrEmptyString(string? str)
    {
        return () => ThrowHelper.Argument.ThrowIfNullOrEmpty(str);
    }

    [Fact]
    public void Argument_String_ThrowIfNullOrEmpty__positive()
    {
        ThrowIfNullOrEmptyString("1")
            .Should()
            .NotThrow();
    }
    [Fact]
    public void Argument_String_ThrowIfNullOrEmpty__empty()
    {
        ThrowIfNullOrEmptyString("")
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("The value cannot be an empty string. (Parameter 'str')");
    }
    
    [Fact]
    public void Argument_String_ThrowIfNullOrEmpty__null()
    {
        ThrowIfNullOrEmptyString(null)
            .Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'str')");
    }
    #endregion

    #region ThrowHelper.Argument.ThrowIfNullOrEmpty<T>(IEnumerable<T>?)
    private static Action ThrowIfNullOrEmptyList(List<int>? list)
    {
        return () => ThrowHelper.Argument.ThrowIfNullOrEmpty(list);
    }

    [Fact]
    public void Argument_Enumerable_ThrowIfNullOrEmpty__positive()
    {
        ThrowIfNullOrEmptyList([1])
            .Should()
            .NotThrow();
    }
    
    [Fact]
    public void Argument_Enumerable_ThrowIfNullOrEmpty__null()
    {
        ThrowIfNullOrEmptyList(null)
            .Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'list')");
    }

    [Fact]
    public void Argument_Enumerable_ThrowIfNullOrEmpty__empty()
    {
        ThrowIfNullOrEmptyList([])
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("The value cannot be an empty collection. (Parameter 'list')");
    }
    #endregion
    
    #region ThrowHelper.EnsureNotNull()
    private static string? ToNullable(string? s) => s;

    [Fact]
    public void EnsureNotNull__when_NOT_null__should_return()
    {
        var e = "123";
        var s = ToNullable(e);
        var r = ThrowHelper.EnsureNotNull(s);
        r.Should().Be(e);
    }

    [Fact]
    public void EnsureNotNull__when_null__should_throw()
    {
        var s = ToNullable(null);

        Action a = () => ThrowHelper.EnsureNotNull(s);

        a.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Value cannot be null. (Expression 's')");
    }

    [Fact]
    public void EnsureNotNull__should_throw_long_message()
    {
        var s = ToNullable(null);

        Action a = () => ThrowHelper.EnsureNotNull(s?.Substring(10)?.ToUpper());

        a.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Value cannot be null. (Expression 's?.Substring(10)?.ToUpper()')");
    }
    #endregion
}