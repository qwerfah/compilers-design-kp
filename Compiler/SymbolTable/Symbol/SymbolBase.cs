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
    /// Represents symbol (variable/function/class/object/trait/type definition) in symbol table.
    /// </summary>
    public abstract class SymbolBase
    {
        /// <summary>
        /// Unique symbol identifier.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// String symbol name.
        /// </summary>
        public string Name { get; init; }
        
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
        public Scope Scope { get; set; }

        /// <summary>
        /// Construct new symbol from specified rule context in stated scope.
        /// </summary>
        /// <param name="context"> Parse rule context of symbol in parse tree. </param>
        /// <param name="type"> Symbol type (variable/function/class). </param>
        /// <param name="scope"> Symbol scope according to parse tree lookup. </param>
        public SymbolBase(ParserRuleContext context, Scope scope)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            Guid = Guid.NewGuid();
            ContextType = context.GetType();
            Definition = context.GetText();
            Scope = scope;
        }

        /// <summary>
        /// Get name of symbol from its definition represented by parse subtree.
        /// </summary>
        /// <param name="context"> Root of parse subtree that represents symbol definition. </param>
        /// <returns> Symbol name. </returns>
        private string GetName(ParserRuleContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.ChildCount == 0 && context is StableIdContext stableId)
            {
                return stableId.GetText();
            }

            for (int i = 0; i < context.ChildCount; i++)
            {
                GetName(context.GetChild(i) as ParserRuleContext);
            }

            throw new ArgumentException("Given node has no StableIdContext as a child.");
        }

        /// <summary>
        /// Resolve all unresolved symbols in current symbol definition.
        /// </summary>
        public abstract void Resolve();
    }
}
