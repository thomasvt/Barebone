namespace Barebone.UI
{
    public interface IClipboard
    {
        void SetText(string text);
        string GetText();
    }
}
