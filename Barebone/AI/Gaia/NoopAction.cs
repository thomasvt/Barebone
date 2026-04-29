namespace Barebone.AI.Gaia
{
    public class NoopAction() : GaiaAction(true)
    {

        protected internal override bool IsDone => false;
    }
}
