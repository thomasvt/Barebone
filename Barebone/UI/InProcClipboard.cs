namespace Barebone.UI
{
    /// <summary>
    /// InProc clipboard, not connected to OS.
    /// </summary>
    public class InProcClipboard : IClipboard
    {
        private string _text = string.Empty;
        public void SetText(string text)
        {
            _text = text;
        }

        public string GetText()
        {
            return _text;
        }
    }
}
