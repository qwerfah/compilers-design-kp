using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Exceptions
{
    /// <summary>
    /// Throws during building symbol table when tring to get current scope beyond any scope.
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
