using System.Numerics;
using Barebone.Pools;

namespace Barebone.Graphics.NodeArt.Core
{
    public sealed class PointSet : Poolable
    {
        public BBList<Point> Points { get; private set; } = null!;

        public AttributeSet Attributes { get; private set; } = null!;

        protected internal override void Construct()
        {
            Points = Pool.Rent<BBList<Point>>();
            Attributes = Pool.Rent<AttributeSet>();
        }

        protected internal override void Destruct()
        {
            Points.Return();
            Attributes.Return();
        }

        public void Clear()
        {
            Points.Clear();
            Attributes.Clear();
        }

        public int AddPoint(Vector2 position)
        {
            var idx = Points.Count;
            Points.Add(new Point(idx, position));
            return idx;
        }

        public void CloneTo(PointSet dest)
        {
            dest.Points.Clear();
            dest.Points.AddBBList(Points);
            Attributes.CloneTo(dest.Attributes);
        }
    }

}
