using Barebone.Graphics.NodeArt.Core;

namespace Barebone.Graphics.NodeArt
{
    public class PointWrangleNode : ArtNode
    {
        public delegate void PointWrangleDelegate(ref Point point);

        public NodeParameter<PointWrangleDelegate> Delegate { get; set; }
        public NodeParameter<ArtNode> Input { get; set; }

        public PointWrangleNode()
        {
            Delegate = DefineParameter<PointWrangleDelegate>(nameof(Delegate));
            Input = DefineParameter<ArtNode>(nameof(Input));
        }

        protected override void Cook(in GeometrySet output)
        {
            Input.GetValueOrThrow().GetResult().CloneInto(output);
            var wrangle = Delegate.GetValueOrThrow();

            foreach (ref var p in output.PointSet.Items)
                wrangle(ref p);
        }
    }
}
