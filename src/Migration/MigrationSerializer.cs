using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Extensions.Migration;

class MigrationSerializer<T> : SerializerBase<T>, IBsonIdProvider, IBsonDocumentSerializer, IBsonPolymorphicSerializer,
    IHasDiscriminatorConvention where T : IVersioned
{
    private readonly EntityContext _context;
    private readonly MigrationRunner<T> _migrationRunner;
    private readonly BsonClassMapSerializer<T> _bsonClassMapSerializer;

    public MigrationSerializer(EntityContext context)
    {
        _bsonClassMapSerializer = new BsonClassMapSerializer<T>(BsonClassMap.LookupClassMap(typeof(T)));
        _context = context;
        _migrationRunner = new MigrationRunner<T>(context);
    }

    public override void Serialize(
        BsonSerializationContext context,
        BsonSerializationArgs args,
        T value)
    {
        value.Version = _context.Option.CurrentVersion;
        _bsonClassMapSerializer.Serialize(context, args, value);
    }

    public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        BsonDocument bsonDocument = BsonDocumentSerializer.Instance.Deserialize(context);

        _migrationRunner.Run(bsonDocument);

        var migratedContext =
            BsonDeserializationContext.CreateRoot(new BsonDocumentReader(bsonDocument));

        T entity = _bsonClassMapSerializer.Deserialize(migratedContext, args);

        entity.Version = _context.Option.CurrentVersion;

        return entity;
    }

    public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
    {
        return _bsonClassMapSerializer.TryGetMemberSerializationInfo(memberName, out serializationInfo);
    }

    public bool GetDocumentId(object document, out object id, out Type idNominalType, out IIdGenerator idGenerator)
    {
        return _bsonClassMapSerializer.GetDocumentId(document, out id, out idNominalType, out idGenerator);
    }

    public void SetDocumentId(object document, object id)
    {
        _bsonClassMapSerializer.SetDocumentId(document, id);
    }

    public bool IsDiscriminatorCompatibleWithObjectSerializer =>
        _bsonClassMapSerializer.IsDiscriminatorCompatibleWithObjectSerializer;

    public IDiscriminatorConvention DiscriminatorConvention => _bsonClassMapSerializer.DiscriminatorConvention;
}
