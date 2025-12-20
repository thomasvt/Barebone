using System.Text.Json;
using YamlDotNet.Serialization;

namespace Barebone.Assets
{
    public class TextFileLoader(string path)
    {
        private Deserializer? _deserializer;
        private Serializer? _serializer;

        public string Load(string filename)
        {
            return File.ReadAllText(Path.Combine(path, filename));
        }

        public string[] GetFilenames(string filemask)
        {
            return Directory.GetFiles(path, filemask);
        }

        public T LoadJsonAs<T>(string filename)
        {
            var json = Load(filename);
            var t = JsonSerializer.Deserialize<T>(json);
            return t ?? throw new Exception($"'{filename}' is not deserializable to '{typeof(T).Name}'.");
        }

        public void SaveAsJson<T>(string filename, T document)
        {
            var json = JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(path, filename), json);
        }

        public T LoadYamlAs<T>(string filename)
        {
            _deserializer ??= new Deserializer();
            var yaml = Load(filename);
            var t = _deserializer.Deserialize<T>(yaml);
            return t ?? throw new Exception($"'{filename}' is not deserializable to '{typeof(T).Name}'.");
        }

        public void SaveAsYaml<T>(string filename, T document)
        {
            _serializer ??= new Serializer();
            var yaml = _serializer.Serialize(document);
            File.WriteAllText(Path.Combine(path, filename), yaml);
        }
    }
}
