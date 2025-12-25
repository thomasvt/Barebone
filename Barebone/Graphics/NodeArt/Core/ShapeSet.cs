using Barebone.Pools;

namespace Barebone.Graphics.NodeArt.Core
{
    public sealed class ShapeSet : Poolable
    {
        public BBList<Shape> Shapes { get; private set; } = null!;

        public AttributeSet Attributes { get; private set; } = null!;

        protected internal override void Construct()
        {
            Shapes = Pool.Rent<BBList<Shape>>();
            Attributes = Pool.Rent<AttributeSet>();
        }

        protected internal override void Destruct()
        {
            Shapes.Return();
            Attributes.Return();
        }

        public void Clear()
        {
            Shapes.Clear();
            Attributes.Clear();
        }

        public void CloneTo(ShapeSet dest)
        {
            dest.Shapes.Clear();
            dest.Shapes.AddBBList(Shapes);
            Attributes.CloneTo(dest.Attributes);
        }

        public int AddShape(int pathIdx)
        {
            var idx = Shapes.Count;
            Shapes.Add(new(idx, pathIdx));
            return idx;
        }
    }
}
