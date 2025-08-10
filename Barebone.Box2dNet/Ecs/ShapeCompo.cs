using Barebone.Architecture.Ecs;
using Barebone.Geometry;
using Box2dNet.Interop;

namespace Barebone.Box2d.Ecs
{
    public struct ShapeCompo
    {
        internal b2ShapeId B2ShapeId;

        public Aabb CapsuleAabb;
        public uint CategoryMask;
        public uint InteractionMask;
        public EntityId? RigidBodyId;

        public static void OnAdd(in EcsScene ecsScene, in EntityId id, ref ShapeCompo s)
        {
            var b2BodyId = ecsScene.GetComponentRef<RigidBodyCompo>(s.RigidBodyId ?? id);
            var def = B2Api.b2DefaultShapeDef();
            def.filter.categoryBits = s.CategoryMask;
            def.filter.maskBits = s.InteractionMask;
            var centerX = s.CapsuleAabb.Center.X;
            var radius = s.CapsuleAabb.Width * 0.5f;
            s.B2ShapeId = B2Api.b2CreateCapsuleShape(b2BodyId.B2BodyId, def, new b2Capsule(new(centerX, s.CapsuleAabb.Top - radius), new(centerX, s.CapsuleAabb.Bottom + radius), radius));
        }

        public static void OnRemove(ref ShapeCompo s)
        {
            B2Api.b2DestroyShape(s.B2ShapeId, true);
            s.B2ShapeId = default;
        }
    }
}
