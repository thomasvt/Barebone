namespace Barebone.Messaging
{
    public interface IMessageBus
    {
        void Publish<TMessage>(in TMessage message) where TMessage : struct;
        void Subscribe<TMessage>(MessageBus.Handler<TMessage> handler) where TMessage : struct;
    }
}
