using System.Text;

namespace BareBone
{
    /// <summary>
    /// StringBuilder for building multiline strings with indentation control.
    /// </summary>
    public class IndentedStringBuilder
    {
        private readonly string _indent;
        private int _indentValue;
        private readonly StringBuilder _sb = new();
        private bool _newLineDone;
        private string _indentation;

        public IndentedStringBuilder(string indent)
        {
            _indent = indent;
            _indentation = "";
            _newLineDone = true;
        }

        public void Indent(int amount = 1)
        {
            SetIndentLevel(_indentValue + amount);
        }

        public void Outdent(int amount = 1)
        {
            SetIndentLevel(_indentValue - amount);
        }

        public void Append(string text)
        {
            if (_newLineDone)
            {
                _sb.Append(_indentation);
                _newLineDone = false;
            }

            _sb.Append(text);
        }

        public void AppendLine(string? text = null)
        {
            Append(text + Environment.NewLine);
            _newLineDone = true;
        }

        /// <summary>
        /// Enters to a new line, but only if it's not already on an empty line.
        /// </summary>
        public void EnsureOnNewLine()
        {
            if (!_newLineDone)
                AppendLine();
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        public void SetIndentLevel(int indent)
        {
            if (indent < 0) throw new Exception($"Negative indentation ({indent}) encountered.");
            _indentValue = indent;
            _indentation = string.Concat(Enumerable.Range(0, _indentValue).Select(i => _indent));
        }
    }
}
