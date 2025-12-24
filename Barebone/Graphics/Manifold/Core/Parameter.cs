namespace Barebone.Graphics.Manifold.Core
{
    public interface IParameter
    {
        /// <summary>
        /// Resets this parameter to its default value.
        /// </summary>
        void ResetToDefault();

        /// <summary>
        /// True if the value has been changed since last Cook of the Node it is part of.
        /// </summary>
        bool IsDirty { get; }
        void ResetIsDirty();
    }

    public class Parameter<T>(T defaultValue) : IParameter
    {
        public T Value {
            get;
            set
            {
                if (Equals(field, value)) return;

                field = value;
                IsDirty = true;
            }
        }

        public void ResetToDefault()
        {
            Value = defaultValue;
        }

        public void ResetIsDirty()
        {
            IsDirty = false;
        }

        public bool IsDirty { get; private set; }
    }
}
