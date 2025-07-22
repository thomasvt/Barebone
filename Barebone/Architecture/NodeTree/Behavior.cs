namespace Barebone.Architecture.NodeTree
{
    /// <summary>
    /// Abstract Node for creating a behavioral Node that targets a Subject node. By default, the Subject is the direct parent Node, but you can also set it to something else.
    /// </summary>
    public abstract class Behavior : Node
    {
        protected internal override void Destruct()
        {
            Subject = default;
            base.Destruct();
        }

        protected override void Activate()
        {
            base.Activate();
            Subject ??= Parent;
        }

        protected override void Deactivate()
        {
            Subject = null;
            base.Deactivate();
        }

        /// <summary>
        /// The Node that the behavior is applied to.
        /// </summary>
        public Node? Subject { get; set; }
    }
}
