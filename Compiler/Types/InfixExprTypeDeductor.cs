using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Symbol.Class;
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
    class InfixExprTypeDeductor : ScalaBaseVisitor<SymbolBase>
    {
        public SymbolBase Deduct(InfixExprContext context, Scope scope)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = scope ?? throw new ArgumentNullException(nameof(scope));

            if ((context.ChildCount == 3) && (context.infixExpr() is { } exprs) && (exprs.Length == 2))
            {
                SymbolBase lhs = Deduct(exprs[0], scope);
                SymbolBase rhs = Deduct(exprs[1], scope);

                string name = context.children
                    .Where(ch => ch is TerminalNodeImpl)
                    ?.SingleOrDefault()
                    ?.GetText()
                    ?? throw new ArgumentNullException(nameof(name));

                FunctionSymbol func = lhs switch
                {
                    ClassSymbolBase classSymbol =>
                        ((FunctionSymbol)classSymbol
                        .GetMember(name, SymbolType.Function, new[] { rhs })),
                    TypeSymbol typeSymbol =>
                        ((FunctionSymbol)typeSymbol.AliasingType
                        .GetMember(name, SymbolType.Function, new[] { rhs })),
                    _ => throw new NotImplementedException(),
                };

                _ = func ?? throw new InvalidSyntaxException(
                   $"Invalid expression: undefined symbol {name}.");

                return func.Apply(new[] { rhs });
            }
            else if (context.prefixExpr() is { } prefixExpr)
            {
                return new PrefixExprTypeDeductor().Deduct(prefixExpr, scope);
            }

            throw new InvalidSyntaxException("Invalid expression: infix expression expected.");
        }
    }
}
