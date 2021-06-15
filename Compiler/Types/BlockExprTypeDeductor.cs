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
    /// <summary>
    /// Block expression return value type deductor.
    /// </summary>
    class BlockExprTypeDeductor : ScalaBaseVisitor<SymbolBase>
    {
        /// <summary>
        /// Contains all functions that call in current expression.
        /// Uses in call graph builder
        /// </summary>
        public HashSet<FunctionSymbol> Calls { get; private set;  } = new();

        private Scope _scope;

        /// <summary>
        /// Deduct return value type for block expression.
        /// Performs type checks in process.
        /// For block it will be return type of its last statement.
        /// </summary>
        /// <param name="context"> Block expression context. </param>
        /// <param name="scope"> Block expression scope. </param>
        /// <returns></returns>
        public SymbolBase Deduct(BlockExprContext context, Scope scope)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = scope ?? throw new ArgumentNullException(nameof(scope));

            _scope = scope;

            return Visit(context);
        }

        /// <summary>
        /// Deduct expression type and perform typechecking to its elements.
        /// </summary>
        /// <param name="context"> Expression context. </param>
        /// <returns> Expression return value type. </returns>
        public override SymbolBase VisitExpr([NotNull] ExprContext context)
        {
            ExprTypeDeductor deductor = new();
            SymbolBase symbol = deductor.Deduct(context, _scope);

            Calls = Calls.Union(deductor.Calls).ToHashSet();

            return symbol;
        }
    }
}
