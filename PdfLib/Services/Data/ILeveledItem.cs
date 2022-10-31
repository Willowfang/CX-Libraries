using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.PdfLib.Services.Data
{
    /// <summary>
    /// An object that has a property indicating its level in a hierarchy.
    /// </summary>
    public interface ILeveledItem
    {
        /// <summary>
        /// Level of the item in the hierarchy.
        /// </summary>
        public int Level { get; set; }
    }
}
