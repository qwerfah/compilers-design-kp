using System;

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
