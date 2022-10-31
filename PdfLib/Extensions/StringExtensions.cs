using System.IO;

namespace WF.PdfLib.Extensions
{
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
            return string.Join("", processed.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
