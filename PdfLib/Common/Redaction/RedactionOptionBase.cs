using System.Text.RegularExpressions;

namespace WF.PdfLib.Common.Redaction
{
    public abstract class RedactionOptionBase : IRedactionOption
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RedactColor Outline { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public RedactColor Fill { get; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool Apply { get; }

        public RedactionOptionBase(
            bool apply, 
            RedactColor? outline, 
            RedactColor? fill)
        {
            Outline = outline ?? RedactColor.Red;
            Fill = fill ?? RedactColor.Black;
            Apply = apply;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public abstract Regex GetRegex();
    }
}
