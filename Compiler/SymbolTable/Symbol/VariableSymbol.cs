using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol
{
    public class VariableSymbol : SymbolBase
    {
        public static Type[] AcceptableContextTypes = new[] 
        { 
            typeof(PatVarDefContext), 
            typeof(ParamContext), 
            typeof(ClassParamContext) 
        };

        public VariableSymbol(ParserRuleContext context, Scope scope) : base(context, scope)
        {

        }

        public override void Resolve()
        {
            throw new NotImplementedException();
        }
    }
}
