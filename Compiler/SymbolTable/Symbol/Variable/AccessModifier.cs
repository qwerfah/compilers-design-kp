using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SymbolTable.Symbol.Variable
{
    /// <summary>
    /// Class member access modifier.
    /// </summary>
    public enum AccessModifier
    {
        /// <summary>
        /// Public class member.
        /// </summary>
        Public,
        /// <summary>
        /// Private class member.
        /// </summary>
        Private,
        /// <summary>
        /// Protected class member.
        /// </summary>
        Protected
    }
}
