using Arkn.Core.Primitives;
using Arkn.Results;

namespace ArknLib.Domain;

/// <summary>
/// Starter entity — rename and implement your domain logic.
/// Domain methods should return Result or Result&lt;T&gt; instead of throwing.
/// </summary>
public sealed class SampleEntity : AggregateRoot
{
    public string Name { get; private set; }

    private SampleEntity(string name) => Name = name;

    public static Result<SampleEntity> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("SampleEntity.NameRequired", "Name is required.");

        return new SampleEntity(name);
    }

    public Result Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure(Error.Validation("SampleEntity.NameRequired", "Name is required."));

        Name = newName;
        MarkUpdated();
        return Result.Success();
    }
}
