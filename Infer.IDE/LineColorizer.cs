using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Windows.Media;

namespace Infer.IDE
{
    /// <summary>
    /// Used for highlighting lines in the AvalonEdit editor.
    /// Code adapted from http://community.sharpdevelop.net/.
    /// </summary>
    class LineColorizer : DocumentColorizingTransformer
    {
        int lineNumber;

        public LineColorizer(int lineNumber)
        {
            if (lineNumber < 1)
                throw new ArgumentOutOfRangeException("lineNumber", lineNumber, "Line numbers are 1-based.");
            this.lineNumber = lineNumber;
        }

        public int LineNumber
        {
            get { return lineNumber; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", value, "Line numbers are 1-based.");
                lineNumber = value;
            }
        }

        protected override void ColorizeLine(ICSharpCode.AvalonEdit.Document.DocumentLine line)
        {
            if (!line.IsDeleted && line.LineNumber == lineNumber)
            {
                ChangeLinePart(line.Offset, line.EndOffset, ApplyChanges);
            }
        }

        void ApplyChanges(VisualLineElement element)
        {
            element.TextRunProperties.SetForegroundBrush(Brushes.White);
            element.TextRunProperties.SetBackgroundBrush((SolidColorBrush)(new BrushConverter().ConvertFrom("#418cf0")));
        }
    }
}
