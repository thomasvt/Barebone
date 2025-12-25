namespace Barebone.Graphics.NodeArt.Core
{
    public interface INaParameter
    {
        /// <summary>
        /// Resets this parameter to its default value.
        /// </summary>
        void ResetToDefault();

        event Action? ValueChange;
    }

    public class NaParameter<T>(T? defaultValue) : INaParameter
    {
        public T? Value {
            get;
            set
            {
                if (Equals(field, value)) return;

                field = value;
                ValueChange?.Invoke();
            }
        }

        public void ResetToDefault()
        {
            Value = defaultValue;
        }

        public event Action? ValueChange;
    }
}
