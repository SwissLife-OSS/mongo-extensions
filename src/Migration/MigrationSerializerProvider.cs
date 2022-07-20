using System;
using System.Linq;
using MongoDB.Bson.Serialization;

namespace MongoDB.Extensions.Migration;

public class MigrationSerializerProvider : IBsonSerializationProvider
{
    private readonly MigrationContext _context;

    public MigrationSerializerProvider(MigrationContext context)
    {
        _context = context;
    }

    public IBsonSerializer? GetSerializer(Type type)
    {
        EntityOption? option = _context.Option.EntityOptions.SingleOrDefault(e => e.Type == type);
        if (option is null)
        {
            return null;
        }

        EntityContext entityContext = new(option, _context.LoggerFactory);
        Type migrationSerializerDefinition = typeof(MigrationSerializer<>);
        Type migrationSerializerType = migrationSerializerDefinition.MakeGenericType(type);
        return (IBsonSerializer?)Activator.CreateInstance(migrationSerializerType, entityContext);
    }
}
