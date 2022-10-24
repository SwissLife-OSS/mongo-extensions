using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;

namespace MongoDB.Prime.Extensions
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

        public static ProfilingStatus GetProfilingStatus(
            this IMongoDatabase mongoDatabase)
        {
            var profileStatusCommand = new BsonDocument("profile", -1);

            BsonDocument profileBsonDocument =
                mongoDatabase.RunCommand<BsonDocument>(profileStatusCommand);

            return CreateProfilingStatus(profileBsonDocument);
        }

        private static ProfilingStatus CreateProfilingStatus(
            BsonDocument profileBsonDocument)
        {
            return new ProfilingStatus(
                level: (ProfileLevel)profileBsonDocument["was"].AsInt32,
                slowMs: profileBsonDocument["slowms"].AsInt32,
                sampleRate: profileBsonDocument["sampleRate"].AsDouble,
                filter: profileBsonDocument["ok"].AsDouble.ToString());
        }

        public static IEnumerable<string> GetProfiledOperations(
            this IMongoDatabase mongoDatabase)
        {
            IMongoCollection<BsonDocument> collection = mongoDatabase
                .GetCollection<BsonDocument>("system.profile");

            List<BsonDocument> docs = collection
                .Find(new BsonDocument())
                .ToList();

            IEnumerable<string> jsons = docs.Select(bson => bson.ToJson(
                new JsonWriterSettings
                {
                    OutputMode = JsonOutputMode.RelaxedExtendedJson
                }));

            IEnumerable<string> normalizedJson = jsons
                .Select(json => JsonSerializer
                    .Serialize(
                        JsonDocument.Parse(json).RootElement,
                        new JsonSerializerOptions()
                        {
                            WriteIndented = true
                        }));

            return normalizedJson;
        }

        public static IMongoCollection<TDocument> GetCollection<TDocument>(
            this IMongoDatabase mongoDatabase,
            MongoCollectionSettings? settings = null)
        {
            return mongoDatabase
                .GetCollection<TDocument>(typeof(TDocument).Name, settings);
        }
    }
}
