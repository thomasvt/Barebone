using System.Numerics;
using Barebone.Graphics.NodeArt.Core;

namespace Barebone.Graphics.NodeArt
{
    public class ScaleNode : ArtNode
    {
        public NodeParameter<Vector2> Scale { get; }
        public NodeParameter<ArtNode> Input { get; }

        public ScaleNode()
        {
            Scale = DefineParameter("Scale", Vector2.One);
            Input = DefineParameter<ArtNode>("Input", null);
        }

        protected override void CookInternal(in GeometrySet output)
        {
            var scale = Scale.Value;
            var input = Input.GetValueOrThrow().Cook();

            input.CloneInto(output);
            var points = output.PointSet.Items;
            foreach (ref var p in points)
            {
                p.Position *= scale;
            }
        }
    }
}
