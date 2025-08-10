using System.Numerics;
using Barebone.Architecture.Ecs;
using Box2dNet.Interop;

namespace Barebone.Box2d.Ecs
{
    public struct RigidBodyCompo
    {
        internal b2BodyId B2BodyId { get; set; }

        public float MaxSpeed;
        public Vector2 Velocity;
        public Vector2 Gravity;
        public float GravityFactor;

        public bool IsHitOnTop;
        public bool IsHitOnBottom;
        public bool IsHitOnLeft;
        public bool IsHitOnRight;
        public float LatestHitOnBottom;

        public bool IsKinematic;

        public static void OnAdd(in EcsScene scene, in b2WorldId worldId, EntityId id, ref RigidBodyCompo b)
        {
            var bodyDef = B2Api.b2DefaultBodyDef();
            bodyDef.position = scene.GetComponentRef<PositionCompo>(id).Position;
            bodyDef.type = b.IsKinematic ? b2BodyType.b2_kinematicBody : b2BodyType.b2_staticBody;
            b.B2BodyId = B2Api.b2CreateBody(worldId, bodyDef);
        }

        public static void OnRemove(ref RigidBodyCompo b)
        {
            B2Api.b2DestroyBody(b.B2BodyId);
        }
    }
}
