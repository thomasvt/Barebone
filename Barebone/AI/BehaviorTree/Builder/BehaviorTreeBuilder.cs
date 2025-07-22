namespace Barebone.AI.BehaviorTree.Builder;

public class BehaviorTreeBuilder : BehaviorBranchBuilder
{
    public override BehaviorTree Build()
    {
        var rootNode = new SequenceNode(GetNodes());
        rootNode.DoReset();
        return new BehaviorTree(rootNode);
    }
}