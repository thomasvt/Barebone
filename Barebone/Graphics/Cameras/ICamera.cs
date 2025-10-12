using System.Numerics;

namespace Barebone.Graphics.Cameras;

public interface ICamera
{
    Matrix4x4 GetViewTransform();
    Matrix4x4 GetProjectionTransform(in Viewport viewport);
}