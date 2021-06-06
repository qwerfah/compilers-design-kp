using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SymbolTable.Symbol
{
    /// <summary>
    /// Symbol type.
    /// </summary>
    public enum SymbolType
    {
        /// <summary>
        /// Variable name symbol.
        /// </summary>
        Variable,
        /// <summary>
        /// Function name symbol.
        /// </summary>
        Function,
        /// <summary>
        /// Class name symbol.
        /// </summary>
        Class,
        /// <summary>
        /// Object name symbol.
        /// </summary>
        Object,
        /// <summary>
        /// Trait name symbol.
        /// </summary>
        Trait,
        /// <summary>
        /// Type name symbol.
        /// </summary>
        Type
    }
}
