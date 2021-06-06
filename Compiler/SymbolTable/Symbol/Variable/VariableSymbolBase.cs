using Antlr4.Runtime;
using Compiler.SymbolTable.Symbol.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Variable
{
    /// <summary>
    /// Abstract class for any variable definition 
    /// (local variable, class field, class param or function argument).
    /// </summary>
    public abstract class VariableSymbolBase : SymbolBase
    {
        /// <summary>
        /// Shows if variable is defined as mutable (var) or immutable (val).
        /// </summary>
        public bool Mutable { get; set; } = false;

        /// <summary>
        /// Reference to symbol that represents variable type.
        /// Any type represented by class, trait, template or type definition.
        /// If type is not stated in variable definition it will be deducted by value.
        /// </summary>
        public SymbolBase Type { get; set; }

        /// <summary>
        /// Variable value (if stated in definition).
        /// </summary>
        public string Value { get; set; }

        public VariableSymbolBase(ParserRuleContext context, Scope scope) : base(context, scope)
        {

        }

        public override void Resolve()
        {
            throw new NotImplementedException();
        }
    }
}
