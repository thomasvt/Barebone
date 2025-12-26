using Barebone.Pools;

namespace Barebone.Graphics.NodeArt.Core
{
    public abstract class ArtNode : Poolable
    {
        protected List<INodeParameter> Parameters = new();
        private GeometrySet? _output;

        protected internal override void Construct()
        {
            ResetToDefaults();
        }

        protected internal override void Destruct()
        {
            _output?.Return();
            _output = null;
        }

        public bool IsDirty { get; private set; }

        /// <summary>
        /// Resets all parameters of this node to their default value.
        /// </summary>
        internal void ResetToDefaults()
        {
            foreach (var p in Parameters)
            {
                p.ResetToDefault();
            }
        }

        /// <summary>
        /// You must call this for all external parameters that should invalidate the previous cooking output when their value changes.
        /// </summary>
        public void DependsOn(INodeParameter parameter)
        {
            parameter.ValueChange += () => IsDirty = true;
        }

        protected NodeParameter<T> DefineParameter<T>(string name, T? defaultValue = default)
        {
            var p = new NodeParameter<T>(name, defaultValue);
            p.ValueChange += () => IsDirty = true;
            Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Cooks the node and returns the resulting geometry. Reuses earlier results if no inputs have changed.
        /// </summary>
        public virtual GeometrySet GetResult()
        {
            if (_output != null && !IsDirty)
                return _output;

            if (_output == null)
            {
                _output = GeometrySet.RentNew();
            }
            else
            {
                // we reuse already allocated capacity, but clear out the contents
                _output.SetItemCountsToZero();
            }

            Cook(_output);
            IsDirty = false;
            return _output;
        }

        protected abstract void Cook(in GeometrySet output);
    }
}
