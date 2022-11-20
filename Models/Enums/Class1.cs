using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Enums
{
    public enum Status
    {
        OK, InitializationError, ConnectionError, UnexpectedError, FeedbackError
    }

    /// <summary>
    /// TCP ASCII code
    /// </summary>
    public enum Symbol
    {
        /// <summary>
        /// End of Line or Newline
        /// </summary>
        EOL,

        /// <summary>
        /// End Of Transmission
        /// </summary>
        EOT
    }
}
