using Arkn.Core.Primitives;

namespace Arkn.Core.Tests.Primitives;

public class EntityTests
{
    private sealed class FakeEntity : Entity
    {
        public FakeEntity() { }
        public FakeEntity(Guid id) { Id = id; }
    }

    [Fact]
    public void NewEntity_ShouldHaveNonEmptyId()
    {
        var entity = new FakeEntity();
        Assert.NotEqual(Guid.Empty, entity.Id);
    }

    [Fact]
    public void Entities_WithSameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var a = new FakeEntity(id);
        var b = new FakeEntity(id);
        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Entities_WithDifferentIds_ShouldNotBeEqual()
    {
        var a = new FakeEntity();
        var b = new FakeEntity();
        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }

    [Fact]
    public void NewEntity_UpdatedAt_ShouldBeNull()
    {
        var entity = new FakeEntity();
        Assert.Null(entity.UpdatedAt);
    }

    [Fact]
    public void MarkUpdated_ShouldSetUpdatedAt()
    {
        var entity = new FakeEntity();
        // Access via reflection to call protected method
        typeof(Entity)
            .GetMethod("MarkUpdated", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(entity, null);
        Assert.NotNull(entity.UpdatedAt);
    }
}
