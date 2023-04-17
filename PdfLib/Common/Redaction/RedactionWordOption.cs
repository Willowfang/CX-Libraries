using System.Text.RegularExpressions;

namespace WF.PdfLib.Common.Redaction
{
    /// <summary>
    /// Implementation for options redacting certain words.
    /// </summary>
    public class RedactionWordOption : RedactionOptionBase
    {
        private readonly string nordicAsterisk = @"[\S]*";
        private readonly string questionMark = @"\w";

        private readonly string word;

        /// <summary>
        /// Create new option for redacting certain words or word-patterns from a document.
        /// </summary>
        /// <param name="word">Word to redact. May use wildcard characters (*, ?).</param>
        /// <param name="apply">If true, redactions should be applied immediately.</param>
        /// <param name="outline">Outline for redactions.</param>
        /// <param name="fill">Fill for redactions.</param>
        public RedactionWordOption(
            string word, 
            bool apply = true,
            RedactColor? outline = null,
            RedactColor? fill = null)
            : base(apply, outline, fill)
        {
            this.word = word;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override Regex GetRegex()
        {
            string pattern = word
                .Replace("*", nordicAsterisk)
                .Replace("?", questionMark);
            return new Regex(pattern, RegexOptions.IgnoreCase);
        }
    }
}
