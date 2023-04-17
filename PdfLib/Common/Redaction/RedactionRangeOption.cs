using System.Text.RegularExpressions;

namespace WF.PdfLib.Common.Redaction
{
    /// <summary>
    /// Range redaction implementation.
    /// </summary>
    public class RedactionRangeOption : RedactionOptionBase
    {
        private readonly string middleRange = @"([\s\S]*?)";

        private readonly string begin;
        private readonly string end;

        /// <summary>
        /// Redact a range of characters between opening and closing statements.
        /// </summary>
        /// <param name="begin">A string to start the redaction at (will be included in the redaction).</param>
        /// <param name="end">A string to end the redaction with (will be included in the redaction).</param>
        /// <param name="apply">If true, the redaction should be immediately applied.</param>
        /// <param name="outline">Outline for the redactions.</param>
        /// <param name="fill">Fill for the redactions.</param>
        public RedactionRangeOption(
            string begin, 
            string end, 
            bool apply = true,
            RedactColor? outline = null,
            RedactColor? fill = null)
            : base(apply, outline, fill)
        {
            this.begin = begin;
            this.end = end;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override Regex GetRegex()
        {
            string pattern = Regex.Escape(begin) + middleRange + Regex.Escape(end);
            return new Regex(pattern, RegexOptions.IgnoreCase);
        }
    }
}
