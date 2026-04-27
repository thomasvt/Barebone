
namespace Barebone.Game
{
    /// <summary>
    /// A Component with children.
    /// </summary>
    public class Actor : Component
    {
        private readonly ComponentCollection _children;
        public IComponentCollection Children => _children;

        public Actor()
        {
            _children = new()
            {
                Parent = this
            };
        }

        public override void Draw()
        {
            _children.DrawAll();
            base.Draw();
        }

        public override void Update()
        {
            _children.UpdateAll();
            base.Update();
        }

        public override void Dispose()
        {
            _children.Dispose();
            base.Dispose();
        }
    }
}
