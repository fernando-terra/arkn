using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Arkn.Core.Primitives;
using Arkn.Extensions.Repository.EntityFrameworkCore;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Arkn.Repository.Tests;

public class EfRepositoryTests
{
    private class TestEntity : AggregateRoot
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();
    }

    private class TestRepository : EfRepository<TestEntity, Guid>
    {
        public TestRepository(TestDbContext context) : base(context) { }
    }

    [Fact]
    public async Task ListAsync_WithPredicate_ShouldReturnFilteredResults()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var repository = new TestRepository(context);

        var entity1 = new TestEntity { Name = "Target", Value = 10 };
        var entity2 = new TestEntity { Name = "Other", Value = 20 };
        
        await repository.AddAsync(entity1);
        await repository.AddAsync(entity2);
        await context.SaveChangesAsync();

        // Act
        var results = await repository.ListAsync(e => e.Name == "Target");

        // Assert
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Target");
    }

    [Fact]
    public async Task AnyAsync_WithPredicate_ShouldReturnTrueIfMatchExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestDbContext(options);
        var repository = new TestRepository(context);

        var entity = new TestEntity { Name = "Found" };
        await repository.AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        var exists = await repository.AnyAsync(e => e.Name == "Found");

        // Assert
        exists.Should().BeTrue();
    }
}
