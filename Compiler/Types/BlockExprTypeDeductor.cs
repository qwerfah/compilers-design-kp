using Antlr4.Runtime.Misc;
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
    class BlockExprTypeDeductor : ScalaBaseVisitor<SymbolBase>
    {
        /// <summary>
        /// Contains all functions that call in current expression.
        /// Uses in call graph builder
        /// </summary>
        public List<FunctionSymbol> Calls { get; } = new();

        private Scope _scope;

        public SymbolBase Deduct(BlockExprContext context, Scope scope)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = scope ?? throw new ArgumentNullException(nameof(scope));

            _scope = scope;

            return Visit(context);
        }

        public override SymbolBase VisitExpr([NotNull] ExprContext context)
        {
            ExprTypeDeductor deductor = new();
            SymbolBase symbol = deductor.Deduct(context, _scope);

            Calls.AddRange(deductor.Calls);

            return symbol;
        }
    }
}
