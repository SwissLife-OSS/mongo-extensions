namespace MongoDB.Extensions.Outbox.Core
{
    public interface IConsumptionFactory
    {
        IConsumption Create<TMessage>(TMessage message)
            where TMessage : class;
    }
}
