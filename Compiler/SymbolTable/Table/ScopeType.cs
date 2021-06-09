using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SymbolTable.Table
{
    /// <summary>
    /// Scope type (global/local).
    /// </summary>
    public enum ScopeType
    {
        /// <summary>
        /// Global scope (package).
        /// </summary>
        Global,

        /// <summary>
        /// Local scope (class, function or block).
        /// </summary>
        Local
    }
}
