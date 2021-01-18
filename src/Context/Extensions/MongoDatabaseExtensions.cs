﻿using System;
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
    }
}
