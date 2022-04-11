using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CX.PdfLib.Services
{
    public interface IAnnotationService
    {
        public Task<IEnumerable<string>> GetTitles(string inputPath);
        public Task<IEnumerable<string>> GetTitles(string inputPath,
            CancellationToken token);

        public Task RemoveAll(string inputPath);
        public Task RemoveAll(string inputPath, CancellationToken token);

        public Task RemoveByTitle(IEnumerable<string> titles, string inputPath);
        public Task RemoveByTitle(IEnumerable<string> titles, string inputPath,
            CancellationToken token);

        public Task FlattenRedactions(string inputPath);
        public Task FlattenRedactions(string inputPath, CancellationToken token);
    }
}
