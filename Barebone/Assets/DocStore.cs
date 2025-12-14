using System.Text.Json;

namespace Barebone.Assets
{
    public class DocStore(string path) : TextFileStore(path)
    {
        public T GetAs<T>(string filename)
        {
            var json = Get(filename);
            var t = JsonSerializer.Deserialize<T>(json);
            return t ?? throw new Exception($"'{filename}' is not deserializable to '{typeof(T).Name}'.");
        }
    }
}
