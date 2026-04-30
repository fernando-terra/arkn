using Arkn.Core.Primitives;

namespace Arkn.Core.Tests.Primitives;

public class ValueObjectTests
{
    private sealed class Money : ValueObject
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void ValueObjects_WithSameComponents_ShouldBeEqual()
    {
        var a = new Money(10m, "BRL");
        var b = new Money(10m, "BRL");
        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void ValueObjects_WithDifferentComponents_ShouldNotBeEqual()
    {
        var a = new Money(10m, "BRL");
        var b = new Money(10m, "USD");
        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }

    [Fact]
    public void ValueObjects_WithSameComponents_ShouldHaveSameHashCode()
    {
        var a = new Money(10m, "BRL");
        var b = new Money(10m, "BRL");
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
