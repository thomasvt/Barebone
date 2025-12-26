using Barebone.Graphics.NodeArt.Core;

namespace Barebone.Graphics.NodeArt
{
    public class PointWrangleNode : ArtNode
    {
        public delegate void PointWrangleDelegate(ref Point point);

        public ArtParameter<PointWrangleDelegate> Delegate { get; set; }
        public ArtParameter<ArtNode> Input { get; set; }

        public PointWrangleNode()
        {
            Delegate = DefineParameter<PointWrangleDelegate>();
            Input = DefineParameter<ArtNode>();
        }

        protected override void Cook(in ArtGeometry output)
        {
            if (Input.Value == null) throw new Exception("Node has no Input.");

            var input = Input.Value.GetResult();
            var wrangle = Delegate.Value;

            input.CloneTo(output);

            if (wrangle == null)
                return;

            foreach (ref var p in output.PointSet.Points.AsSpan())
                wrangle(ref p);
        }
    }
}
