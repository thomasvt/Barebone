namespace Barebone.AI.BehaviorTree;

internal abstract class CompositeNode : BehaviorNode
{
    public readonly BehaviorNode[] Children;

    protected CompositeNode(BehaviorNode[] children)
    {
        Children = children;
    }

    protected override void Reset()
    {
        foreach (var child in Children)
        {
            child.DoReset();
        }
    }

    protected override IEnumerable<BehaviorNode> GetChildren()
    {
        return Children;
    }
}