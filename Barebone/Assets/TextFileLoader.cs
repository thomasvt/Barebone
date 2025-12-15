using System.Text.Json;

namespace Barebone.Assets
{
    public class TextFileLoader(string path)
    {
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
    }
}
