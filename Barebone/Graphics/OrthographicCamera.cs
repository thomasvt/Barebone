using System.Numerics;
using BareBone.Geometry.Triangulation;
using Barebone.Graphics;

namespace BareBone.Graphics
{
    public enum OriginPosition
    {
        Center,
        TopLeft
    }

    public class OrthographicCamera : ICamera
    {
        private Vector3 _lookAt = Vector3.Zero;
        private Vector3 _lookAtUp = Vector3.UnitY;

        /// <summary>
        /// Gets the world size of a subject at the given distance from the camera, to have it render as the size of exactly one device pixel.
        /// </summary>
        private float GetDevicePixelSizeInWorld(float distance)
        {
            throw new NotImplementedException();
            // return distance * ViewY;
        }

        public Vector2 WorldToScreen(Vector3 worldLocation)
        {
            throw new NotImplementedException();
            // var postProjectiveLocation = Vector4.Transform(new Vector4(worldLocation.X, worldLocation.Y, worldLocation.Z, 1f), ViewTransform * ProjectionTransform);
            // var clipSpace = new Vector2(postProjectiveLocation.X / postProjectiveLocation.W, -postProjectiveLocation.Y / postProjectiveLocation.W); // <- inverted Y axis for screen coords already in here
            // return (clipSpace + Vector2.One) * 0.5f * new Vector2(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height); // -> screen space
        }

        public Vector3 ScreenToWorld(in Vector2 screenLocation, float worldZPlane)
        {
            throw new NotImplementedException();
            // var ray = GetCameraRay(screenLocation);
            // 
            // // x = x0 + ta
            // // y = y0 + tb
            // // z = z0 + tc
            // //
            // // x0,y0,z0 = origin
            // // a b c = direction
            // 
            // // find t where z == worldZPlane:
            // var t = (worldZPlane - ray.Origin.Z) / ray.Direction.Z;
            // 
            // if (t < 0)
            //     return Vector3.Zero; // ray is pointing away from the requested worldZPlane.
            // 
            // return ray.Origin + ray.Direction * t;
        }

        /// <summary>
        /// returns a world ray from the camera in the direction of a given pixel on the screen (in normal 2D pixel coords)
        /// </summary>
        /// <param name="pixelLocation"></param>
        /// <returns></returns>
        public Ray3D GetCameraRay(in Vector2 pixelLocation)
        {
            throw new NotImplementedException();

            // // https://sibaku.github.io/computer-graphics/2017/01/10/Camera-Ray-Generation.html
            // 
            // var locationScreenSpace = pixelLocation * 2 / new Vector2(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height) - Vector2.One; // -> screenspace [-1, +1]
            // locationScreenSpace.Y = -locationScreenSpace.Y;
            // 
            // Matrix4x4.Invert(ViewTransform, out var viewInverse);
            // Matrix4x4.Invert(ProjectionTransform, out var projectionInverse);
            // 
            // var directionScreenSpace = new Vector4(locationScreenSpace.X, locationScreenSpace.Y, 0, 1);
            // var directionViewSpace = Vector4.Transform(directionScreenSpace, projectionInverse);
            // directionViewSpace.W = 0f;
            // var directionWorldSpace = Vector4.Transform(directionViewSpace, viewInverse);
            // return new Ray3D(Position, Vector3.Normalize(new Vector3(directionWorldSpace.X, directionWorldSpace.Y, directionWorldSpace.Z))); 
        }

        public void LookAt(Vector3 target, Vector3 up)
        {
            _lookAt = target;
            _lookAtUp = up;
        }

        public Vector3 Position { get; set; }

        public float NearPlane { get; set; } = 0;

        public float FarPlane { get; set; } = 1;

        public OriginPosition Origin { get; set; }

        /// <summary>
        /// How many world units fit on one screen's height?
        /// </summary>
        public float ViewY { get; set; } = 600f;

        public Matrix4x4 GetViewTransform()
        {
            return Matrix4x4.CreateLookAt(Position, _lookAt, _lookAtUp);
        }

        public Matrix4x4 GetProjectionTransform(in Viewport viewport)
        {
            if (Origin == OriginPosition.Center)
                return Matrix4x4.CreateOrthographic(ViewY * viewport.AspectRatio, ViewY, NearPlane, FarPlane);

            var width = ViewY * viewport.AspectRatio;
            return Matrix4x4.CreateOrthographicOffCenter(0, width, ViewY, 0, NearPlane, FarPlane);
        }

        /// <summary>
        /// Creates an orthographic camera for 2D canvas rendering with the screen topleft being (0,0), and topright being (width, height).
        /// Note that 'width' depends on the viewport's actual aspectratio at runtime.
        /// </summary>
        /// <param name="height">Heigth of the canvas. Width is derived from this and the viewport's aspectratio.</param>
        /// <param name="maxZ">Max allowed Z for render sorting with depth buffer. minZ is always 0.</param>
        public static OrthographicCamera For2DCanvas(int height, int maxZ)
        {
            return new OrthographicCamera
            {
                Position = new(0, 0, maxZ),
                _lookAt = new(0, 0, 0),
                _lookAtUp = new(0, 1, 0), // RREALLY weird up = Y+, coords of meshes are lower == higher on the screen
                NearPlane = 0,
                FarPlane = maxZ,
                ViewY = height,
                Origin = OriginPosition.TopLeft
            };
        }
    }
}
