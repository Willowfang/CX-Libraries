using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Service for handling digital signatures in a document.
    /// </summary>
    public interface ISigningService
    {
        /// <summary>
        /// Remove digital signatures from a document.
        /// </summary>
        /// <param name="source">File to remove signatures from.</param>
        /// <param name="destination">File to save the new file as.</param>
        public Task RemoveSignature(FileInfo source, FileInfo destination);
        /// <summary>
        /// Remove digital signatures from a document.
        /// </summary>
        /// <param name="source">File to remove signatures from.</param>
        /// <param name="destination">File to save the new file as.</param>
        /// <param name="token">Cancellation token for the current task.</param>
        public Task RemoveSignature(FileInfo source, FileInfo destination,
            CancellationToken token);
    }
}
