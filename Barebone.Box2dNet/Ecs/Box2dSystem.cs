using Barebone.Architecture.Ecs;
using Barebone.Architecture.Ecs.Components;
using Barebone.Geometry;
using Box2dNet.Interop;

namespace Barebone.Box2d.Ecs
{
    /// <summary>
    /// Drives physical ECS components with a single Box2D world behind the scenes. Uses TransformCompo for position, so your game must register it.
    /// </summary>
    public class Box2dSystem : IDisposable
    {
        private readonly EcsScene _scene;
        private readonly int _subStepCount;
        private readonly b2WorldDef? _worldDef;

        public b2WorldId B2WorldId { get; private set; }

        /// <param name="registerPosition2Compo">Pass in false if you register TransformCompo in the EcsScene yourself, else let Box2dSystem do it.</param>
        public Box2dSystem(EcsScene scene, bool registerPosition2Compo, int subStepCount, b2WorldDef? worldDef = null)
        {
            _scene = scene;
            _subStepCount = subStepCount;
            _worldDef = worldDef;
            B2WorldId = B2Api.b2CreateWorld(worldDef ?? B2Api.b2DefaultWorldDef());
            
            if (registerPosition2Compo)
                scene.RegisterComponent<TransformCompo>();

            RigidBodyCompo.Register(scene, () => B2WorldId);
            PolygonShapeCompo.Register(scene);
            CircleShapeCompo.Register(scene);
        }

        public void Execute(float deltaT)
        {
            B2Api.b2World_Step(B2WorldId, deltaT, _subStepCount);
            ReadBodyStates();
        }

        private void ReadBodyStates()
        {
            foreach (var moveEvent in B2Api.b2World_GetBodyEvents(B2WorldId).moveEventsAsSpan)
            {
                var entityId = new EntityId(moveEvent.userData);
                ref var t = ref _scene.GetComponentRef<TransformCompo>(entityId);
                t.Position = moveEvent.transform.p;
                t.Angle = moveEvent.transform.q.GetAngle();
                var maxSpeed = _scene.GetComponentRef<RigidBodyCompo>(entityId).MaxSpeed;
                if (maxSpeed.HasValue)
                {
                    var velocity = B2Api.b2Body_GetLinearVelocity(moveEvent.bodyId);
                    velocity = velocity.CapVectorLength(maxSpeed.Value);
                    B2Api.b2Body_SetLinearVelocity(moveEvent.bodyId, velocity);
                }

            }
        }

        public void Clear()
        {
            B2Api.b2DestroyWorld(B2WorldId);
            B2WorldId = B2Api.b2CreateWorld(_worldDef ?? B2Api.b2DefaultWorldDef());
        }

        public void Dispose()
        {
            B2Api.b2DestroyWorld(B2WorldId);
        }
    }
}
