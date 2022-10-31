using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX.Common.Base
{
    /// <summary>
    /// A base class for classes containing an id property and creation date.
    /// </summary>
    public abstract class IdClass
    {
        /// <summary>
        /// The unique id of the instance.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Time of instance creation.
        /// </summary>
        public DateTime CreationDate { get; }

        /// <summary>
        /// Create a new id class.
        /// </summary>
        /// <param name="id">Id of the instance.</param>
        /// <param name="creationDate">Datetime of the creation of this instance.</param>
        public IdClass(Guid id, DateTime creationDate)
        {
            Id = id;
            CreationDate = creationDate;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is IdClass @class &&
                   Id.Equals(@class.Id);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }
    }
}
