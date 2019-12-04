using System;
using Models;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Extensions.Context;

namespace DataAccess
{
    internal class UserCollectionConfiguration : IMongoCollectionConfiguration<User>
    {
        public void Configure(IMongoCollectionBuilder<User> mongoCollectionBuilder)
        {
            mongoCollectionBuilder
                .WithCollectionName("users")
                .AddBsonClassMap<User>(ConfigureUserClassMap())
                .WithMongoCollectionSettings(ConfigureCollectionSettings())
                .WithMongoCollectionConfiguration(ConfigureIndexes());
        }

        private static Action<MongoCollectionSettings> ConfigureCollectionSettings()
        {
            return setting =>
            {
                setting.WriteConcern = WriteConcern.WMajority.With(journal: true);
                setting.ReadConcern = ReadConcern.Majority;
                setting.ReadPreference = ReadPreference.Primary;
            };
        }

        private static Action<IMongoCollection<User>> ConfigureIndexes()
        {
            return collection =>
            {
                var emailIndex = new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(user => user.Email),
                    new CreateIndexOptions { Unique = true });

                var nicknameIndex = new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(user => user.Nickname),
                    new CreateIndexOptions { Unique = true });

                var firstname = new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(user => user.Firstname),
                    new CreateIndexOptions { Unique = false });

                var secondname = new CreateIndexModel<User>(
                    Builders<User>.IndexKeys.Ascending(user => user.Lastname),
                    new CreateIndexOptions { Unique = false });

                collection.Indexes.CreateMany(
                    new[] { emailIndex, nicknameIndex, firstname, secondname });
            };
        }

        private static Action<BsonClassMap<User>> ConfigureUserClassMap()
        {
            return cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
            };
        }
    }
}