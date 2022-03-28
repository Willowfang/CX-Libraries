using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Service for manipulating digital signatures
    /// </summary>
    public interface ISigningService
    {
        /// <summary>
        /// Remove digital signatures from a document.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public Task RemoveSignature(FileInfo source, FileInfo destination);
        /// <summary>
        /// Remove digital signatures from a document.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="token"></param>
        public Task RemoveSignature(FileInfo source, FileInfo destination,
            CancellationToken token);
    }
}
