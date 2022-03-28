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
    public class MergingOptions
    {
        public IEnumerable<IMergeInput> Inputs { get; }
        public FileInfo OutputFile { get; }
        public bool AddPageNumbers { get; }
        public IProgress<ProgressReport> Progress { get; set; }
        [JsonIgnore]
        public CancellationToken Cancellation { get; set; }

        public MergingOptions(IEnumerable<IMergeInput> inputs, FileInfo outputFile,
            bool addPageNumbers)
        {
            Inputs = inputs;
            OutputFile = outputFile;
            AddPageNumbers = addPageNumbers;
            Progress = null;
            Cancellation = CancellationToken.None;
        }
    }
}
