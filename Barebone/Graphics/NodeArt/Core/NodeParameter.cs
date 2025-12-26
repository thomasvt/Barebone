namespace Barebone.Graphics.NodeArt.Core
{
    public interface INodeParameter
    {
        /// <summary>
        /// Resets this parameter to its default value.
        /// </summary>
        void ResetToDefault();

        event Action? ValueChange;
    }

    public class NodeParameter<T>(string name, T? defaultValue) : INodeParameter
    {
        public string Name { get; } = name;

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

        public T GetValueOrThrow()
        {
            if (Value == null) throw new Exception($"Parameter '{Name}' is not set.");
            return Value!;
        }
    }
}
