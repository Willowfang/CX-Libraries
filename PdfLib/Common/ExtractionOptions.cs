using CX.PdfLib.Services.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.Common
{
    public class PdfAConversionEventArgs : EventArgs
    {
        public bool WasFaulted { get; set; }
        public string Path { get; set; }
    }

    public class FileAndExtractables
    {
        public string FilePath { get; }
        public IEnumerable<ILeveledBookmark> Extractables { get; }

        public FileAndExtractables(string filePath, IEnumerable<ILeveledBookmark> extractables)
        {
            FilePath = filePath;
            Extractables = extractables;
        }
    }

    public class ExtractionOptions
    {
        public event EventHandler<PdfAConversionEventArgs> PdfAConversionFinished;

        public IEnumerable<FileAndExtractables> Files { get; }
        public FileSystemInfo Destination { get; }
        public IProgress<ProgressReport> Progress { get; set; }
        [JsonIgnore]
        public CancellationToken Cancellation { get; set; }
        public bool PdfA { get; set; }
        public AnnotationOption Annotations { get; set; }

        public ExtractionOptions(IEnumerable<FileAndExtractables> files, FileSystemInfo destination)
        {
            Files = files;
            Destination = destination;
            Progress = null;
            Cancellation = CancellationToken.None;
            PdfA = false;
            Annotations = AnnotationOption.Keep;
        }

        public virtual void OnPdfAConversionFinished(PdfAConversionEventArgs e)
        {
            EventHandler<PdfAConversionEventArgs> handler = PdfAConversionFinished;
            handler?.Invoke(this, e);
        }
    }
}
