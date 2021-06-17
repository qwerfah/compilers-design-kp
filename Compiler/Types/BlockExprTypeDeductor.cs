using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
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

        /// <summary>
        /// Block expression definition scope.
        /// </summary>
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

            SymbolBase symbol = Visit(context.block());

            return symbol;
        }

        public override SymbolBase VisitBlock([NotNull] BlockContext context)
        {
            SymbolBase symbol = null;

            foreach (var stat in context.blockStat())
            {
                symbol = Visit(stat);
            }

            return symbol;
        }

        public override SymbolBase VisitBlockStat([NotNull] BlockStatContext context)
        {
            return Visit((IParseTree)context.def() ?? context.expr1());
        }

        /// <summary>
        /// If expression is definition then return type is Unit.
        /// </summary>
        /// <param name="context"> Definition context. </param>
        /// <returns> Unit class symbol. </returns>
        public override SymbolBase VisitDef([NotNull] DefContext context)
        {
            base.VisitDef(context);
            return _scope.GetSymbol("Unit", SymbolType.Class);
        }

        /// <summary>
        /// Deduct expression type and perform typechecking to its elements.
        /// </summary>
        /// <param name="context"> Expression context. </param>
        /// <returns> Expression return value type. </returns>
        public override SymbolBase VisitExpr1([NotNull] Expr1Context context)
        {
            ExprTypeDeductor deductor = new();
            SymbolBase symbol = deductor.Deduct(context, _scope);

            Calls = Calls.Union(deductor.Calls).ToHashSet();

            return symbol;
        }
    }
}
