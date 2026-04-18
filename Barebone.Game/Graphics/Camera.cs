using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Game.Graphics
{
    internal class Camera : ICamera
    {
        private Matrix3x2 _screenToWorld;
        private Matrix3x2 _worldToScreen;
        private Vector2I _viewportSize;
        private bool _isDirty;

        internal Camera()
        {
            _isDirty = true;
        }

        public Vector2 Position
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                _isDirty = true;
            }
        }
        public float Zoom
        {
            get;
            set
            {
                if (field == value) return;
                if (Zoom <= 0) throw new Exception($"Zoom must be > 0 (not {value})");
                field = value;
                _isDirty = true;
            }
        } = 1f;


        public float? LogicalViewHeight
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                _isDirty = true;
            }
        }

        public ScreenOrigin ScreenOrigin
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                _isDirty = true;
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

        internal void SetViewportSize(Vector2I viewportSize)
        {
            if (viewportSize == _viewportSize) return;

            _viewportSize = viewportSize;
            _isDirty = true;
        }

        private void EnsureMatrices()
        {
            if (!_isDirty) return;


            var combinedZoom = Zoom;
            if (LogicalViewHeight.HasValue) combinedZoom *= _viewportSize.Y / LogicalViewHeight.Value;

            if (ScreenOrigin == ScreenOrigin.Center)
            {
                var viewportCenter = _viewportSize * 0.5f;
                _screenToWorld = Matrix3x2.CreateTranslation(-viewportCenter) * Matrix3x2.CreateScale(1f / combinedZoom) * Matrix3x2.CreateTranslation(Position);
                _worldToScreen = Matrix3x2.CreateTranslation(-Position) * Matrix3x2.CreateScale(combinedZoom) * Matrix3x2.CreateTranslation(viewportCenter);
            }
            else
            {
                _screenToWorld = Matrix3x2.CreateScale(1f / combinedZoom) * Matrix3x2.CreateTranslation(Position);
                _worldToScreen = Matrix3x2.CreateTranslation(-Position) * Matrix3x2.CreateScale(combinedZoom);
            }

            _isDirty = false;
        }

        public Matrix3x2 WorldToScreenTransform
        {
            get
            {
                EnsureMatrices();
                return _worldToScreen;
            }
        }

        public Matrix3x2 ScreenToWorldTransform
        {
            get
            {
                EnsureMatrices();
                return _screenToWorld;
            }
        }
    }
}
