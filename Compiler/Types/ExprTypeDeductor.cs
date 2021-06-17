using Antlr4.Runtime.Misc;
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
    class ExprTypeDeductor : ScalaBaseVisitor<SymbolBase>
    {
        /// <summary>
        /// Contains all functions that call in current expression.
        /// Uses in call graph builder
        /// </summary>
        public HashSet<FunctionSymbol> Calls { get; private set; } = new();

        /// <summary>
        /// Expression definition scope.
        /// </summary>
        private Scope _scope;

        public SymbolBase Deduct(Expr1Context context, Scope scope)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = scope ?? throw new ArgumentNullException(nameof(scope));

            _scope = scope;

            return Visit(context);
        }

        public override SymbolBase VisitBlockExpr([NotNull] BlockExprContext context)
        {
            BlockExprTypeDeductor deductor = new();
            SymbolBase symbol = deductor.Deduct(context, _scope);

            Calls = Calls.Union(deductor.Calls).ToHashSet();

            return symbol;
        }

        public override SymbolBase VisitInfixExpr([NotNull] InfixExprContext context)
        {
            if (context.prefixExpr() is { } prefixExpr)
            {
                return base.VisitInfixExpr(context);
            }

            InfixExprTypeDeductor deductor = new();
            SymbolBase symbol = deductor.Deduct(context, _scope);

            Calls = Calls.Union(deductor.Calls).ToHashSet();

            return symbol;
        }

        public override SymbolBase VisitPrefixExpr([NotNull] PrefixExprContext context)
        {
            if (context.simpleExpr()?.blockExpr() is { })
            {
                return base.VisitPrefixExpr(context);
            }

            PrefixExprTypeDeductor deductor = new();
            SymbolBase symbol = deductor.Deduct(context, _scope);

            Calls = Calls.Union(deductor.Calls).ToHashSet();

            return symbol;
        }
    }
}
