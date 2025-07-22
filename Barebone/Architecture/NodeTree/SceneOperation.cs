namespace Barebone.Architecture.NodeTree
{
    internal enum OperationType
    {
        Add,
        Remove,
        Move
    }

    internal readonly record struct SceneOperation(OperationType Type, Node Node, Node? Parent);
}
