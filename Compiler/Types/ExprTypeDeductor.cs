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
        public HashSet<FunctionSymbol> Calls { get; private set; } = new();

        public SymbolBase Deduct(ExprContext context, Scope scope)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = scope ?? throw new ArgumentNullException(nameof(scope));

            if (context.expr1()?.postfixExpr()?.infixExpr()
                ?.prefixExpr()?.simpleExpr()?.blockExpr() is { } blockExpr)
            {
                BlockExprTypeDeductor deductor = new();
                SymbolBase symbol = deductor.Deduct(blockExpr, scope);

                Calls = Calls.Union(deductor.Calls).ToHashSet();

                return symbol;
            }
            if (context.expr1()?.postfixExpr()?.infixExpr()?.prefixExpr() 
                is { } prefixExpr)
            {
                PrefixExprTypeDeductor deductor = new();
                SymbolBase symbol = deductor.Deduct(prefixExpr, scope);

                Calls = Calls.Union(deductor.Calls).ToHashSet();

                return symbol;
            }
            else if (context.expr1()?.postfixExpr()?.infixExpr() is { } infixExpr)
            {
                InfixExprTypeDeductor deductor = new();
                SymbolBase symbol = deductor.Deduct(infixExpr, scope);

                Calls = Calls.Union(deductor.Calls).ToHashSet();

                return symbol;
            }

            throw new InvalidSyntaxException(
                "Invalid expression: prefix or infix notation expected.");
        }
    }
}
