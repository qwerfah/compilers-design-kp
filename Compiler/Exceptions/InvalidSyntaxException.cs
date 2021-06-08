using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Exceptions
{
    /// <summary>
    /// Throws if actial parse tree structure does not corresponds to expected structure
    /// (there are no some nodes in tree that are expected to be) due to the syntax error.
    /// </summary>
    class InvalidSyntaxException : Exception
    {
        public InvalidSyntaxException()
        { }

        public InvalidSyntaxException(string message)
            : base(message)
        { }

        public InvalidSyntaxException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
