using Arkn.MCP.Tools;
using Xunit;

namespace Arkn.MCP.Tests;

public class ScaffoldDomainEntityTests
{
    [Fact]
    public void ScaffoldDomainEntity_ShouldContainEntityName()
    {
        var result = DomainTools.ScaffoldDomainEntity("User");
        Assert.Contains("class User", result);
    }

    [Fact]
    public void ScaffoldDomainEntity_Aggregate_ShouldInheritAggregateRoot()
    {
        var result = DomainTools.ScaffoldDomainEntity("Order", isAggregate: true);
        Assert.Contains("AggregateRoot", result);
        Assert.Contains("CreatedEvent", result);
        Assert.Contains("Raise(", result);
    }

    [Fact]
    public void ScaffoldDomainEntity_Entity_ShouldInheritEntity()
    {
        var result = DomainTools.ScaffoldDomainEntity("Address", isAggregate: false);
        Assert.Contains(": Entity", result);
        Assert.DoesNotContain("AggregateRoot", result);
    }

    [Fact]
    public void ScaffoldDomainEntity_WithValueObjects_ShouldGenerateVoClasses()
    {
        var result = DomainTools.ScaffoldDomainEntity("User", "Email,PhoneNumber");
        Assert.Contains("class Email", result);
        Assert.Contains("class PhoneNumber", result);
        Assert.Contains("ValueObject", result);
    }

    [Fact]
    public void ScaffoldDomainEntity_ShouldReturnResultFactory()
    {
        var result = DomainTools.ScaffoldDomainEntity("Payment");
        Assert.Contains("Result<Payment>", result);
        Assert.Contains("static Result<Payment> Create(", result);
    }

    [Fact]
    public void ScaffoldDomainEntity_EmptyName_ShouldReturnError()
    {
        var result = DomainTools.ScaffoldDomainEntity("");
        Assert.StartsWith("Error:", result);
    }
}
