using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Exceptions
{
    /// <summary>
    /// Throws if actial parse tree structure does not corresponds to expected structure
    /// (there are no some nodes in tree that are expected to be).
    /// </summary>
    class InvalidParseTreeException : Exception
    {
        public InvalidParseTreeException()
        { }

        public InvalidParseTreeException(string message)
            : base(message)
        { }

        public InvalidParseTreeException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
