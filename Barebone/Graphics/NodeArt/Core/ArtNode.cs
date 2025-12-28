using Barebone.Pools;

namespace Barebone.Graphics.NodeArt.Core
{
    public abstract class ArtNode : Poolable
    {
        protected List<INodeParameter> Parameters = new();
        public GeometrySet? Output = null!;

        protected internal override void Construct()
        {
            ResetToDefaults(); // we don't delete parameters because they are permanently defined in ctor: they persist between rentals.
        }

        protected internal override void Destruct()
        {
        }

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

        protected NodeParameter<T> DefineParameter<T>(string name, T? defaultValue = default)
        {
            var p = new NodeParameter<T>(name, defaultValue);
            Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Cooks the node and returns the resulting geometry. Reuses earlier results if no inputs have changed.
        /// </summary>
        public virtual GeometrySet Cook()
        {
            Output?.Return();
            Output = GeometrySet.RentNew();

            CookInternal(Output);
            return Output;
        }

        protected abstract void CookInternal(in GeometrySet output);
    }
}
