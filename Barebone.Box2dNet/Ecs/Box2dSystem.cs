using Barebone.Architecture.Ecs;
using Barebone.Architecture.Ecs.Components;
using Barebone.Geometry;
using Box2dNet.Interop;

namespace Barebone.Box2d.Ecs
{
    /// <summary>
    /// Drives physical ECS components with a single Box2D world behind the scenes. Uses Position2Compo for position, so your game must register it.
    /// </summary>
    public class Box2dSystem : IDisposable
    {
        private readonly EcsScene _scene;
        private readonly int _subStepCount;
        private readonly b2WorldId _b2WorldId;

        /// <param name="registerPosition2Compo">Pass in false if you register Position2Compo in the EcsScene yourself, else let Box2dSystem do it.</param>
        public Box2dSystem(EcsScene scene, bool registerPosition2Compo, int subStepCount, b2WorldDef? worldDef = null)
        {
            _scene = scene;
            _subStepCount = subStepCount;
            _b2WorldId = B2Api.b2CreateWorld(worldDef ?? B2Api.b2DefaultWorldDef());
            
            if (registerPosition2Compo)
                scene.RegisterComponent<Position2Compo>();

            RigidBodyCompo.Register(scene, _b2WorldId);
            PolygonShapeCompo.Register(scene);
            CircleShapeCompo.Register(scene);
        }

        public void Execute(float deltaT)
        {
            B2Api.b2World_Step(_b2WorldId, deltaT, _subStepCount);
            ReadBodyStates();
        }

        private void ReadBodyStates()
        {
            foreach (var moveEvent in B2Api.b2World_GetBodyEvents(_b2WorldId).moveEventsAsSpan)
            {
                var entityId = new EntityId(moveEvent.userData);
                _scene.GetComponentRef<Position2Compo>(entityId).Position = moveEvent.transform.p;
                var maxSpeed = _scene.GetComponentRef<RigidBodyCompo>(entityId).MaxSpeed;
                if (maxSpeed.HasValue)
                {
                    var velocity = B2Api.b2Body_GetLinearVelocity(moveEvent.bodyId);
                    velocity = velocity.CapVectorLength(maxSpeed.Value);
                    B2Api.b2Body_SetLinearVelocity(moveEvent.bodyId, velocity);
                }

            }
        }

        public void Dispose()
        {
            B2Api.b2DestroyWorld(_b2WorldId);
        }
    }
}
