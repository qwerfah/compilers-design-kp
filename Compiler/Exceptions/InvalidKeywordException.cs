using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Exceptions
{
    /// <summary>
    /// Throws when an unregistered keyword occurs.
    /// </summary>
    class InvalidKeywordException : Exception
    {
        public InvalidKeywordException()
        { }

        public InvalidKeywordException(string message)
            : base(message)
        { }

        public InvalidKeywordException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
