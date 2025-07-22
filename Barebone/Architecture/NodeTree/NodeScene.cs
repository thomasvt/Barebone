namespace Barebone.Architecture.NodeTree
{
    /// <summary>
    /// The backbone of the game world. Includes a <see cref="Node"/> tree that represents all objects in the game.
    /// </summary>
    public sealed class NodeScene : Node
    {
        /// <summary>
        /// We use a normal Queue here because we need order to be maintained and this is a single instance for the entire game so virtually no GC pressure.
        /// </summary>
        private readonly Queue<SceneOperation> _deferredOperations = new();

        protected internal override void Destruct()
        {
            // first execute deferred operations to make the situation simpler to destroy.
            ExecuteDeferredOperations();
            base.Destruct();
        }

        public new void Update()
        {
            Scene = this;
            // modify nodetree outside of update cycle for consistency of hierarchy during that update cycle.
            ExecuteDeferredOperations();
            UpdateInternal();
        }

        internal void Schedule(OperationType operation, Node node, Node? parent = null)
        {
            _deferredOperations.Enqueue(new SceneOperation(operation, node, parent));
        }

        public override void Clear(bool freeCapacity)
        {
            ExecuteDeferredOperations();
            base.Clear(freeCapacity);
        }

        private void ExecuteDeferredOperations()
        {
            // loop over this with idx, because the operations may enqueue more operations that we also want to do immediately after (in eg. Node.Activate())
            while (_deferredOperations.Any())
            {
                var operation = _deferredOperations.Dequeue();
                switch (operation.Type)
                {
                    case OperationType.Add:
                    {
                        operation.Parent!.AddInternal(operation.Node); 
                        break;
                    }
                    case OperationType.Remove:
                    {
                        operation.Node.Parent!.RemoveInternal(operation.Node);
                        break;
                    }
                    case OperationType.Move: throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException("OperationType", $"Unknown OperationType: {operation.Type}");
                }
            }
        }
    }
}
