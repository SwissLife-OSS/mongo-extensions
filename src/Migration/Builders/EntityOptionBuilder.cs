using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Extensions.Migration;

public class EntityOptionBuilder<T> where T : IVersioned
{
    private int? _atVersion;
    private readonly List<IMigration> _migrations = new();

    /// <summary>
    /// Set the current Version of the data which is suitable for the application.
    /// If not set the newest Version is used.
    /// </summary>
    public EntityOptionBuilder<T> AtVersion(int atVersion)
    {
        _atVersion = atVersion;
        return this;
    }

    /// <summary>
    /// Register a migration for an entity. The versions of the migrations must start at 1 and be
    /// continuously incremented without a gap
    /// </summary>
    public EntityOptionBuilder<T> WithMigration(IMigration migration)
    {
        _migrations.Add(migration);
        return this;
    }

    public EntityOption Build()
    {
        if (_migrations.Count == 0)
        {
            throw new BuilderNotInitializedException(
                "Please register at least one migration using the WithMigration method");
        }

        _migrations.Sort((x, y) => x.Version.CompareTo(y.Version));

        _atVersion ??= _migrations.Last().Version;

        // TODO: use IComparibel for this
        if (_atVersion != 0 && _migrations.All(m => !Equals(m.Version, _atVersion)))
        {
            throw new InvalidConfigurationException(
                $"There is no migration for version {_atVersion} for entity {typeof(T).Name}");
        }

        for (var i = 1; i < _migrations.Count; i++)
        {
            if (_migrations[i - 1].Version + 1 != _migrations[i].Version)
            {
                // TODO: Compare with IComparibel instead
                throw new InvalidConfigurationException(
                    "The versions of the migrations must be continuously incremented");
            }

        }

        return new EntityOption(
            typeof(T),
            _atVersion.Value,
            _migrations);
    }
}
