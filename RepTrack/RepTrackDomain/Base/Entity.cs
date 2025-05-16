using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackDomain.Base
{
    /// <summary>
    /// Base class for all domain entities.
    /// Provides common properties and functionality.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Unique identifier for the entity
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        /// Date and time when the entity was created
        /// </summary>
        public DateTime CreatedAt { get; protected set; } = DateTime.Now;

        /// <summary>
        /// Date and time when the entity was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; protected set; }

        /// <summary>
        /// Updates the UpdatedAt timestamp to the current time
        /// </summary>
        protected void SetUpdated()
        {
            UpdatedAt = DateTime.Now;
        }
    }
}
