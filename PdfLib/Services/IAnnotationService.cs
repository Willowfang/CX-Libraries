using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    /// <summary>
    /// Service for dealing with annotations in a pdf document.
    /// </summary>
    public interface IAnnotationService
    {
        /// <summary>
        /// Retrieve the titles of annotations in a document.
        /// </summary>
        /// <param name="inputPath">File to retrieve the annotation titles from.</param>
        /// <returns>Titles of found annotations.</returns>
        public Task<IEnumerable<string>> GetTitles(string inputPath);

        /// <summary>
        /// Retrieve the titles of annotations in a document.
        /// </summary>
        /// <param name="inputPath">File to retrieve the annotation titles from.</param>
        /// <param name="token">Token for cancellation of current task.</param>
        /// <returns>Titles of found annotations.</returns>
        public Task<IEnumerable<string>> GetTitles(string inputPath,
            CancellationToken token);

        /// <summary>
        /// Remove all annotations from a document.
        /// </summary>
        /// <param name="inputPath">File to remove annotations from.</param>
        /// <returns>An awaitable task.</returns>
        public Task RemoveAll(string inputPath);

        /// <summary>
        /// Remove all annotations from a document.
        /// </summary>
        /// <param name="inputPath">File to remove annotations from.</param>
        /// <param name="token">A token for the cancellation of the current task.</param>
        /// <returns>An awaitable task.</returns>
        public Task RemoveAll(string inputPath, CancellationToken token);

        /// <summary>
        /// Remove all annotations with any of the given titles.
        /// </summary>
        /// <param name="titles">Annotations with any of these titles will be removed.</param>
        /// <param name="inputPath">File to remove annotations from.</param>
        /// <returns>An awaitable task.</returns>
        public Task RemoveByTitle(IEnumerable<string> titles, string inputPath);

        /// <summary>
        /// Remove all annotations with any of the given titles.
        /// </summary>
        /// <param name="titles">Annotations with any of these titles will be removed.</param>
        /// <param name="inputPath">File to remove annotations from.</param>
        /// <param name="token">Token for the cancellation of current task.</param>
        /// <returns>An awaitable task.</returns>
        public Task RemoveByTitle(IEnumerable<string> titles, string inputPath,
            CancellationToken token);

        /// <summary>
        /// Flatten all redaction annotations in a document (and replace them with red rectangles).
        /// </summary>
        /// <param name="inputPath">File to flatten.</param>
        /// <returns>An awaitable task.</returns>
        public Task FlattenRedactions(string inputPath);

        /// <summary>
        /// Flatten all redaction annotations in a document (and replace them with red rectangles).
        /// </summary>
        /// <param name="inputPath">File to flatten.</param>
        /// <param name="token">Cancellation token for the current task.</param>
        /// <returns>An awaitable task.</returns>
        public Task FlattenRedactions(string inputPath, CancellationToken token);
    }
}
