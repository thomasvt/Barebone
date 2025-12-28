using System.Drawing;
using Barebone.Graphics.NodeArt.Core;

namespace Barebone.Graphics.NodeArt
{
    public class FillNode : ArtNode
    {
        public NodeParameter<ArtNode> Input { get; }
        public NodeParameter<Color> Color { get; }

        public FillNode()
        {
            Input = DefineParameter<ArtNode>(nameof(Input));
            Color = DefineParameter(nameof(Color), System.Drawing.Color.White);
        }

        protected override void CookInternal(in GeometrySet output)
        {
            Input.GetValueOrThrow().Cook().CloneInto(output);

            var colorArray = output.ShapeSet.GetOrCreateAttributeArray<Color>("color");
            foreach (var shape in output.ShapeSet.Items)
            {
                colorArray.Set(shape.Idx, Color.Value);
            }
        }
    }
}
