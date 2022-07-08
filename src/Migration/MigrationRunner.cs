using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace MongoDB.Extensions.Migration;

public class MigrationRunner<T>
{
    private readonly ILogger<MigrationRunner<T>> _logger;
    private const string Version = "Version";
    private const string Id = "_id";
    private readonly Dictionary<int, IMigration> _migrationRegistry;
    private readonly int _currentVersion;

    public MigrationRunner(EntityContext context)
    {
        _currentVersion = context.Option.CurrentVersion;
        _migrationRegistry = context.Option.Migrations.ToDictionary(m => m.Version, m => m);
        _logger = context.LoggerFactory.CreateLogger<MigrationRunner<T>>();
    }

    public void Run(BsonDocument document)
    {
        // TODO: Change to IComparable
        // use BsonTypeMapper.MapToDotNetValue or BsonDocumentSerializer.Instance.Deserialize depending if it is a primitive type or not
        // unresolved issue: To what do we default if we do not have a version, maybe null?
        var fromVersion = document.Contains(Version) && document[Version].IsInt32 
            ? document[Version].AsInt32
            : 0;
        string id = document[Id].ToString() ?? "";

        MigrateUp(document, _currentVersion, fromVersion, id);
        MigrateDown(document, _currentVersion, fromVersion, id);
    }

    private void MigrateUp(
        BsonDocument document,
        int toVersion,
        int fromVersion,
        string id)
    {
        for (var version = fromVersion + 1; version <= toVersion; version++)
        {
            try
            {
                IMigration migration = _migrationRegistry[version];
                migration.Up(document);
                document.Set(Version, version);
                _logger.LogInformation(
                    "Successfully Migrated {entity} with id {id} to version {version} ",
                    typeof(T).Name,
                    id,
                    version);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Migration of {entity} with id {id} to version {version} failed",
                    typeof(T).Name,
                    id,
                    version);
                break;
            }
        }
    }

    private void MigrateDown(
        BsonDocument document,
        int toVersion,
        int fromVersion,
        string id)
    {
        for (var version = fromVersion; version > toVersion; version--)
        {
            try
            {
                IMigration migration = _migrationRegistry[version];
                migration.Down(document);
                document.Set(Version, version);
                _logger.LogInformation(
                    "Successfully Migrated {entity} with id {id} to version {version} ",
                    typeof(T).Name,
                    id,
                    version);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Migration of {entity} with id {id} to version {version} failed",
                    typeof(T).Name,
                    id,
                    version);
                break;
            }
        }
    }
}
