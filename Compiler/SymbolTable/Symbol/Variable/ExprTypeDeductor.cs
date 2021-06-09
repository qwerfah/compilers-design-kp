using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Parser.Antlr.Grammar;
using Parser.Antlr.TreeLookup.Impls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Variable
{
    public class ExprTypeDeductor : ScalaBaseVisitor<string>
    {
        public List<Tuple<string, SymbolType>> Symbols { get; } = new();

        public override string VisitSimpleExpr1([NotNull] SimpleExpr1Context context)
        {
            string name = context.children
                .SingleOrDefault(ch => ch is TerminalNodeImpl t && t.GetText() != ".")
                ?.GetText();

            if (name is not null)
            {
                Symbols.Add(new(name, SymbolType.Variable));
            }

            return base.VisitSimpleExpr1(context);
        }

        public override string VisitStableId([NotNull] StableIdContext context)
        {
            return base.VisitStableId(context);
        }
    }
}
