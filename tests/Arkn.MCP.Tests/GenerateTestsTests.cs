using Arkn.MCP.Tools;
using Xunit;

namespace Arkn.MCP.Tests;

public class GenerateTestsTests
{
    [Fact]
    public void GenerateTests_ShouldIncludeSuccessTest()
    {
        var sig    = "public async Task<Result<UserDto>> GetUserAsync(Guid id)";
        var result = TestGenTools.GenerateTests(sig, "UserService");

        Assert.Contains("ShouldReturnSuccess", result);
        Assert.Contains("Assert.True(result.IsSuccess)", result);
    }

    [Fact]
    public void GenerateTests_ShouldIncludeFailureTests()
    {
        var sig    = "public async Task<Result<UserDto>> GetUserAsync(Guid id)";
        var result = TestGenTools.GenerateTests(sig, "UserService", "User.NotFound,User.Invalid");

        Assert.Contains("NotFound_ShouldReturnFailure", result);
        Assert.Contains("Invalid_ShouldReturnFailure", result);
    }

    [Fact]
    public void GenerateTests_ShouldIncludeArrangeActAssert()
    {
        var sig    = "public Task<Result<OrderDto>> CreateOrderAsync(CreateOrderRequest request)";
        var result = TestGenTools.GenerateTests(sig, "OrderService", "Order.Conflict");

        Assert.Contains("// Arrange", result);
        Assert.Contains("// Act",     result);
        Assert.Contains("// Assert",  result);
    }

    [Fact]
    public void GenerateTests_ShouldIncludeXunitFact()
    {
        var sig    = "public Task<Result> DeleteUserAsync(Guid id)";
        var result = TestGenTools.GenerateTests(sig, "UserService");

        Assert.Contains("[Fact]", result);
    }

    [Fact]
    public void GenerateTests_ShouldIncludeErrorTypeAssertion()
    {
        var sig    = "public async Task<Result<InvoiceDto>> GetInvoiceAsync(Guid id)";
        var result = TestGenTools.GenerateTests(sig, "InvoiceService", "Invoice.NotFound");

        Assert.Contains("ErrorType.NotFound", result);
        Assert.Contains("result.FirstError.Code", result);
    }

    [Fact]
    public void GenerateTests_ShouldUseProvidedClassName()
    {
        var sig    = "public Task<Result<PaymentDto>> GetPaymentAsync(Guid id)";
        var result = TestGenTools.GenerateTests(sig, "PaymentService");

        Assert.Contains("IPaymentService", result);
    }

    [Fact]
    public void GenerateTests_EmptySignature_ShouldReturnError()
    {
        var result = TestGenTools.GenerateTests("");
        Assert.StartsWith("Error:", result);
    }

    [Fact]
    public void GenerateTests_VoidResult_ShouldNotAccessValue()
    {
        var sig    = "public Task<Result> DeleteAsync(Guid id)";
        var result = TestGenTools.GenerateTests(sig, "ProductService");

        // For Result (no value), should not assert .Value
        var successBlock = result.Split("// ── Failure")[0];
        Assert.DoesNotContain("result.Value", successBlock);
    }

    [Fact]
    public void GenerateTests_ShouldIncludeNSubstituteNote()
    {
        var sig    = "public Task<Result<UserDto>> GetAsync(Guid id)";
        var result = TestGenTools.GenerateTests(sig, "UserService");

        Assert.Contains("NSubstitute", result);
    }
}
