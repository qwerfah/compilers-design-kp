using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Class
{
    public class TraitSymbol : ClassSymbolBase
    {
        public TraitSymbol(TraitDefContext context, Scope scope) : base(context, scope)
        {

        }
    }
}
