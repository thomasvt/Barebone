
using System.Diagnostics.CodeAnalysis;

namespace Barebone.Game
{
    /// <summary>
    /// A Component with children.
    /// </summary>
    public class Actor : Component, IDisposable
    {
        private readonly ComponentCollection _children;
        public IComponentCollection Children => _children;

        public Actor()
        {
            _children = new(this);
        }

        public override void OnAdded()
        {
            foreach (var c in Children.AsSpan())
                c.OnAdded();
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

        public virtual void Dispose()
        {
            _children.Dispose();
        }

        public bool TryFindChild<T>([MaybeNullWhen(false)] out T child) where T : Component
        {
            child = FindChild<T>();
            return child != null;
        }

        public T? FindChild<T>() where T : Component
        {
            return Children.Find<T>();
        }

        public T FindChildOrThrow<T>() where T : Component
        {
            return Children.Find<T>() ?? throw new Exception($"No child of type '{typeof(T).Name}' found.");
        }
    }
}
