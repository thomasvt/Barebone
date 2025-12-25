using Barebone.Pools;

namespace Barebone.Graphics.Manifold.Core
{
    public abstract class Node : Poolable
    {
        protected List<IParameter> Parameters = new();
        private Geometry? _output;

        protected internal override void Construct()
        {
            ResetToDefaults();
        }

        protected internal override void Destruct()
        {
            _output?.Return();
            _output = null;
        }

        public bool IsDirty
        {
            get
            {
                foreach (var p in Parameters)
                {
                    if (p.IsDirty) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Resets all parameters of this node to their default value.
        /// </summary>
        public void ResetToDefaults()
        {
            foreach (var p in Parameters)
            {
                p.ResetToDefault();
            }
        }

        protected Parameter<T> NewParameter<T>(T defaultValue)
        {
            var p = new Parameter<T>(defaultValue);
            Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Cooks the node and returns the resulting geometry. Reuses earlier results if no inputs have changed.
        /// </summary>
        public Geometry GetResult()
        {
            if (_output != null && !IsDirty)
                return _output;

            if (_output == null)
            {
                _output = Geometry.RentNew();
            }
            else
            {
                // we reuse already allocated capacity, but clear out the contents
                _output.Clear();
            }

            Cook(_output);
            ResetIsDirty();
            return _output;
        }

        private void ResetIsDirty()
        {
            foreach (var parameter in Parameters)
            {
                parameter.ResetIsDirty();
            }
        }

        public abstract void Cook(in Geometry output);
    }
}
