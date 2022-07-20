using System;
using System.Collections.Generic;
using System.Linq;

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
        if (_entityOptions.Any(e => e.Type == typeof(T)))
        {
            throw new InvalidConfigurationException(
                $"Migrations for entity of type {typeof(T).FullName} have already been registered");
        }
        _entityOptions.Add(builderAction(new EntityOptionBuilder<T>()).Build());
        return this;
    }
}
