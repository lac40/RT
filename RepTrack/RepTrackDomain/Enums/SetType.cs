using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepTrackDomain.Enums
{
    /// <summary>
    /// Represents the type of exercise set.
    /// </summary>
    public enum SetType
    {
        /// <summary>
        /// Warm-up set with lighter weight
        /// </summary>
        WarmUp = 0,

        /// <summary>
        /// Main working set with heaviest weight
        /// </summary>
        TopSet = 1,

        /// <summary>
        /// Set performed after the top set, usually with reduced weight
        /// </summary>
        BackOff = 2,

        /// <summary>
        /// Set where weight is reduced immediately after reaching failure
        /// </summary>
        DropSet = 3,

        /// <summary>
        /// As Many Reps As Possible
        /// </summary>
        AMRAP = 4,

        /// <summary>
        /// Standard working set
        /// </summary>
        Regular = 5
    }
}
