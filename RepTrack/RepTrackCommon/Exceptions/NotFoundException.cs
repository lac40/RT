using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackCommon.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested resource is not found.
    /// </summary>
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the NotFoundException class.
        /// </summary>
        public NotFoundException() : base("The requested resource was not found.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotFoundException class with a specific message.
        /// </summary>
        /// <param name="message">The exception message</param>
        public NotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NotFoundException class with a specific message and inner exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
