namespace Barebone.Assets
{
    public class TextFileStore(string path) : AssetStore<string>
    {
        protected override string Load(string filename)
        {
            return File.ReadAllText(Path.Combine(path, filename));
        }
    }
}
