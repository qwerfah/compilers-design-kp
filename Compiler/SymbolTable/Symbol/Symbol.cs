using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol
{
    /// <summary>
    /// Represents symbol (variable/function/class/type definition) in symbol table.
    /// </summary>
    public abstract class Symbol
    {
        private const string ContextSuffix = "Context";
        /// <summary>
        /// String symbol name.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Type of parse rule context in parse tree.
        /// </summary>
        public Type ContextType { get; }

        /// <summary>
        /// String representation of symbol definition in code.
        /// </summary>
        public string Definition { get; }

        /// <summary>
        /// Symbol scope in code.
        /// </summary>
        public Scope Scope { get; }

        /// <summary>
        /// Construct new symbol from specified rule context in stated scope.
        /// </summary>
        /// <param name="context"> Parse rule context of symbol in parse tree. </param>
        /// <param name="type"> Symbol type (variable/function/class). </param>
        /// <param name="scope"> Symbol scope according to parse tree lookup. </param>
        public Symbol(ParserRuleContext context, Scope scope)
        {
            ContextType = context.GetType();
            Name = ContextType.Name.Replace(ContextSuffix, string.Empty);
            Definition = context.GetText();
            Scope = scope;
        }
    }
}
