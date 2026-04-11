using System.Numerics;

namespace Barebone.Game.Graphics
{
    public class Camera : ICamera
    {
        private Vector2 _viewportSize;
        private Matrix3x2 _screenToWorld;
        private Matrix3x2 _worldToScreen;

        public Vector2 Position
        {
            get;
            set
            {
                field = value;
                CalculateMatrices();
            }
        }
        public float Zoom
        {
            get;
            set
            {
                if (Zoom <= 0) throw new Exception($"Zoom must be > 0 (not {value})");
                field = value;
                CalculateMatrices();
            }
        } = 1f;


        public float? LogicalViewHeight { 
            get;
            set
            {
                field = value;
                CalculateMatrices();
            }
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, ScreenToWorldTransform);
        }

        public float ScreenLengthToWorld(float length)
        {
            return length * ScreenToWorldTransform.M11;
        }

        public Vector2 WorldToScreen(Vector2 position)
        {
            return Vector2.Transform(position, WorldToScreenTransform);
        }

        public float WorldLengthToScreen(float length)
        {
            return length * WorldToScreenTransform.M11;
        }

        public void SetViewportSize(Vector2 viewportSize)
        {
            _viewportSize = viewportSize;
            CalculateMatrices();
        }

        private void CalculateMatrices()
        {
            var viewportCenter = _viewportSize * 0.5f;
            
            var combinedZoom = Zoom;
            if (LogicalViewHeight.HasValue) combinedZoom *= _viewportSize.Y / LogicalViewHeight.Value;

            _screenToWorld = Matrix3x2.CreateTranslation(-viewportCenter) * Matrix3x2.CreateScale(1f/ combinedZoom) * Matrix3x2.CreateTranslation(Position);
            _worldToScreen = Matrix3x2.CreateTranslation(-Position) * Matrix3x2.CreateScale(combinedZoom) * Matrix3x2.CreateTranslation(viewportCenter);
        }

        public Matrix3x2 WorldToScreenTransform
        {
            get
            {
                CalculateMatrices();
                return _worldToScreen;
            }
        }

        public Matrix3x2 ScreenToWorldTransform
        {
            get
            {
                CalculateMatrices();
                return _screenToWorld;
            }
        }
    }
}
