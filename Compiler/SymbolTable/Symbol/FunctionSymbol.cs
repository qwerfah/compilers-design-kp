using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol
{
    public class FunctionSymbol : SymbolBase
    {
        public FunctionSymbol(FunDefContext context, Scope scope) : base(context, scope)
        {

        }

        public override void Resolve()
        {
            throw new NotImplementedException();
        }
    }
}
