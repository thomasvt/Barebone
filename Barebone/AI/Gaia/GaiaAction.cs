namespace Barebone.AI.Gaia
{
    public abstract class GaiaAction(bool interruptable)
    {
        protected internal virtual void OnEnter() { }
        protected internal virtual void Update() { }
        protected internal virtual void OnLeave() { }
        protected internal virtual bool IsDone => true;
        public bool Interruptable { get; } = interruptable;

        public static GaiaAction Noop = new NoopAction();
    }
}
