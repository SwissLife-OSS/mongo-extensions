using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public static class MongoDatabaseExtensions
    {
        public static void EnableProfiling(
            this IMongoDatabase mongoDatabase,
            ProfileLevel profileLevel = ProfileLevel.All)
        {
            var profileCommand = new BsonDocument("profile", (int)profileLevel);

            mongoDatabase.RunCommand<BsonDocument>(profileCommand);
        }

        public static void DisableProfiling(this IMongoDatabase mongoDatabase)
        {
            var profileCommand = new BsonDocument("profile", (int)ProfileLevel.Off);

            mongoDatabase.RunCommand<BsonDocument>(profileCommand);
        }

        public static ProfilingStatus GetProfilingStatus(this IMongoDatabase mongoDatabase)
        {
            var profileStatusCommand = new BsonDocument("profile", BsonValue.Create(null));

            BsonDocument profileBsonDocument =
                mongoDatabase.RunCommand<BsonDocument>(profileStatusCommand);

            return new ProfilingStatus(
                level: (ProfileLevel)profileBsonDocument["was"].AsInt32,
                slowMs: profileBsonDocument["slowms"].AsInt32,
                sampleRate: profileBsonDocument["sampleRate"].AsDouble,
                filter: profileBsonDocument["ok"].AsDouble.ToString());
        }

        public static IEnumerable<BsonDocument> GetProfileOutputs(
            this IMongoDatabase mongoDatabase)
        {
            IMongoCollection<BsonDocument> collection =
                GetProfileCollection(mongoDatabase);

            return collection.Find(new BsonDocument()).ToList();
        }

        public static BsonDocument GetLastProfileOutput(
            this IMongoDatabase mongoDatabase)
        {
            IMongoCollection<BsonDocument> collection =
                GetProfileCollection(mongoDatabase);

            return collection.Find(new BsonDocument()).Single();
        }

        public static IEnumerable<JsonDocument> GetProfileOutputsJson(
            this IMongoDatabase mongoDatabase)
        {
            IEnumerable<BsonDocument> profilingBsonDocuments =
                GetProfileOutputs(mongoDatabase);

            IEnumerable<JsonDocument> jsonDocuments = 
                profilingBsonDocuments.Select(bson => bson.ToJsonDocument());

            return jsonDocuments;
        }

        public static JsonDocument GetLastProfileOutputJson(
            this IMongoDatabase mongoDatabase)
        {
            BsonDocument lastProfileOutput =
                GetLastProfileOutput(mongoDatabase);

            return lastProfileOutput.ToJsonDocument();
        }

        public static IMongoCollection<BsonDocument> GetProfileCollection(
            IMongoDatabase mongoDatabase)
        {
            return mongoDatabase.GetCollection<BsonDocument>("system.profile");
        }
    }
}
