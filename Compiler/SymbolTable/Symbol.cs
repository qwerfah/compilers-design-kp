using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SymbolTable
{
    /// <summary>
    /// Represents symbol in symbol table.
    /// </summary>
    public class Symbol
    {
        /// <summary>
        /// String symbol name.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Type of parse tree node that contain that symbol.
        /// </summary>
        public Type NodeType { get; }

        /// <summary>
        /// String token of input stream that contains current symbol.
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Symbol scope in code.
        /// </summary>
        public Scope Scope { get; }

        public Symbol() { }

        public Symbol(string name, Type type, string token, Scope scope)
        {
            Name = name;
            NodeType = type;
            Token = token;
            Scope = scope;

        }
    }
}
