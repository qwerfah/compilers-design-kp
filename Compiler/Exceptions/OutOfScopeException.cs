using System;

namespace Compiler.Exceptions
{
    /// <summary>
    /// Throws during building symbol table when trying to get current scope beyond any scope.
    /// </summary>
    [Serializable]
    public class OutOfScopeException : Exception
    {
        public OutOfScopeException()
        { }

        public OutOfScopeException(string message)
            : base(message)
        { }

        public OutOfScopeException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
