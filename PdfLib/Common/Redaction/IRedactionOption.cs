using System.Drawing;
using System.Text.RegularExpressions;

public class RedactColor
{
    public string Hex { get; }

    /// <summary>
    /// Create a color for redaction.
    /// </summary>
    /// <param name="hex">Hex color code for this color.</param>
    public RedactColor(string hex)
    {
        Hex = hex.ToUpper();
    }

    /// <summary>
    /// Get this color as an instance of System.Drawing.Color.
    /// </summary>
    /// <returns></returns>
    public Color GetColor()
    {
        return ColorTranslator.FromHtml(Hex);
    }

    /// <summary>
    /// Get this color as a float array (r, g, b).
    /// </summary>
    /// <returns></returns>
    public float[] GetFloat()
    {
        Color c = GetColor();
        return new float[] { c.R, c.G, c.B };
    }

    /// <summary>
    /// A red instance.
    /// </summary>
    public static RedactColor Red
    {
        get => new RedactColor("#FF0000");
    }

    /// <summary>
    /// A black instance.
    /// </summary>
    public static RedactColor Black
    {
        get => new RedactColor("#000000");
    }
}

namespace WF.PdfLib.Common.Redaction
{
    /// <summary>
    /// Interface for options used when making redactions.
    /// </summary>
    public interface IRedactionOption
    {
        /// <summary>
        /// Outline color for the redactions.
        /// </summary>
        public RedactColor Outline { get; }

        /// <summary>
        /// Fill color for the redactions.
        /// </summary>
        public RedactColor Fill { get; }

        /// <summary>
        /// If true, apply redactions once they have been created.
        /// </summary>
        public bool Apply { get; }

        /// <summary>
        /// Get given search criteria as a Regex object.
        /// </summary>
        /// <returns></returns>
        public Regex GetRegex();
    }
}
