using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Variable
{
    /// <summary>
    /// Represents function argument definition.
    /// </summary>
    class FunctionParamSymbol : VariableSymbolBase
    {
        public FunctionParamSymbol(ParamContext context, Scope scope)
            : base(context, scope)
        {

        }
    }
}
