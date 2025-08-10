using Barebone.Architecture.Ecs;
using Barebone.Architecture.Ecs.Components;
using Box2dNet.Interop;

namespace Barebone.Box2d.Ecs
{
    public enum BodyType
    {
        Static,
        Kinematic,
        Dynamic
    }

    /// <summary>
    /// Creates and destroys the body in Box2D, the rest is up to you by calling B2Api and use the B2BodyId.
    /// </summary>
    public struct RigidBodyCompo
    {
        public b2BodyId B2BodyId { get; set; }
        public BodyType BodyType { get; set; }
        public bool LockRotation { get; set; }

        public static void OnAdd(in EcsScene scene, in b2WorldId worldId, EntityId id, ref RigidBodyCompo b)
        {
            var bodyDef = B2Api.b2DefaultBodyDef();
            bodyDef.position = scene.GetComponentRef<Position2Compo>(id).Position;
            bodyDef.motionLocks.angularZ = b.LockRotation;

            bodyDef.type = b.BodyType switch { BodyType.Static => b2BodyType.b2_staticBody, BodyType.Kinematic => b2BodyType.b2_kinematicBody, BodyType.Dynamic => b2BodyType.b2_dynamicBody,
                _ => throw new NotSupportedException($"Unknown BodyType: '{b.BodyType}'")
            };
            bodyDef.userData = (nint)id.Value;
            b.B2BodyId = B2Api.b2CreateBody(worldId, bodyDef);
        }

        public static void OnRemove(ref RigidBodyCompo b)
        {
            B2Api.b2DestroyBody(b.B2BodyId);
        }

        public static void Register(EcsScene scene, b2WorldId worldId)
        {
            scene.RegisterComponent<RigidBodyCompo>();
            scene.SubscribeOnAdd((in EntityId id, ref RigidBodyCompo b) => OnAdd(scene, worldId, id, ref b));
            scene.SubscribeOnRemove((in EntityId id, ref RigidBodyCompo b) => OnRemove(ref b));
        }
    }
}
