using System.Text;
using Barebone.Geometry;
using Barebone.UI.Undo;

namespace Barebone.UI.Controls
{
    internal enum CharType
    {
        Whitespace,
        Identifier,
        NonIdentifier
    }

    /// <summary>
    /// Abstract model that allows multi line text editing. Drives the <see cref="TextBox"/> which adds UI to this TextEditor.
    /// </summary>
    internal class TextEditor<TUndoData>
    {
        private readonly Action<TUndoData> _applyCustomUndoData;

        private record Snapshot<TUndoData>(string[] Lines, TUndoData CustomData);

        private readonly List<string> _lines = new();
        public int LineCount => _lines.Count;
        public IReadOnlyList<string> Lines => _lines;

        private readonly IUndoHistory<Snapshot<TUndoData>> _history;
        private string? _previousFullText;

        public TextEditor(bool enableUndo, Func<TUndoData> getCustomUndoData, Action<TUndoData> applyCustomUndoData)
        {
            _applyCustomUndoData = applyCustomUndoData;
            _history = enableUndo 
                ? new UndoHistory<Snapshot<TUndoData>>(100, () => new Snapshot<TUndoData>(_lines.ToArray(), getCustomUndoData.Invoke()))
                : new NullUndoHistory<Snapshot<TUndoData>>();
        }

        /// <summary>
        /// Returns the textindex of the end of the entire text.
        /// </summary>
        public Vector2I GetEndOfText()
        {
            var lastLine = _lines.Last();
            return new Vector2I(lastLine.Length, _lines.Count - 1);
        }

        public void AddLineNoUndo(string text)
        {
            _lines.Add(text);
            TextModified?.Invoke();
        }

        public void RemoveLineRangeNoUndo(int offset, int count)
        {
            _lines.RemoveRange(offset, count);
            TextModified?.Invoke();
        }

        /// <summary>
        /// Moves one character towards the beginning of the text, stepping to the previous line if necessary.
        /// </summary>
        public Vector2I StepBackwards(Vector2I textIndex, bool jumpToNextToken)
        {
            if (!jumpToNextToken)
            {
                if (textIndex.X > 0)
                    return new(textIndex.X - 1, textIndex.Y);
                if (textIndex.Y > 0)
                    return new(_lines[textIndex.Y - 1].Length, textIndex.Y - 1);

                return textIndex; // we're (0,0)
            }
            else
            {
                return textIndex;
            }
        }

        /// <summary>
        /// Moves one character towards the end of the text, stepping to the next line if necessary.
        /// </summary>
        public Vector2I StepForwards(Vector2I textIndex, bool jumpToNextToken)
        {
            var line = _lines[textIndex.Y];
            var lineLength = line.Length;

            if (!jumpToNextToken)
            {
                if (textIndex.X < lineLength)
                    return new(textIndex.X + 1, textIndex.Y);
                if (textIndex.Y < _lines.Count - 1)
                    return new(0, textIndex.Y + 1);

                return textIndex; // we're at the last char of all lines.
            }
            else
            {
                // jump to next token
                return textIndex;
            }
        }

        private CharType GetCharType(char c)
        {
            if (char.IsWhiteSpace(c)) return CharType.Whitespace;
            if (char.IsLetterOrDigit(c) || c == '_') return CharType.Identifier;
            return CharType.NonIdentifier;
        }

        private void ReplaceLines(string[] lines)
        {
            _lines.Clear();
            if (IsMultiLine)
            {
                _lines.AddRange(lines);
            }
            else
            { 
                _lines.Add(lines.FirstOrDefault(""));
            }
            
            TextModified?.Invoke();
        }

        public IReadOnlyList<string> GetLineRange(int index, int count)
        {
            return _lines.GetRange(index, count);
        }

        /// <summary>
        /// Gets a single string containing the multi-line range containing \n for line separators.
        /// </summary>
        public string GetTextRange(Vector2I from, Vector2I to)
        {
            var sb = new StringBuilder();
            for (var y = from.Y; y <= to.Y; y++)
            {
                var line = _lines[y].AsSpan();

                var beginX = 0;
                var endX = line.Length;

                if (y == from.Y)
                    beginX = from.X;
                if (y == to.Y)
                    endX = to.X;

                if (y > from.Y)
                    sb.AppendLine();
                sb.Append(line.Slice(beginX, endX - beginX));
            }
            return sb.ToString();
        }

        public string GetFullText()
        {
            return string.Join(Environment.NewLine, _lines);
        }

        public bool SetFullText(string text, bool resetUndoHistory)
        {
            if (text != _previousFullText)
            {
                if (resetUndoHistory)
                    _history.Clear();
                else
                    _history.Record();

                ReplaceLines(text.Split('\n'));
                _previousFullText = text;
                return true;
            }
            return false;
        }

        public Vector2I ReplaceTextRange(Vector2I begin, Vector2I end, string text)
        {
            if (string.IsNullOrEmpty(text) && begin == end) return end;

            _history.Record();

            RemoveTextRangeInternal(begin, end);
            var finalPos = InsertInternal(begin, text);
            TextModified?.Invoke();
            return finalPos;
        }

        public void RemoveTextRange(Vector2I from, Vector2I to)
        {
            if (from == to)
                return;

            _history.Record();
            RemoveTextRangeInternal(from, to);
            TextModified?.Invoke();
        }
        public bool TryUndo()
        {
            if (_history.TryUndo(out var snapshot))
            {
                ApplyHistorySnapshot(snapshot);
                return true;
            }
            return false;
        }

        public bool TryRedo()
        {
            if (_history.TryRedo(out var snapshot))
            {
                ApplyHistorySnapshot(snapshot);
                return true;
            }
            return false;
        }

        private void ApplyHistorySnapshot(Snapshot<TUndoData> snapshot)
        {
            ReplaceLines(snapshot.Lines);
            _applyCustomUndoData(snapshot.CustomData);
        }

        private void RemoveTextRangeInternal(Vector2I from, Vector2I to)
        {
            if (from == to)
                return;

            if (from.Y == to.Y)
            {
                _lines[from.Y] = _lines[from.Y].Remove(from.X, to.X - from.X);
            }
            else
            {
                var firstLine = _lines[from.Y];
                var lastLine = _lines[to.Y];
                _lines[from.Y] = firstLine[..from.X] + lastLine[to.X..];
                if (to.Y - from.Y > 0)
                    _lines.RemoveRange(from.Y + 1, to.Y - from.Y);
            }
        }

        /// <summary>
        /// Insert `text` at `textIdx` with newline support.
        /// </summary>
        private Vector2I InsertInternal(Vector2I textIdx, string text)
        {
            var linesToInsert = text.Split('\n').Select(l => l.Replace("\r", "").Replace("\t", "")).ToArray().AsSpan();
            
            if (linesToInsert.Length == 0) return textIdx;
            if (!IsMultiLine)
                linesToInsert = linesToInsert[..1];

            var line = _lines[textIdx.Y];
            var before = line[..textIdx.X];
            var after = line[textIdx.X..];

            var y = textIdx.Y;
            if (linesToInsert.Length > 1)
                _lines.InsertRange(y + 1, linesToInsert[1..]); // copy all but the first line, because the first will reuse the line the caret was in.

            var lastY = y + linesToInsert.Length - 1;

            _lines[y] = before + linesToInsert[0];
            var caretX = _lines[lastY].Length;
            _lines[lastY] += after; // ensure 'before' is kept if lastY == y

            TextModified?.Invoke();

            return new(caretX, lastY);
        }

        public event Action? TextModified;


        public bool IsMultiLine { get; set; }
    }

}
