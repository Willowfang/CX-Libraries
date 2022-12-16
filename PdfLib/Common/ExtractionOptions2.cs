using System.Collections.Generic;
using System.Threading;

namespace WF.PdfLib.Common
{
    public class ExtractionOptions2
    {
        public CancellationToken Token { get; }

        public bool ConvertToPdfA { get; }

        public AnnotationOption Annotations { get; }

        public IEnumerable<string> AnnotationUsersToRemove { get; }

        public ExtractionOptions2(
            CancellationToken token = default(CancellationToken), 
            bool convertToPdfA = false, 
            AnnotationOption annotations = AnnotationOption.Keep, 
            IEnumerable<string> annotationUsersToRemove = null)
        {
            Token = token;
            ConvertToPdfA = convertToPdfA;
            Annotations = annotations;
            AnnotationUsersToRemove = annotationUsersToRemove ?? new List<string>();
        }
    }
}
