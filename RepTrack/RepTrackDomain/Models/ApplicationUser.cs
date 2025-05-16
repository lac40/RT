using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace RepTrackDomain.Models
{
    /// <summary>
    /// Represents a user in the application.
    /// Extends the IdentityUser from ASP.NET Core Identity.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Date when the user registered
        /// </summary>
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Whether the user account is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Navigation property for the user's workout sessions
        /// </summary>
        public virtual ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
    }
}
