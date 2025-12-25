using Barebone.Graphics.NodeArt.Core;

namespace Barebone.Graphics.NodeArt
{
    public class PointWrangleNode : NaNode
    {
        public delegate void PointWrangleDelegate(ref Point point);

        public NaParameter<PointWrangleDelegate> Delegate { get; set; }
        public NaParameter<NaNode> Input { get; set; }

        public PointWrangleNode()
        {
            Delegate = DefineParameter<PointWrangleDelegate>();
            Input = DefineParameter<NaNode>();
        }

        protected override void Cook(in NaGeometry output)
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
