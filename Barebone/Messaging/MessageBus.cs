namespace Barebone.Messaging
{
    public class MessageBus : IMessageBus
    {
        public delegate void Handler<TMessage>(in TMessage message) where TMessage : struct;

        private readonly Dictionary<Type, object> _handlers = new();

        public void Subscribe<TMessage>(Handler<TMessage> handler) where TMessage : struct
        {
            var messageType = typeof(TMessage);

            if (!_handlers.TryGetValue(messageType, out var handlerList))
            {
                _handlers.Add(messageType, handlerList = new List<Handler<TMessage>>());
            }
            ((List<Handler<TMessage>>)handlerList).Add(handler);
        }

        public void Publish<TMessage>(in TMessage message) where TMessage : struct
        {
            var messageType = typeof(TMessage);

            if (_handlers.TryGetValue(messageType, out var handlerList))
            {
                var handlers = (List<Handler<TMessage>>)handlerList;
                foreach (var handler in handlers)
                    handler.Invoke(in message);
            }
        }
    }
}
