using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackCommon.Exceptions
{
    /// <summary>
    /// Exception thrown when a user attempts an operation they're not authorized to perform.
    /// </summary>
    public class AccessDeniedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the AccessDeniedException class.
        /// </summary>
        public AccessDeniedException() : base("You do not have permission to perform this operation.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the AccessDeniedException class with a specific message.
        /// </summary>
        /// <param name="message">The exception message</param>
        public AccessDeniedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AccessDeniedException class with a specific message and inner exception.
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception</param>
        public AccessDeniedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
