using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SymbolTable.Symbol
{
    public class TypeSymbol : SymbolBase
    {
        public TypeSymbol(ParserRuleContext context, Scope scope) : base(context, scope)
        {

        }

        public override void Resolve()
        {
            throw new NotImplementedException();
        }
    }
}
