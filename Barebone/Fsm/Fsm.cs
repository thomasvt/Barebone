using Barebone.Pools;

namespace Barebone.Fsm
{
    public class Fsm<TAgent> : Poolable
    {
        private IFsmState<TAgent>? _currentState;
        private TAgent _agent = default!;
        public IFsmState<TAgent>? CurrentState => _currentState;
        public IFsmState<TAgent>? PreviousState { get; private set; }

        protected internal override void Construct()
        {
            _currentState = null;
        }

        public void Init(TAgent agent, IFsmState<TAgent>? initialState)
        {
            _currentState = null;
            _agent = agent;
            SwitchState(initialState);
        }

        protected internal override void Destruct()
        {
        }

        public void Update(TAgent agent)
        {
            // Console.WriteLine(_currentState?.GetType().Name ?? "<no-state>");
            var newState = _currentState?.Update(agent);
            PreviousState = _currentState;
            if (newState != _currentState) SwitchState(newState);
        }

        public void SwitchState(IFsmState<TAgent>? newState)
        {
            var previousState = _currentState;
            _currentState?.OnLeave(_agent, newState);
            _currentState = newState;
            newState?.OnEnter(_agent, previousState);
        }
    }
}
