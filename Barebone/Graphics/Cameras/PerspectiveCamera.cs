using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Graphics.Cameras
{
    /// <summary>
    /// A perspective camera, right handed, defaulted to Fov 60, position (0,0,10) looking at (0,0,0), Y up (0,1,0)
    /// </summary>
    public class PerspectiveCamera : ICamera
    {
        public Vector3 LookTo { get; set; } = new(0, 0, -1);
        public Vector3 Up { get; set; } = new(0, 1, 0);
        public float FovY { get; set; } = Angles._060;

        public Vector3 Position { get; set; } = new(0, 0, -10);

        public Matrix4x4 GetViewTransform() => Matrix4x4.CreateLookTo(Position, LookTo, Up);

        public Matrix4x4 GetProjectionTransform(in Viewport viewport) => Matrix4x4.CreatePerspectiveFieldOfView(FovY, viewport.AspectRatio, NearPlane, FarPlane);

        public float NearPlane { get; set; } = 1;

        public float FarPlane { get; set; } = 100;
        
        /// <summary>
        /// Configure the camera to have Y+ up, Z+ towards the user, X+ to the right, looking at (0,0,0) at a given `distanceOnZ`.
        /// </summary>
        public void LookAtXYPlane(float distanceOnZ, float nearPlane, float farPlane)
        {
            Position = new(0, 0, distanceOnZ);
            NearPlane = nearPlane;
            FarPlane = farPlane;
            LookTo = new(0, 0, -1);
            Up = Vector3.UnitY;
        }

        /// <summary>
        /// Returns the pixel coords where a world location is projected onto the screen.
        /// </summary>
        public Vector2 WorldToScreen(Vector3 worldLocation, in Viewport viewport)
        {
            var postProjectiveLocation = Vector4.Transform(new Vector4(worldLocation.X, worldLocation.Y, worldLocation.Z, 1f), GetViewTransform() * GetProjectionTransform(in viewport));
            var clipSpace = new Vector2(postProjectiveLocation.X / postProjectiveLocation.W, -postProjectiveLocation.Y / postProjectiveLocation.W); // <- inverted Y axis for screen coords already in here
            return (clipSpace + Vector2.One) * 0.5f * viewport.Size.ToVector2(); // -> pixel space
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

            var locationScreenSpace = pixelLocation * 2 / viewport.Size.ToVector2() - Vector2.One; // -> screenspace [-1, +1]
            locationScreenSpace.Y = -locationScreenSpace.Y;

            Matrix4x4.Invert(GetViewTransform(), out var viewInverse);
            Matrix4x4.Invert(GetProjectionTransform(in viewport), out var projectionInverse);

            var directionScreenSpace = new Vector4(locationScreenSpace.X, locationScreenSpace.Y, 0, 1);
            var directionViewSpace = Vector4.Transform(directionScreenSpace, projectionInverse);
            directionViewSpace.W = 0f;
            var directionWorldSpace = Vector4.Transform(directionViewSpace, viewInverse);
            return new Ray3(Position, Vector3.Normalize(new Vector3(directionWorldSpace.X, directionWorldSpace.Y, directionWorldSpace.Z))); 
        }
    }
}
