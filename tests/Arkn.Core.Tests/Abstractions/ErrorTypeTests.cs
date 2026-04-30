using Arkn.Core.Abstractions;

namespace Arkn.Core.Tests.Abstractions;

public class ErrorTypeTests
{
    [Fact]
    public void ErrorType_HasExpectedValues()
    {
        Assert.Equal(0, (int)ErrorType.Failure);
        Assert.Equal(1, (int)ErrorType.NotFound);
        Assert.Equal(2, (int)ErrorType.Validation);
        Assert.Equal(3, (int)ErrorType.Conflict);
        Assert.Equal(4, (int)ErrorType.Unauthorized);
    }

    [Fact]
    public void ErrorType_ValuesAreDistinct()
    {
        var values = Enum.GetValues<ErrorType>();
        Assert.Equal(values.Length, values.Distinct().Count());
    }
}
