using Compiler.SymbolTable.Table;
using System;
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
