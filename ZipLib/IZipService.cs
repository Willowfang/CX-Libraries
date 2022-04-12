using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.ZipLib
{
    public interface IZipService
    {
        public Task Compress(DirectoryInfo sourceDirectory, FileInfo destinationFile);
    }
}
