namespace Barebone.Messaging
{
    public interface IMessagePublisher
    {
        void Publish<TMessage>(in TMessage message) where TMessage : struct;
    }
}
