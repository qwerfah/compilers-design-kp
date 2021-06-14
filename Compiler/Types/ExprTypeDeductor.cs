using Compiler.Exceptions;
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
        /// <summary>
        /// Contains all functions that call in current expression.
        /// Uses in call graph builder
        /// </summary>
        public List<FunctionSymbol> Calls { get; } = new();

        public SymbolBase Deduct(ExprContext context, Scope scope)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = scope ?? throw new ArgumentNullException(nameof(scope));

            if (context.expr1()?.postfixExpr()?.infixExpr()?.prefixExpr() is { } prefixExpr)
            {
                PrefixExprTypeDeductor deductor = new();
                SymbolBase symbol = deductor.Deduct(prefixExpr, scope);

                Calls.AddRange(deductor.Calls);

                return symbol;
            }
            else if (context.expr1()?.postfixExpr()?.infixExpr() is { } infixExpr)
            {
                InfixExprTypeDeductor deductor = new();
                SymbolBase symbol = deductor.Deduct(infixExpr, scope);

                Calls.AddRange(deductor.Calls);

                return symbol;
            }

            throw new InvalidSyntaxException(
                "Invalid expression: prefix or infix notation expected.");
        }
    }
}
