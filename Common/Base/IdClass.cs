using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.Common.Base
{
    public abstract class IdClass
    {
        public Guid Id { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
