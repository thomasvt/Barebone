namespace Barebone
{
    public class MessageBus
    {
        public delegate void Handler<TMessage>(in TMessage message) where TMessage : struct;

        private readonly Dictionary<Type, List<object>> _handlersPerMessageType = new();

        public void Subscribe<TMessage>(Handler<TMessage> handler) where TMessage : struct
        {
            var messageType = typeof(TMessage);
            if (!_handlersPerMessageType.TryGetValue(messageType, out var handlers))
            {
                _handlersPerMessageType.Add(messageType, handlers = new List<object>());
            }
            handlers.Add(handler);
        }

        public void Publish<TMessage>(in TMessage message) where TMessage : struct
        {
            var messageType = typeof(TMessage);
            if (_handlersPerMessageType.TryGetValue(messageType, out var handlers))
            {
                foreach (Handler<TMessage> handler in handlers)
                    handler.Invoke(in message);
            }
        }
    }
}
