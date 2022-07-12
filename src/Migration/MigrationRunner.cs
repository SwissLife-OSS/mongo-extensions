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
    private readonly int _currentVersionOfApplication;

    public MigrationRunner(EntityContext context)
    {
        _currentVersionOfApplication = context.Option.CurrentVersion;
        _migrationRegistry = context.Option.Migrations.ToDictionary(m => m.Version, m => m);
        _logger = context.LoggerFactory.CreateLogger<MigrationRunner<T>>();
    }

    public void Run(BsonDocument document)
    {
        var fromVersion = FindCurrentVersionOfDocument(document);

        MigrateUp(document, fromVersion, _currentVersionOfApplication);
        MigrateDown(document, fromVersion, _currentVersionOfApplication);
    }

    private int FindCurrentVersionOfDocument(BsonDocument document)
    {
        // Document is from before Migrations have been introduced
        if (!(document.Contains(Version) && document[Version].IsInt32))
        {
            return _migrationRegistry.Keys.First() - 1;
        }

        var fromVersion = document[Version].AsInt32;
        if (_migrationRegistry.ContainsKey(fromVersion))
        {
            return fromVersion;
        }

        // Document is newer than any migration we know
        if (fromVersion > _migrationRegistry.Keys.Last())
        {
            return _migrationRegistry.Keys.Last();
        }

        // Document is older than any migration we know
        return _migrationRegistry.Keys.First() - 1;

    }

    private void MigrateUp(
        BsonDocument document,
        int fromVersion,
        int toVersion)
    {
        for (var version = fromVersion + 1; version <= toVersion; version++)
        {
            try
            {
                IMigration migration = _migrationRegistry[version];
                migration.Up(document);
                document.Set(Version, version);
                _logger.LogInformation(
                    "Successfully Migrated {entity} with id {id} to version {version}",
                    typeof(T).Name,
                    GetIdOrEmpty(document),
                    version);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Migration of {entity} with id {id} to version {version} failed",
                    typeof(T).Name,
                    GetIdOrEmpty(document),
                    version);
                break;
            }
        }
    }

    private void MigrateDown(
        BsonDocument document,
        int fromVersion,
        int toVersion)
    {
        for (var version = fromVersion; version > toVersion; version--)
        {
            try
            {
                IMigration migration = _migrationRegistry[version];
                migration.Down(document);
                document.Set(Version, version);
                _logger.LogInformation(
                    "Successfully Migrated {entity} with id {id} to version {version}",
                    typeof(T).Name,
                    GetIdOrEmpty(document),
                    version);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Migration of {entity} with id {id} to version {version} failed",
                    typeof(T).Name,
                    GetIdOrEmpty(document),
                    version);
                break;
            }
        }
    }

    private static string GetIdOrEmpty(BsonDocument document)
    {
        return document.Contains(Id) ? document[Id].ToString() ?? string.Empty : string.Empty;
    }
}
