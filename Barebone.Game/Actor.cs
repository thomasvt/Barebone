
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

        public T AddChild<T>(T child) where T : Component
        {
            return Children.Add(child);
        }

        /// <summary>
        /// Gets the first child of given type. Returns whether such a child was found or not.
        /// </summary>
        public bool TryGetChild<T>([MaybeNullWhen(false)] out T child) where T : Component
        {
            child = GetChildOrNull<T>();
            return child != null;
        }

        /// <summary>
        /// Returns the first child of given type, or null if no such child exists.
        /// </summary>
        public T? GetChildOrNull<T>() where T : Component
        {
            return Children.GetFirst<T>();
        }

        /// <summary>
        /// Returns the first child of given type, or throws if no such child exists.
        /// </summary>
        public T GetChild<T>() where T : Component
        {
            return Children.GetFirst<T>() ?? throw new Exception($"No child of type '{typeof(T).Name}' found.");
        }
    }
}
