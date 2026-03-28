using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FEFF.Extentions.Tests;

public class DisposeHelperTests
{
    #region Positive: Empty
    [Fact]
    public void Empty__SyncList__should_not_throw()
    {
        var dd = new List<IDisposable>();
        DisposeHelper.Dispose(dd);
    }

    [Fact]
    public async Task Empty__AsyncList__should_not_throw()
    {
        var dd = new List<IAsyncDisposable>();
        await DisposeHelper.DisposeAsync(dd);
    }

    [Fact]
    public async Task Empty__ObjectList__should_not_throw()
    {
        var dd = new List<object>();
        await DisposeHelper.DisposeAsync(dd);
    }
    #endregion

    #region Positive: Single
    [Fact]
    public void Single__Disp_should_be_disposed()
    {
        var d = new Disp();
        d.IsDisposed.Should().BeFalse();

        DisposeHelper.Dispose([d]);
        d.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task Single__ADisp0__should_be_disposed()
    {
        var d = new ADisp0();
        d.IsDisposed.Should().BeFalse();

        await DisposeHelper.DisposeAsync([d]);
        d.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public async Task Single__ADisp1__should_be_disposed()
    {
        var d = new ADisp1();
        d.IsDisposed.Should().BeFalse();

        await DisposeHelper.DisposeAsync([d]);
        d.IsDisposed.Should().BeTrue();
    }
    #endregion

    #region Positive: Many
    [Fact]
    public void Many__Disp__should_be_disposed()
    {
        var dd = new Disp[] { new(), new() };
        dd.Should().AllSatisfy( x => 
            x.IsDisposed.Should().BeFalse()
        );

        DisposeHelper.Dispose(dd);
        dd.Should().AllSatisfy( x => 
            x.IsDisposed.Should().BeTrue()
        );
    }

    [Fact]
    public async Task Many__ADisp0__should_be_disposed()
    {
        var dd = new ADisp0[] { new(), new() };
        dd.Should().AllSatisfy( x => 
            x.IsDisposed.Should().BeFalse()
        );

        await DisposeHelper.DisposeAsync(dd);
        dd.Should().AllSatisfy( x => 
            x.IsDisposed.Should().BeTrue()
        );
    }

    [Fact]
    public async Task Many__ADisp1__should_be_disposed()
    {
        var dd = new ADisp1[] { new(), new() };
        dd.Should().AllSatisfy( x => 
            x.IsDisposed.Should().BeFalse()
        );

        await DisposeHelper.DisposeAsync(dd);
        dd.Should().AllSatisfy( x => 
            x.IsDisposed.Should().BeTrue()
        );
    }

    [Fact]
    public async Task Many__mixed__should_be_disposed()
    {
        var dd = new IDisp[] { new Disp(), new ADisp0(), new ADisp1() };
        dd.Should().AllSatisfy( x => 
            x.IsDisposed.Should().BeFalse()
        );

        await DisposeHelper.DisposeAsync(dd);
        dd.Should().AllSatisfy( x => 
            x.IsDisposed.Should().BeTrue()
        );
    }
    #endregion

    #region Negative: All throw
    [Theory]
    [InlineData(typeof(ErrDisp)  , Label = nameof(ErrDisp))]
    [InlineData(typeof(ErrADisp0), Label = nameof(ErrADisp0))]
    [InlineData(typeof(ErrADisp1), Label = nameof(ErrADisp1))]
    public async Task Single_should_throw__InvalidOperationException(Type t)
    {
        var d = (IDisp)Activator.CreateInstance(t)!;

        var act = () => DisposeHelper.DisposeAsync([d]).AsTask();
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("test-err");
    }

    [Theory]
    [InlineData(typeof(ErrDisp)  , Label = nameof(ErrDisp))]
    [InlineData(typeof(ErrADisp0), Label = nameof(ErrADisp0))]
    [InlineData(typeof(ErrADisp1), Label = nameof(ErrADisp1))]
    [InlineData(null             , Label = "Mixed")]
    public async Task Many_should_throw__AggregateException(Type? t)
    {
        if(t != null)
        {
            var d1 = (IDisp)Activator.CreateInstance(t)!;
            var d2 = (IDisp)Activator.CreateInstance(t)!;

            await TestManyError([d1, d2]);
        }
        else
            await TestManyError([new ErrDisp(), new ErrADisp0(), new ErrADisp1()]);
    }

    private static async Task TestManyError(List<IDisp> list)
    {
        var act = () => DisposeHelper.DisposeAsync(list).AsTask();
        var agg = await act.Should()
            .ThrowExactlyAsync<AggregateException>();

        agg.Which.Message.Should().StartWith("Multiple errors at .Dispose[Async]().");

        agg.Which.InnerExceptions.Should().HaveCount(list.Count);
        agg.Which.InnerExceptions.Should().AllSatisfy(x =>
            x.Should()
                .BeOfType<InvalidOperationException>()
                .Which.Message.Should().Be("test-err")
        );
    }
    #endregion

    #region Negative: Dispose & throw
    [Fact]
    public async Task Many_SYNC__with_single_error__should_be_disposed_and_throw_and_InvalidOperationException()
    {
        var input = new IDisp[] {new Disp(), new ErrDisp(), new Disp()};
        var disposables = input.Cast<IDisposable>().ToList();

        JToken.FromObject(input.Select(x => x.IsDisposed))
        .Should().BeEquivalentTo("""
        [
            false,
            false,
            false
        ]
        """);

        var act = () => DisposeHelper.Dispose(disposables);
        act.Should()
            .ThrowExactly<InvalidOperationException>()
            .WithMessage("test-err");
        
        JToken.FromObject(input.Select(x => x.IsDisposed))
        .Should().BeEquivalentTo("""
        [
            true,
            false,
            true
        ]
        """);
    }

    [Theory]
    [InlineData(typeof(ErrDisp)  , Label = nameof(ErrDisp))]
    [InlineData(typeof(ErrADisp0), Label = nameof(ErrADisp0))]
    [InlineData(typeof(ErrADisp1), Label = nameof(ErrADisp1))]
    //[InlineData(null             , Label = "Mixed")]
    public async Task Many_ASYNC__with_single_error__should_be_disposed_and_throw_and_InvalidOperationException(Type t)
    {
        var d = (IDisp)Activator.CreateInstance(t)!;
        List<IDisp> input = d switch
        {
           ErrDisp   => [new Disp()  , d, new Disp()  ],
           ErrADisp0 => [new ADisp0(), d, new ADisp0()],
           ErrADisp1 => [new ADisp1(), d, new ADisp1()],
           _ => throw new InvalidOperationException("match error"),
        };

        JToken.FromObject(input.Select(x => x.IsDisposed))
        .Should().BeEquivalentTo("""
        [
            false,
            false,
            false
        ]
        """);

        var act = () => DisposeHelper.DisposeAsync(input).AsTask();
        await act.Should()
            .ThrowExactlyAsync<InvalidOperationException>()
            .WithMessage("test-err");
        
        JToken.FromObject(input.Select(x => x.IsDisposed))
        .Should().BeEquivalentTo("""
        [
            true,
            false,
            true
        ]
        """);
    }
    #endregion
}

// for testing convenience
internal interface IDisp
{
    bool IsDisposed { get; }
}

// test IDisposable
internal sealed class Disp : IDisposable, IDisp
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
    }
}
internal sealed class ErrDisp : IDisposable, IDisp
{
    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        throw new InvalidOperationException("test-err");
    }
}

// test IAsyncDisposable without await
internal sealed class ADisp0 : IAsyncDisposable, IDisp
{
    public bool IsDisposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        IsDisposed = true;
        return ValueTask.CompletedTask;
    }
}
internal sealed class ErrADisp0 : IAsyncDisposable, IDisp
{
    public bool IsDisposed { get; private set; }

    public ValueTask DisposeAsync()
    {
        throw new InvalidOperationException("test-err");
    }
}

// test IAsyncDisposable with await
internal sealed class ADisp1 : IAsyncDisposable, IDisp
{
    public bool IsDisposed { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await Task.Delay(1);
        IsDisposed = true;
    }
}
internal sealed class ErrADisp1 : IAsyncDisposable, IDisp
{
    public bool IsDisposed { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await Task.Delay(1);
        throw new InvalidOperationException("test-err");
    }
}