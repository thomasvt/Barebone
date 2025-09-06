using System.Numerics;

namespace Barebone.Physics
{
    /// <summary>
    /// Physical spring simulator for a spring with length 0.
    /// </summary>
    public record struct Spring(float Stiffness, float Damping)
    {
        private Vector2 _springVelocity;

        public void Update(in float deltaT)
        {
            Position += _springVelocity * deltaT;

            var force = -Position * Stiffness - Damping * _springVelocity;
            _springVelocity += force * deltaT;
        }

        /// <summary>
        /// Current position of the dynamic end of the spring.
        /// </summary>
        public Vector2 Position { get; set; }
    }
}
