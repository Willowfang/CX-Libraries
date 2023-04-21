using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.PdfLib.Services.Data
{
    public interface IIndexed
    {
        /// <summary>
        /// Index in the sequence of bookmarks, taking placeholders into account.
        /// </summary>
        public int Index { get; set; }
    }
}
