namespace Barebone.Game
{
    /// <summary>
    /// A Component with children.
    /// </summary>
    public class Actor : Component
    {
        public readonly ComponentCollection Children = new();

        public override void Draw()
        {
            Children.DrawAll();
            base.Draw();
        }

        public override void Update()
        {
            Children.UpdateAll();
            base.Update();
        }

        public override void Dispose()
        {
            Children.Dispose();
            base.Dispose();
        }
    }
}
