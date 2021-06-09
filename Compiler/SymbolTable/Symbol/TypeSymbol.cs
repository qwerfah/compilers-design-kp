using Antlr4.Runtime;
using Compiler.SymbolTable.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol
{
    public class TypeSymbol : SymbolBase
    {
        public TypeSymbol(TypeDefContext context, Scope scope) : base(context, scope)
        {

        }

        public override void Resolve()
        {
            throw new NotImplementedException();
        }
    }
}
