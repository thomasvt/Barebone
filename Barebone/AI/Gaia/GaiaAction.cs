namespace Barebone.AI.Gaia
{
    public abstract class GaiaAction
    {
        protected internal virtual void OnEnter() { }
        protected internal virtual void Update() { }
        protected internal virtual void OnLeave() { }
        protected internal virtual bool IsDone => true;
    }
}
