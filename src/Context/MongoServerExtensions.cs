using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Extensions.Context
{
    public static class MongoServerExtensions
    {
        public static void RefreshSessions(this IMongoClient mongoClient, params Guid[] sessionIds)
        {
            SessionId[] sessions = sessionIds.Select(x => new SessionId(x)).ToArray();
            var refreshSession = new RefreshSession(sessions);
            var command = new ObjectCommand<BsonDocument>(refreshSession);

            mongoClient.GetDatabase("admin").RunCommand(command);
        }

        public static void DisableTableScan(this IMongoClient mongoClient)
        {
            var command = new BsonDocument { { "setParameter", 1 }, { "notablescan", 1 } };

            mongoClient.GetDatabase("admin").RunCommand<BsonDocument>(command);
        }
        
        public static bool IsTableScanDisabled(this IMongoClient mongoClient)
        {
            var getNoTableScanCommand =
                new BsonDocument { { "getParameter", 1 }, { "notablescan", 1 } };

            BsonDocument parameterResult = mongoClient.GetDatabase("admin")
                .RunCommand<BsonDocument>(getNoTableScanCommand);

            BsonElement notablescanElement = parameterResult.GetElement("notablescan");
            
            if(bool.TryParse(notablescanElement.Value.ToString(), out bool isDisabled))
            {
                return isDisabled;
            }

            return false;
        }
    }
}
