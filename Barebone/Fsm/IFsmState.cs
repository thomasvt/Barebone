namespace Barebone.Fsm
{
    public interface IFsmState<TAgent>
    {
        void OnEnter(TAgent agent, IFsmState<TAgent>? previousState);
        IFsmState<TAgent> Update(TAgent agent);
        void OnLeave(TAgent agent, IFsmState<TAgent>? nextState);
    }
}
