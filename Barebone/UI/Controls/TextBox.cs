using System.Drawing;
using System.Numerics;
using Barebone.Geometry;
using Barebone.Graphics;
using Barebone.Graphics.Gpu;
using Barebone.Input;
using Barebone.Pools;
using Barebone.UI.Text;

namespace Barebone.UI.Controls
{
    /// <summary>
    /// A single or multi-line text editor with Undo
    /// </summary>
    public class TextBox : UIControl
    {
        private readonly Vector2I _textOffset = new(8, 2);

        private Vector2I _caret = Vector2I.Zero;

        private readonly Mesh _caretMesh = Pool.Rent<Mesh>();
        private readonly BBList<GpuTexTriangle> _textTriangles = new();
        private readonly TextEditor<Vector2I> _textEditor;
        private float _caretBlinkStart;
        private Font _font;

        private Vector2I _selectBegin;
        private Vector2I _selectEnd;
        private bool _isShiftDown, _isMouseLeftDown;

        public TextBox(UserInterface ui, bool enableUndo) : base(ui)
        {
            _textEditor = new(enableUndo, () => _caret, c => _caret = c);
            TextColor = ui.DefaultTextColor;
            IsFocussable = true;
            Font = ui.DefaultFont;
            BorderThickness = 2;
            BorderColor = BorderColorNormal = PaletteApollo.Pink0;
            BorderColorFocus = PaletteApollo.Pink4;
            BackgroundColor = PaletteApollo.Blue1;
            SelectionColor = PaletteApollo.Pink4;
            IsMultiLine = false;
            AllowTabIndent = false;

            _textEditor.TextModified += () =>
            {
                TextChanged?.Invoke();
                InvalidateVisual();
            };
            _textEditor.AddLineNoUndo(string.Empty); // for caret positioning, there must always be >= 1 line here!
            MoveCaretTo(new(0));
        }

        public void AppendLine(string line)
        {
            _textEditor.AddLineNoUndo(line);
        }

        public void RemoveLineRange(int offset, int count)
        {
            _textEditor.RemoveLineRangeNoUndo(offset, count);
        }

        protected override void Draw()
        {
            base.Draw();
            DrawCaretMesh();
            DrawSelection();
            DrawText();
        }

        private void DrawCaretMesh()
        {
            var contrastColor = new ColorHSL(BackgroundColor).L < 128 ? PaletteApollo.Gray9 : PaletteApollo.Gray0;
            _caretMesh.LineInZ(new(0.5f, 0), new(0.5f, -Font.LineHeight), 0.5f, 0, contrastColor);
        }

        private void DrawSelection()
        {
            if (IsSelectionEmpty()) return;

            GetSelectionRange(out var begin, out var end);

            for (var y = begin.Y; y <= end.Y; y++)
            {
                var beginPos = y == begin.Y
                    ? TextIndexToScreen(begin)
                    : TextIndexToScreen(new(0, y));
                var endPos = y == end.Y
                    ? TextIndexToScreen(end)
                    : TextIndexToScreen(new(_textEditor.Lines[y].Length, y)) + new Vector2I(Font.LineHeight / 2, 0);

                if (beginPos.HasValue && endPos.HasValue)
                    Mesh.FillAabbInZ(new Aabb(beginPos.Value + new Vector2(0, -Font.LineHeight), endPos.Value), 0, SelectionColor);
            }
        }

        /// <summary>
        /// Get the selection's range with 'begin' quaranteed to come before 'end'.
        /// When user has no visible selection, selection range is still an empty range on the caret position.
        /// </summary>
        private void GetSelectionRange(out Vector2I begin, out Vector2I end)
        {
            begin = _selectBegin;
            end = _selectEnd;

            // swap if begin > end
            if (begin.Y > end.Y || begin.Y == end.Y && begin.X > end.X)
                (begin, end) = (end, begin);
        }

        private void DrawText()
        {
            _textTriangles.Clear();

            var maxLinesVisible = Viewport.Height / _font.LineHeight;
            var topLeft = Viewport.MinCorner + _textOffset;

            if (maxLinesVisible <= 0)
                return;

            var linesInViewport = GetLinesToDraw(maxLinesVisible, out var topIndexActual);
            TopIndexActual = topIndexActual;
            BottomIndexActual = topIndexActual + maxLinesVisible;

            var cursor = topLeft;
            foreach (var line in linesInViewport)
            {
                _font.AppendString(_textTriangles, line, TextColor, cursor);
                cursor.Y += _font.LineHeight;
            }
        }

        private IReadOnlyList<string> GetLinesToDraw(int maxLinesVisible, out int actualTopIndex)
        {
            actualTopIndex = Math.Min(TopIndex, Math.Max(0, _textEditor.LineCount - maxLinesVisible));
            var actualLineCount = _textEditor.LineCount - actualTopIndex;
            return _textEditor.GetLineRange(actualTopIndex, actualLineCount);
        }

        protected override void Render(IImmediateRenderer renderer)
        {
            base.Render(renderer);
            if (HasFocus && (UI.Clock.Now - _caretBlinkStart) % 1f < 0.5f)
                RenderCaret(renderer);
            renderer.Draw(Matrix4x4.Identity, _textTriangles.AsReadOnlySpan(), _font.Texture);
        }

        private void RenderCaret(IImmediateRenderer renderer)
        {
            var p = TextIndexToScreen(_caret);
            if (p.HasValue)
                renderer.Draw(Matrix4x4.CreateTranslation(p.Value.X, p.Value.Y, 0), _caretMesh, TextColor);
        }

        protected override void OnFocusChanged(bool isFocussed)
        {
            base.OnFocusChanged(isFocussed);
            if (isFocussed)
            {
                ResetCaretBlink();
            }
        }

        public override void OnTypeInput(char ch, Barebone.Platform.Inputs.Button button)
        {
            if (IsReadOnly) return;

            if (!char.IsControl(ch)) // is it a printable character?
                TypeText(ch.ToString());
        }

        public override void OnKeyStroke(KeyStrokeEvent e)
        {
            if (!IsReadOnly)
                ProcessEditingStrokes(e);

            ProcessNavigationStrokes(e);
        }

        public override void OnKeyDown(Barebone.Platform.Inputs.Button button)
        {
            if (button == Barebone.Platform.Inputs.Button.LeftShift || button == Barebone.Platform.Inputs.Button.RightShift)
            {
                _isShiftDown = true;
            }
        }

        public override void OnKeyUp(Barebone.Platform.Inputs.Button button)
        {
            if (button == Barebone.Platform.Inputs.Button.LeftShift || button == Barebone.Platform.Inputs.Button.RightShift)
            {
                _isShiftDown = false;
            }
        }

        private void ProcessEditingStrokes(KeyStrokeEvent e)
        {
            if (!e.Control)
            {
                switch (e.Button)
                {
                    case Barebone.Platform.Inputs.Button.Enter:
                        {
                            if (!IsMultiLine) return;
                            TypeText("\n");
                            var firstNonWhite = GetFirstNonWhitespace(_textEditor.Lines[_caret.Y - 1], int.MaxValue);
                            if (firstNonWhite != null)
                            {
                                // bring to same indent as previous line.
                                TypeText(new string(' ', firstNonWhite.Value));
                            }
                            break;
                        }
                    case Barebone.Platform.Inputs.Button.Backspace:
                        {
                            if (IsSelectionEmpty())
                            {
                                var from = _textEditor.StepBackwards(_caret, false);
                                _textEditor.RemoveTextRange(from, _caret);
                                MoveCaretTo(from);
                            }
                            else
                                RemoveSelection();
                            break;
                        }
                    case Barebone.Platform.Inputs.Button.Delete:
                        {
                            if (IsSelectionEmpty())
                            {
                                var to = _textEditor.StepForwards(_caret, false);
                                _textEditor.RemoveTextRange(_caret, to);
                            }
                            else
                                RemoveSelection();
                            break;
                        }
                    case Barebone.Platform.Inputs.Button.Tab:
                        {
                            if (!AllowTabIndent) return;
                            var count = TabIndentSize - _caret.X % TabIndentSize;
                            TypeText(new(' ', count));
                            break;
                        }
                    case Barebone.Platform.Inputs.Button.Escape:
                        {
                            ClearSelection();
                            break;
                        }
                }
            }
            else
            {
                // Ctrl + ... 
                if (e.Control)
                {
                    switch (e.Button)
                    {
                        case Barebone.Platform.Inputs.Button.A: SelectAll(); break;
                        case Barebone.Platform.Inputs.Button.X: CutSelectionToClipboard(); break;
                        case Barebone.Platform.Inputs.Button.C: CopySelectionToClipboard(); break;
                        case Barebone.Platform.Inputs.Button.V: PasteClipboard(); break;
                        case Barebone.Platform.Inputs.Button.Z: Undo(); break;
                        case Barebone.Platform.Inputs.Button.Y: Redo(); break;
                    }
                }
            }
        }

        private void ProcessNavigationStrokes(KeyStrokeEvent e)
        {
            switch (e.Button)
            {
                case Barebone.Platform.Inputs.Button.Up:
                    MoveCaretTo(new(_caret.X, _caret.Y - 1));
                    break;
                case Barebone.Platform.Inputs.Button.Down:
                    MoveCaretTo(new(_caret.X, _caret.Y + 1));
                    break;
                case Barebone.Platform.Inputs.Button.Left:
                    if (!IsUserSelecting() && !IsSelectionEmpty())
                    {
                        GetSelectionRange(out var begin, out _);
                        MoveCaretTo(begin); // jump to begin of selection. This will also clear the selection. Just like in VS.NET
                    }
                    else
                    {
                        MoveCaretTo(_textEditor.StepBackwards(_caret, e.Control));
                    }
                    break;
                case Barebone.Platform.Inputs.Button.Right:
                    if (!IsUserSelecting() && !IsSelectionEmpty())
                    {
                        GetSelectionRange(out _, out var end);
                        MoveCaretTo(end); // jump to end of selection. This will also clear the selection. Just like in VS.NET
                    }
                    else
                    {
                        MoveCaretTo(_textEditor.StepForwards(_caret, e.Control));
                    }
                    break;
                case Barebone.Platform.Inputs.Button.End:
                    if (e.Control)
                        MoveCaretTo(new(int.MaxValue, int.MaxValue));
                    else
                        MoveCaretTo(_caret with { X = int.MaxValue });
                    break;
                case Barebone.Platform.Inputs.Button.Home:
                    if (e.Control)
                        MoveCaretTo(new(0, 0));
                    else
                        MoveCaretTo(FindStartOfLine(_caret));
                    break;
            }
        }

        private Vector2I FindStartOfLine(Vector2I caret)
        {
            var line = _textEditor.Lines[caret.Y];
            var x = caret.X;
            if (x == 0)
                return new(0, caret.Y);
            var firstNonWhitespace = GetFirstNonWhitespace(line, x);
            return firstNonWhitespace.HasValue
                ? new(firstNonWhitespace.Value, caret.Y)
                : new(0, caret.Y);
        }

        private static int? GetFirstNonWhitespace(string line, int maxX)
        {
            for (var i = 0; i < maxX; i++)
            {
                if (line.Length == i) return null;
                if (!char.IsWhiteSpace(line[i]))
                    return i;
            }
            return null;
        }

        private bool IsSelectionEmpty()
        {
            GetSelectionRange(out var begin, out var end);
            return begin == end;
        }

        private void ClearSelection()
        {
            _selectBegin = _caret;
            _selectEnd = _caret;
            InvalidateVisual();
        }

        private void SelectAll()
        {
            _selectBegin = new Vector2I(0, 0);
            _selectEnd = _textEditor.GetEndOfText();
            _caret = _selectEnd;
            InvalidateVisual();
        }

        public void Undo()
        {
            if (_textEditor.TryUndo())
                ResetCaretBlink();
        }

        public void Redo()
        {
            if (_textEditor.TryRedo())
                ResetCaretBlink();
        }

        private void ResetCaretBlink()
        {
            _caretBlinkStart = UI.Clock.Now;
        }

        private void RemoveSelection()
        {
            if (IsSelectionEmpty()) return;

            GetSelectionRange(out var begin, out var end);

            _textEditor.RemoveTextRange(begin, end);
            ClearSelection();
            MoveCaretTo(begin);
        }

        private void CutSelectionToClipboard()
        {
            if (IsSelectionEmpty()) return;

            GetSelectionRange(out var begin, out var end);

            var text = _textEditor.GetTextRange(begin, end);
            UI.Clipboard.SetText(text);
            RemoveSelection();
        }

        private void CopySelectionToClipboard()
        {
            if (IsSelectionEmpty()) return;

            GetSelectionRange(out var begin, out var end);

            var text = _textEditor.GetTextRange(begin, end);
            UI.Clipboard.SetText(text);
        }

        public void PasteClipboard()
        {
            var text = UI.Clipboard.GetText();
            if (text != null)
            {
                TypeText(text);
            }
        }

        /// <summary>
        /// Inserts text at the caret with newline support.
        /// </summary>
        private void TypeText(string text)
        {
            GetSelectionRange(out var begin, out var end);

            var caret = _textEditor.ReplaceTextRange(begin, end, text);
            MoveCaretTo(caret, false);
        }

        private bool IsUserSelecting() => _isMouseLeftDown || _isShiftDown;

        protected override void OnMouseButtonChange(Vector2I position, MouseButton button, ButtonState state)
        {
            if (button == MouseButton.Left && state == ButtonState.Pressed)
            {
                UI.CaptureMouse(this);
                ClearSelection();
                MoveCaretTo(ScreenToTextIndex(position));
                _isMouseLeftDown = true;
            }
            else if (button == MouseButton.Left && state == ButtonState.Released)
            {
                _isMouseLeftDown = false;
                UI.ReleaseMouse();
            }
        }

        public override void OnMouseMove(Vector2I previousPosition, Vector2I position)
        {
            if (_isMouseLeftDown)
            {
                MoveCaretTo(ScreenToTextIndex(position)); // this also adjust the selectionEnd.
            }
        }

        /// <summary>
        /// Gets the text index from a global screen position.
        /// </summary>
        public Vector2I ScreenToTextIndex(Vector2I position)
        {
            var localPosition = position - Viewport.MinCorner - _textOffset;
            var y = localPosition.Y / Font.LineHeight + TopIndexActual;

            // coerce into valid range:
            y = Math.Min(_textEditor.LineCount - 1, Math.Max(0, y));

            var x = Font.GetCharacterAtCaretX(_textEditor.Lines[y], localPosition.X);
            return new(x, y);
        }

        /// <summary>
        /// Gets the global screen position of the bottom-left of the character at the given `textIndex`. Returns null if `textIndex` is not a valid text location.
        /// </summary>
        public Vector2I? TextIndexToScreen(Vector2I textIndex)
        {
            if (textIndex.Y < 0 || textIndex.Y >= _textEditor.LineCount) return null;

            var line = _textEditor.Lines[textIndex.Y];
            if (textIndex.X < 0 || textIndex.X > line.Length) return null;

            var lineStart = new Vector2I(0, (textIndex.Y - TopIndexActual) * _font.LineHeight);
            var p = Viewport.MinCorner + _textOffset + lineStart + _font.Measure(line.AsSpan(0, textIndex.X));

            return Viewport.Grow(Font.LineHeight).Contains(p) ? p : null;
        }

        /// <summary>
        /// Moves the caret to the closest valid position from `position` in the text. 
        /// </summary>
        /// <param name="growSelection">Changes the selection as the user moves the caret. If false, the selection is reset to empty below the caret</param>
        private void MoveCaretTo(Vector2I position, bool growSelection = true)
        {
            // coerce Y into valid range:
            position.Y = Math.Min(_textEditor.LineCount - 1, Math.Max(0, position.Y));

            // coerce X into valid range:
            var lineLength = _textEditor.Lines[position.Y].Length;
            var x = Math.Min(lineLength, Math.Max(0, position.X));

            _caret = new Vector2I(x, position.Y);

            // technically we're always updating the selectrange, but we keep selection length 0 if user is not selecting.
            _selectEnd = _caret;
            if (!IsUserSelecting() || !growSelection)
                _selectBegin = _caret;

            InvalidateVisual();
            ResetCaretBlink();
        }

        public void SetText(string text, bool resetUndoHistory = true)
        {
            _textEditor.SetFullText(text.Replace("\r", "").Replace("\t", ""), resetUndoHistory);
            ClearSelection();
        }

        /// <summary>
        /// Clears the text and undo history.
        /// </summary>
        public void Clear()
        {
            SetText(string.Empty);
        }

        public string Text
        {
            get => _textEditor.GetFullText();
            set => SetText(value, true);
        }

        public override void Dispose()
        {
            _caretMesh.Return();
            base.Dispose();
        }

        /// <summary>
        /// Desired line index to be at the top of the textbox. Actuals may differ depending on how many lines there are and how many there can be shown.
        /// </summary>
        public int TopIndex
        {
            get;
            set
            {
                if (field == value) return;

                field = value;
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Index of the highest visible line in the viewport of the textbox.
        /// </summary>
        public int TopIndexActual { get; private set; }

        /// <summary>
        /// Index of the lowest visible line in the viewport of the textbox.
        /// </summary>
        public int BottomIndexActual { get; private set; }

        public Color TextColor
        {
            get;
            set
            {
                if (field == value) return;

                field = value;
                InvalidateVisual();
            }
        }

        public Color SelectionColor { get; set; } = PaletteApollo.Pink2;

        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Code-editor style tab indent size. AllowTabIndent must be True.
        /// </summary>
        public int TabIndentSize { get; set; } = 2;

        public Font Font
        {
            get => _font;
            set
            {
                if (_font == value) return;

                _font = value;
                InvalidateVisual();
            }
        }

        public int LineCount => _textEditor.LineCount;

        /// <summary>
        /// Raised at each change of the text.
        /// </summary>
        public event Action? TextChanged;

        /// <summary>
        /// Allows the textbox to hold more than 1 line of text by pressing Enter, or pasting in text with newlines.
        /// </summary>
        public bool IsMultiLine
        {
            get => _textEditor.IsMultiLine;
            set => _textEditor.IsMultiLine = value;
        }

        /// <summary>
        /// Enables to indent by pressing tab. Like in a code editor.
        /// </summary>
        public bool AllowTabIndent { get; set; }

    }
}