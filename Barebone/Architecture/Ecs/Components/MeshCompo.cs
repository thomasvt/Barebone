using Barebone.Graphics;
using Barebone.Pools;

namespace Barebone.Architecture.Ecs.Components
{
    public struct MeshCompo
    {
        public Mesh? Mesh = null;
        public bool IsMeshOwner = true;

        public MeshCompo()
        {
        }

        public static void Register(in EcsScene scene)
        {
            scene.RegisterComponent<MeshCompo>();
            scene.SubscribeOnRemove((in EntityId _, ref MeshCompo m) => OnRemove(in m));
        }

        private static void OnRemove(in MeshCompo compo)
        {
            if (compo is { Mesh: not null, IsMeshOwner: true }) Pool.Return(compo.Mesh);
        }
    }
}
