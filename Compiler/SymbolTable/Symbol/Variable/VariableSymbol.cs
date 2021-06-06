using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Variable
{
    /// <summary>
    /// Represents local variable or class field definition.
    /// </summary>
    class VariableSymbol : VariableSymbolBase
    {
        public VariableSymbol(PatVarDefContext context, Scope scope)
            : base(context, scope)
        {

        }
    }
}
