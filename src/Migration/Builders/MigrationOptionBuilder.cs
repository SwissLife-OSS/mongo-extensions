using System;
using System.Collections.Generic;

namespace MongoDB.Extensions.Migration;

public class MigrationOptionBuilder
{
    private readonly List<EntityOption> _entityOptions = new();

    public MigrationOption Build()
    {
        return new MigrationOption(_entityOptions);
    }

    public MigrationOptionBuilder ForEntity<T>(
        Func<EntityOptionBuilder<T>, EntityOptionBuilder<T>> builderAction) where T : IVersioned
    {
        _entityOptions.Add(builderAction(new EntityOptionBuilder<T>()).Build());
        return this;
    }
}
