using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Table;
using Parser.Antlr.TreeLookup.Impls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.Types
{
    class ExprTypeDeductor
    {
        public SymbolBase Deduct(ExprContext context, Scope scope)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = scope ?? throw new ArgumentNullException(nameof(scope));

            return context switch
            {
                _ when context.expr1()?.postfixExpr()?.infixExpr()?.prefixExpr() 
                    is { } prefixExpr => new PrefixExprTypeDeductor().Deduct(prefixExpr, scope),
                _ when context.expr1()?.postfixExpr()?.infixExpr() 
                    is { } infixExpr => new InfixExprTypeDeductor().Deduct(infixExpr, scope),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
