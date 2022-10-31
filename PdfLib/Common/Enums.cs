using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WF.PdfLib.Common
{
    /// <summary>
    /// Options chosen to deal with annotations found in a document.
    /// </summary>
    public enum AnnotationOption
    {
        /// <summary>
        /// Keep all annotations intact.
        /// </summary>
        Keep,
        /// <summary>
        /// Remove annotations by specific users.
        /// </summary>
        RemoveUser,
        /// <summary>
        /// Remove all annotations.
        /// </summary>
        RemoveAll
    }
}
