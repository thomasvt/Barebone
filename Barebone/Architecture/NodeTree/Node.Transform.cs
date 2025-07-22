using System.Numerics;
using Barebone.Geometry;

namespace Barebone.Architecture.NodeTree
{
    /// <summary>
    /// Spatial and transform aspects of Node.
    /// </summary>
    public partial class Node
    {
        private bool _worldToLocalDirty = true;

        private Vector3 _eulerAngles;

        private Matrix4x4 _parentTransform = Matrix4x4.Identity;
        private float _parentAngle = 0;

        private Vector3 _position3D;

        private Matrix4x4 _localToWorld = Matrix4x4.Identity;

        private Matrix4x4 _worldToLocal;

        protected virtual void OnUpdateTransform(in Matrix4x4 transform)
        {
        }

        private void UpdateTransform()
        {
            // todo performance: merge rotation + translation transform into one
            _localToWorld = Matrix4x4.CreateRotationX(_eulerAngles.X) * Matrix4x4.CreateRotationY(_eulerAngles.Y) *
                            Matrix4x4.CreateRotationZ(_eulerAngles.Z) * Matrix4x4.CreateTranslation(Position3D)
                            * _parentTransform;
            var angleWorld = AngleWorld;

            OnUpdateTransform(in _localToWorld);
            foreach (var child in Children)
            {
                child.ParentTransform = _localToWorld;
                child.ParentAngle = angleWorld;
            }

            _worldToLocalDirty = true; // inverse calc is heavy and not often used, so it is lazy. Mark it dirty.
        }

        /// <summary>
        /// position for 2D games
        /// </summary>
        public Vector2 Position
        {
            get => _position3D.XY();
            set
            {
                if (_position3D.XY() == value) return;
                _position3D = value.ToVector3(_position3D.Z);
                UpdateTransform();
            }
        }

        /// <summary>
        /// Z separately, for convenient Z sorting in 2D games.
        /// </summary>
        public float Z
        {
            get => _position3D.Z;
            set
            {
                if (_position3D.Z == value) return;
                _position3D = _position3D with { Z = value };
                UpdateTransform();
            }
        }

        private Matrix4x4 ParentTransform
        {
            get => _parentTransform;
            set
            {
                if (_parentTransform == value) return;

                _parentTransform = value;
                UpdateTransform();
            }
        }

        private float ParentAngle
        {
            get => _parentAngle;
            set
            {
                if (_parentAngle == value) return;

                _parentAngle = value;
                UpdateTransform();
            }
        }

        public Vector3 Position3D
        {
            get => _position3D;
            set
            {
                if (_position3D == value) return;
                _position3D = value;
                UpdateTransform();
            }
        }

        /// <summary>
        /// Angle around Z. For 2D games.
        /// </summary>
        public float Angle
        {
            get => _eulerAngles.Z;
            set
            {
                var newValue = value.NormalizeAngle();
                if (_eulerAngles.Z == newValue) return;
                _eulerAngles.Z = newValue;
                UpdateTransform();
            }
        }

        public float AngleWorld => _parentAngle + Angle;

        /// <summary>
        /// Euler angles around axes X Y and Z.
        /// </summary>
        public Vector3 EulerAngles
        {
            get => _eulerAngles;
            set
            {
                var newValue = new Vector3(value.X.NormalizeAngle(), value.Y.NormalizeAngle(), value.Z.NormalizeAngle());
                if (newValue == _eulerAngles) return;
                _eulerAngles = newValue;
                UpdateTransform();
            }
        }

        public Vector2 PositionWorld => LocalToWorld.Translation.XY();

        public Vector3 Position3DWorld => LocalToWorld.Translation;

        /// <summary>
        /// Local to World transform.
        /// </summary>
        public Matrix4x4 LocalToWorld => _localToWorld;

        /// <summary>
        /// World to Local transform.
        /// </summary>
        public Matrix4x4 WorldToLocal
        {
            get
            {
                if (_worldToLocalDirty)
                    Matrix4x4.Invert(LocalToWorld, out _worldToLocal);

                _worldToLocalDirty = false;
                return _worldToLocal;
            }
        }
    }
}
