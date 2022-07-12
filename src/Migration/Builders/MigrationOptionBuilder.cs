using System;
using System.Collections.Generic;

namespace MongoDB.Extensions.Migration;

public class MigrationOptionBuilder
{
    private readonly List<EntityOption> _entityOptions = new();

    /// <summary>
    /// Builds the MigrationOption
    /// </summary>
    public MigrationOption Build()
    {
        return new MigrationOption(_entityOptions);
    }

    /// <summary>
    /// Register a migration for a given entity.
    /// </summary>
    public MigrationOptionBuilder ForEntity<T>(
        Func<EntityOptionBuilder<T>, EntityOptionBuilder<T>> builderAction) where T : IVersioned
    {
        _entityOptions.Add(builderAction(new EntityOptionBuilder<T>()).Build());
        return this;
    }
}
