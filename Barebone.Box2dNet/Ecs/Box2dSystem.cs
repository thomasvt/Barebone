using System.Numerics;
using Barebone.Architecture.Ecs;
using Box2dNet.Interop;

namespace Barebone.Box2d.Ecs
{
    public class Box2dSystem : IEcsSystem
    {
        private readonly b2WorldId _b2WorldId;

        public Box2dSystem(EcsScene scene)
        {
            var worldDef = B2Api.b2DefaultWorldDef(); 
            _b2WorldId = B2Api.b2CreateWorld(worldDef);

            scene.RegisterComponent<PositionCompo>();

            scene.RegisterComponent<RigidBodyCompo>();
            scene.SubscribeOnAdd((in EntityId id, ref RigidBodyCompo b) => RigidBodyCompo.OnAdd(scene, _b2WorldId, id, ref b));
            scene.SubscribeOnRemove((in EntityId id, ref RigidBodyCompo b) => RigidBodyCompo.OnRemove(ref b));

            scene.RegisterComponent<ShapeCompo>();
            scene.SubscribeOnAdd((in EntityId id, ref ShapeCompo s) => ShapeCompo.OnAdd(scene, id, ref s));
            scene.SubscribeOnRemove((in EntityId id, ref ShapeCompo s) => ShapeCompo.OnRemove(ref s));
        }

        public void Execute()
        {
            
        }
    }
}
