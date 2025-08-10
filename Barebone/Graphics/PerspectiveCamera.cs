using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Graphics
{
    /// <summary>
    /// A perspective camera
    /// </summary>
    public class PerspectiveCamera : ICamera
    {
        private Vector3 _lookAt;
        private Vector3 _lookAtUp;

        /// <summary>
        /// Creates a default camera with Y+ up, Z- away from camera, X+ to the right, looking at (0,0,0) at a given `distance`.
        /// </summary>
        public PerspectiveCamera(float distance, float nearPlane, float farPlane)
        {
            Position = new(0, 0, distance);
            NearPlane = nearPlane;
            FarPlane = farPlane;
            LookAt(new(0, 0, 0), Vector3.UnitY);
        }

        /// <summary>
        /// Returns the pixel coords where a world location is projected onto the screen.
        /// </summary>
        public Vector2 WorldToScreen(Vector3 worldLocation, in Viewport viewport)
        {
            var postProjectiveLocation = Vector4.Transform(new Vector4(worldLocation.X, worldLocation.Y, worldLocation.Z, 1f), GetViewTransform() * GetProjectionTransform(in viewport));
            var clipSpace = new Vector2(postProjectiveLocation.X / postProjectiveLocation.W, -postProjectiveLocation.Y / postProjectiveLocation.W); // <- inverted Y axis for screen coords already in here
            return (clipSpace + Vector2.One) * 0.5f * viewport.SizeF; // -> pixel space
        }

        /// <summary>
        /// Returns the world coords where a ray through a pixel of the screen intersects with a world Z plane.
        /// </summary>
        public Vector3 ScreenToWorld(in Vector2 screenLocation, float worldZPlane, in Viewport viewport)
        {
            var ray = GetCameraRay(in screenLocation, in viewport);

            // x = x0 + ta
            // y = y0 + tb
            // z = z0 + tc
            //
            // x0,y0,z0 = origin
            // a b c = direction

            // find t where z == worldZPlane:
            var t = (worldZPlane - ray.Origin.Z) / ray.Direction.Z;

            if (t < 0)
                return Vector3.Zero; // ray is pointing away from the requested worldZPlane.

            return ray.Origin + ray.Direction * t;
        }

        /// <summary>
        /// returns a world ray from the camera in the direction of a given pixel on the screen (in screen-pixel coords)
        /// </summary>
        public Ray3 GetCameraRay(in Vector2 pixelLocation, in Viewport viewport)
        {
            // https://sibaku.github.io/computer-graphics/2017/01/10/Camera-Ray-Generation.html

            var locationScreenSpace = pixelLocation * 2 / viewport.SizeF - Vector2.One; // -> screenspace [-1, +1]
            locationScreenSpace.Y = -locationScreenSpace.Y;

            Matrix4x4.Invert(GetViewTransform(), out var viewInverse);
            Matrix4x4.Invert(GetProjectionTransform(in viewport), out var projectionInverse);

            var directionScreenSpace = new Vector4(locationScreenSpace.X, locationScreenSpace.Y, 0, 1);
            var directionViewSpace = Vector4.Transform(directionScreenSpace, projectionInverse);
            directionViewSpace.W = 0f;
            var directionWorldSpace = Vector4.Transform(directionViewSpace, viewInverse);
            return new Ray3(Position, Vector3.Normalize(new Vector3(directionWorldSpace.X, directionWorldSpace.Y, directionWorldSpace.Z))); 
        }

        public void LookAt(Vector3 target, Vector3 up)
        {
            _lookAt = target;
            _lookAtUp = up;
        }

        public float FovY { get; set; } = Angles._045;

        public Vector3 Position { get; set; }

        public Matrix4x4 GetViewTransform() => Matrix4x4.CreateLookAt(Position, _lookAt, _lookAtUp);

        public Matrix4x4 GetProjectionTransform(in Viewport viewport) => Matrix4x4.CreatePerspectiveFieldOfView(FovY, viewport.AspectRatio, NearPlane, FarPlane);

        public float NearPlane { get; set; } = 1;

        public float FarPlane { get; set; } = 10000;
    }
}
