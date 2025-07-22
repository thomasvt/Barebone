using System.Numerics;
using BareBone;
using Barebone.Pools;

namespace Barebone.Architecture.NodeTree
{
    /// <summary>
    /// A Node for building the game world as a tree of Nodes, called a <see cref="Scene"/>>. Supports 3D and 2D, hierarchic transforms, disabling node-branches, and offers overridable Activate, Execute and Deactivate methods. Fully supports the Barebone pooling system for minimal GC pressure.
    /// </summary>
    public partial class Node : Poolable
    {
#if DEBUG
        /// <summary>
        /// Logs lifecycle events of nodes satisfying LogFilter
        /// </summary>
        public static bool LogLifecycle = false;
        public static Func<Node, bool> LogFilter = n => true;
#endif

        // This partial contains the basic definitions of the Node: tree hierarchy, lifecycle hooks, pool support.

        private bool _isDisabled = false;
        private bool _isActive = false;
        private BBOwningList<Node>? _children;
        private Node? _parent;
        private NodeScene? _scene;
        private bool _isDisabledByParent;

        protected internal override void Construct()
        {
#if DEBUG
            if (LogLifecycle && LogFilter(this)) Console.WriteLine(this + " Rented");
#endif
            _isDisabledByParent = false;
            _isDisabled = false;
            _isActive = false;
            _children = Pool.Rent<BBOwningList<Node>>();
            _scene = default;
            _parent = default;
            _eulerAngles = Vector3.Zero;
            _parentTransform = Matrix4x4.Identity;
            _parentAngle = 0;
            _position3D = Vector3.Zero;
        }

        protected internal override void Destruct()
        {
#if DEBUG
            if (LogLifecycle && LogFilter(this)) Console.WriteLine(this + " Return");
#endif
            _isDisabledByParent = default;
            _isDisabled = default;
            _isActive = default;
            _scene = default;
            _parent = default;
            Pool.Return(_children!);
            _children = default;

            // destruct transform data:
            _worldToLocalDirty = default;
            _eulerAngles = default;
            _parentTransform = default;
            _parentAngle = default;
            _position3D = default;
            _localToWorld = default;
            _worldToLocal = default;
        }

        /// <summary>
        /// Clears and Destructs all child nodes.
        /// </summary>
        public virtual void Clear(bool freeCapacity = false)
        {
            // ensure deactivate is triggered where applicable:
            foreach (var child in Children)
            {
                child.Parent = null;
            }
            _children!.Clear(true, freeCapacity);
        }

        /// <summary>
        /// Creates and adds a child node of type T. If the parent node is already on a NodeScene, this operation is scheduled to happen after the current frame for consistency during the entire frame.
        /// </summary>
        public T AddNewChild<T>() where T : Node, new()
        {
            var child = Pool.Rent<T>();

            if (Scene == null)
                AddInternal(child);
            else
                Scene!.Schedule(OperationType.Add, child, this);
            return child;
        }

        /// <summary>
        /// Deactivates, removes and Destructs this node and all its children. If the node is on a NodeScene, this operation is scheduled to happen after the current frame for consistency during the entire frame, unless you force immediate destruction by passing immediate = true.
        /// </summary>
        public void Remove(bool immediate = false)
        {
            if (this == Scene) throw new Exception($"Cannot remove the NodeScene root node ({this}).");
            if (Parent == null) throw new Exception($"Cannot remove node '{this}' from its parent, because it has no parent.");
            if (Scene == null || immediate)
                Parent.RemoveInternal(this);
            else
                Scene!.Schedule(OperationType.Remove, this);
        }

        /// <summary>
        /// Looks upwards in the tree hierarchy for the first node that is of a certain type. The current node is also considered.
        /// </summary>
        public T? FindParent<T>(bool includeSelf) where T : class
        {
            var p = includeSelf ? this : Parent;
            while (p != null)
            {
                if (p is T t) return t;
                p = p.Parent;
            }
            return null;
        }

        /// <summary>
        /// Looks upwards in the tree hierarchy for the first node that is of a certain type. The current node is also considered. Throws if no such parent is found.
        /// </summary>
        public T FindParentOrThrow<T>(bool includeSelf) where T : class
        {
            var p = includeSelf ? this : Parent;
            while (p != null)
            {
                if (p is T t) return t;
                p = p.Parent;
            }
            throw new NodeNotFoundException($"Node '{this}' has no parent of type '{typeof(T).Name}'.");
        }

        /// <summary>
        /// Finds the first child node directly below the current one that is of a certain type.
        /// </summary>
        public T? FindChild<T>()
        {
            foreach (var child in Children)
            {
                if (child is T t) return t;
            }
            return default;
        }

        /// <summary>
        /// Finds the first child node directly below the current one that is of a certain type and matches the predicate.
        /// </summary>
        public T? FindChild<T>(Func<T, bool> predicate)
        {
            foreach (var child in Children)
            {
                if (child is T t && predicate(t)) return t;
            }
            return default;
        }


        /// <summary>
        /// Enumerates all direct children that are of type T and match the predicate.
        /// </summary>
        public IEnumerable<T> FindChildren<T>(Func<T, bool> predicate)
        {
            for (var i = 0; i < _children!.Count; i++)
            {
                var child = _children.InternalArray[i];
                if (child is T t && predicate(t)) yield return t;
            }
        }

        /// <summary>
        /// Enumerates all nodes below this node that are of type T and match the predicate.
        /// </summary>
        public IEnumerable<T> FindChildrenRecursive<T>(Func<T, bool> predicate)
        {
            for (var i = 0; i < _children!.Count; i++)
            {
                var child = _children.InternalArray[i];
                if (child is T t && predicate(t)) yield return t;
                foreach (var grandChild in child.FindChildrenRecursive<T>(predicate))
                {
                    yield return grandChild;
                }
            }
        }

        /// <summary>
        /// Finds the first child node directly below the current one that is of a certain type. Throws an exception is no such child is found.
        /// </summary>
        public T FindChildOrThrow<T>()
        {
            foreach (var child in Children)
            {
                if (child is T t) return t;
            }
            throw new NodeNotFoundException($"Node '{this}' has no child of type '{typeof(T).Name}'.");
        }

        private void DisableChildren()
        {
            foreach (var child in Children)
            {
                child._isDisabledByParent = true;

                if (child._isDisabled) continue; // don't propagate when the child is disabled itself, because then all children already are `disabledByParent`
                child.DisableChildren();
                child.TryToDeactivate();
            }
        }

        private void EnableChildren()
        {
            foreach (var child in Children)
            {
                child._isDisabledByParent = false;

                if (child._isDisabled) continue; // the child is disabled itself, so don't reenable its children.
                child.TryToActivate();
                child.EnableChildren();
            }
        }

        internal virtual void UpdateInternal()
        {
            if (!IsActive) return;

            Update();
            foreach (var child in _children!.AsSpan())
                child.UpdateInternal();
        }

        internal void AddInternal(Node child)
        {
#if DEBUG
            if (_children == null) throw new Exception($"Cannot add child '{child}' before Rented of parent '{this}' is called.");
#endif
            child.Parent = this;
            _children!.Add(child);
            if (_isDisabled || _isDisabledByParent)
                child.DisableChildren();
        }

        /// <summary>
        /// Removes and Destructs the child.
        /// </summary>
        internal void RemoveInternal(Node child)
        {
            child.Parent = null; // triggers Deactivate() on this child and its children.
            if (_isDisabled || _isDisabledByParent)
                child.EnableChildren(); // resets the `disabledByParent` flag
            _children!.SwapRemove(child);
        }

        /// <summary>
        /// Checks if the node has changed to active, and sets `IsActive` and calls `Activate()` if so. It does not check the opposite, you must call TryToDeactivate() also.
        /// These are split so we can have Activate() and Deactivate() happen at a different time in the parent algorithm.
        /// </summary>
        private void TryToActivate()
        {
            var isActive = _scene != null && !(_isDisabled || _isDisabledByParent);
            if (_isActive != isActive && isActive)
            {
                Activate();
                _isActive = true;
            }
        }

        /// <summary>
        /// Checks if the node has changed to inactive, sets `IsActive` and calls `Deactivate()` if so. It does not check the opposite, you must call TryToActivate() also.
        /// These are split so we can have Activate() and Deactivate() happen at a different time in the parent algorithm.
        /// </summary>
        private void TryToDeactivate()
        {
            var isActive = _scene != null && !(_isDisabled || _isDisabledByParent);
            if (_isActive != isActive && !isActive)
            {
                Deactivate();
                _isActive = false;
            }
        }

        public NodeScene? Scene
        {
            get => _scene;
            internal set
            {
                if (_scene == value) return;

                _scene = value;
                TryToActivate();

                foreach (var child in Children)
                {
                    child.Scene = value;
                }

                // Deactivate parent after children so children's Deactivate still can reach the `NodeScene` and `Parent` during their `Deactivate`.
                TryToDeactivate();
            }
        }

        public Node? Parent
        {
            get => _parent;
            private set
            {
                if (_parent == value) return;

                _parent = value;
                if (value == null)
                {
                    _parentTransform = Matrix4x4.Identity;
                    _parentAngle = 0f;
                }
                else
                {
                    _parentTransform = value.LocalToWorld;
                    _parentAngle = value.AngleWorld;
                }
                UpdateTransform();

                Scene = value?.Scene;
            }
        }

        public ReadOnlySpan<Node> Children => _children!.AsReadOnlySpan();

        protected virtual void Activate()
        {
#if DEBUG
            if (LogLifecycle && LogFilter(this)) Console.WriteLine(this + " Activated");
#endif
        }

        protected virtual void Update()
        {
#if DEBUG
            if (!IsActive) throw new Exception($"Cannot call Execute on Node '{this}' before it is Activated.");
#endif
        }

        protected virtual void Deactivate()
        {
#if DEBUG
            if (LogLifecycle && LogFilter(this)) Console.WriteLine(this + " Deactivated");
#endif
        }

        public bool IsOnScene => _scene != null;

        /// <summary>
        /// True as long as the Node is on a NodeScene and has no `IsDisabled` set on itself or any of its parents.
        /// </summary>
        public bool IsActive => _isActive;

        /// <summary>
        /// Deactivates this node and all of its children, which stops calling their Execute() and calls Deactivate() or Activate() on all nodes transitioning between active and inactive because of this.
        /// A node remains inactive as long as it or at least one of its parents has `IsDisabled` set.
        /// The value of the `IsDisabled` flag itself does not propagate to children: see `IsActive` to see if a node is currently active (getting Execute calls) or not.
        /// </summary>
        public bool IsDisabled
        {
            set
            {
                if (value == _isDisabled) return;

                _isDisabled = value;

                if (_isDisabledByParent) return; // this node and its children are still disabledByParent from a higher parent, don't Enable/Disable

                if (value)
                {
                    DisableChildren();
                    TryToDeactivate();
                }
                else
                {
                    TryToActivate();
                    EnableChildren();
                }
            }
            get => _isDisabled;
        }
    }
}
