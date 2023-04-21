using System.IO;

namespace WF.PdfLib.Extensions
{
    /// <summary>
    /// Enum describing the type of a placeholder string (e.g. "[bookmark]").
    /// </summary>
    public enum Placeholders
    {
        /// <summary>
        /// Placeholder for bookmark name.
        /// </summary>
        Bookmark,
        /// <summary>
        /// Placeholder for file name.
        /// </summary>
        File,
        /// <summary>
        /// Placeholder for automated numbering.
        /// </summary>
        Number
    }

    public static class StringExtensions
    {
        /// <summary>
        /// Replace all non-allowed characters in a string.
        /// </summary>
        /// <param name="original">String to handle.</param>
        /// <returns>A string with non-allowed characters replaced with empty characters.</returns>
        public static string ReplaceIllegal(this string original)
        {
            string processed = original.Replace(":", "");
            processed = processed.Replace("/", "-");
            processed = processed.Replace("\"", "'");
            return string.Join("", processed.Split(Path.GetInvalidFileNameChars()));
        }

        /// <summary>
        /// Replace a placeholder name string in a name template.
        /// </summary>
        /// <param name="template">Template string to apply this action on.</param>
        /// <param name="placeholder">Placeholder to replace.</param>
        /// <param name="replacement">What to replace the placeholder with.</param>
        /// <returns>String with replacements.</returns>
        public static string ReplacePlaceholder(
            this string template,
            Placeholders placeholder,
            string replacement)
        {
            string placeholderName = $"[{placeholder}]";

            // Get correct cultural string representation of the placeholder.

            string product = template.Replace(placeholderName, replacement, System.StringComparison.OrdinalIgnoreCase);

            return product;
        }
    }
}
