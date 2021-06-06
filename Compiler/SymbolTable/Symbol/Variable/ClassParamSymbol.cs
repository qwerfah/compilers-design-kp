using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Variable
{
    /// <summary>
    /// Represents class constructor argument that may be also class field.
    /// </summary>
    class ClassParamSymbol : VariableSymbolBase
    {
        public ClassParamSymbol(ClassParamContext context, Scope scope)
            : base(context, scope)
        {

        }
    }
}
