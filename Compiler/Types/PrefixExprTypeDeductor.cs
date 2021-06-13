using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Symbol.Variable;
using Compiler.SymbolTable.Table;
using Parser.Antlr.TreeLookup.Impls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.Types
{
    /// <summary>
    /// Represents expression type deduction algorithm 
    /// for variable definition without implicitly specified type.
    /// </summary>
    public class PrefixExprTypeDeductor : ScalaBaseVisitor<SymbolBase>
    {
        /// <summary>
        /// Expression definition scope.
        /// </summary>
        private Scope _scope;

        /// <summary>
        /// Deduct prefix expression type according to expression definition context.
        /// </summary>
        /// <param name="context"> Prefix expression definition context. </param>
        /// <param name="scope"> Prefix expression definition scope. </param>
        /// <returns> Deducted expression type. </returns>
        public SymbolBase Deduct(PrefixExprContext context, Scope scope)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = scope ?? throw new ArgumentNullException(nameof(scope));
            _scope = scope;

            return Visit(context);
        }

        /// <summary>
        /// Get literal type by its string representation.
        /// </summary>
        /// <param name="literal"> Literal string representation. </param>
        /// <returns> Literal type. </returns>
        private SymbolBase GetLiteralType(string literal) => literal switch
        {
            _ when (literal.First(), literal.Last()) is ('\"', '\"') =>
                    _scope.GetSymbol("String", SymbolType.Class),
            _ when (literal.First(), literal.Last()) is ('\'', '\'')
                && char.TryParse(string.Join("", literal.Skip(1).Take(literal.Length - 2)), out _) =>
                _scope.GetSymbol("Char", SymbolType.Class),
            _ when bool.TryParse(literal, out _) =>
                _scope.GetSymbol("Boolean", SymbolType.Class),
            _ when int.TryParse(literal, out _) =>
                _scope.GetSymbol("Int", SymbolType.Class),
            _ when double.TryParse(literal, NumberStyles.Any, CultureInfo.InvariantCulture, out _) =>
                _scope.GetSymbol("Double", SymbolType.Class),
            _ => throw new NotImplementedException(),
        };

        /// <summary>
        /// Get type for variable with specified name. 
        /// Variable is expected to be defined in inner scope of precedence symbol type. 
        /// </summary>
        /// <param name="name"> Variable name. </param>
        /// <param name="prevType"> Precedence symbol type. </param>
        /// <returns> Variable type. </returns>
        private SymbolBase GetVariableType(string name, SymbolBase prevType)
        {
            return prevType switch
            {
                null => 
                    ((VariableSymbol)_scope.GetSymbol(name, SymbolType.Variable))?.Type 
                    ?? _scope.GetSymbol(name, SymbolType.Object),
                ClassSymbolBase classSymbol => 
                    ((VariableSymbol)classSymbol.GetMember(name, SymbolType.Variable))?.Type 
                    ?? classSymbol.GetMember(name, SymbolType.Object),
                TypeSymbol typeSymbol => 
                    ((VariableSymbol)typeSymbol.AliasingType.GetMember(name, SymbolType.Variable))?.Type 
                    ?? typeSymbol.AliasingType.GetMember(name, SymbolType.Object),
                _ => throw new NotImplementedException(),
            } ?? throw new InvalidSyntaxException(
                    $"Invalid expression: undefined symbol {name}.");
        }

        /// <summary>
        /// Get return type for function with specified signature.
        /// Function expected to be defined in inner 
        /// scope of previously stated symbol in expression.
        /// </summary>
        /// <param name="symbol"> Contains partial function signature (name and argument types). </param>
        /// <param name="prevType"> Type of previously stated symbol in expression defintion. </param>
        /// <returns> Function return type. </returns>
        private SymbolBase GetFunctionReturnType(string name,
                                                 IEnumerable<SymbolBase> args,
                                                 SymbolBase prevType)
        {
            FunctionSymbol func = prevType switch
            {
                null => (FunctionSymbol)_scope
                    .GetSymbol(name, SymbolType.Function, true, args),
                ClassSymbolBase classSymbol => (FunctionSymbol)classSymbol
                    .GetMember(name, SymbolType.Function, args),
                TypeSymbol typeSymbol => (FunctionSymbol)typeSymbol.AliasingType
                    .GetMember(name, SymbolType.Function, args),
                _ => throw new NotImplementedException(),
            };

            _ = func ?? throw new InvalidSyntaxException(
                    $"Invalid expression: {prevType?.Name ?? _scope.Owner.Name} does not have member " +
                    $"{name}({string.Join(", ", args.Select(a => a.Name))}).");

            return func.Apply(args);
        }

        public override SymbolBase VisitInfixExpr([NotNull] InfixExprContext context)
        {
            return new InfixExprTypeDeductor().Deduct(context, _scope);
        }

        /// <summary>
        /// Extract symbol name from expression definition.
        /// </summary>
        /// <param name="context"> Exression definition context. </param>
        /// <returns></returns>
        public override SymbolBase VisitSimpleExpr1([NotNull] SimpleExpr1Context context)
        {
            if (context.argumentExprs() is { } argExprs)
            {
                // Get function argument types
                List<SymbolBase> argTypes = argExprs.args()?.exprs()?.expr()
                   ?.Select(arg => new ExprTypeDeductor().Deduct(arg, _scope))
                   ?.ToList();

                _ = argTypes ?? throw new InvalidSyntaxException(
                    "Invalid prefix expression: function arguments list expected.");

                // Get function name
                string name = (context switch
                {
                    _ when context.simpleExpr1()?.stableId() is { } id => id.children,
                    _ when context.simpleExpr1() is { } expr => expr.children,
                    _ => throw new NotImplementedException(),
                })
                ?.SingleOrDefault(ch => ch is TerminalNodeImpl t && !".()".Contains(t.GetText()))
                ?.GetText();

                _ = name ?? throw new InvalidSyntaxException(
                    "Invalid prefix expression: function name expected.");

                // Get callable symbol if exists
                SymbolBase symbol = context switch
                {
                    _ when context.simpleExpr1()?.stableId() is { } id => Visit(id.children.SingleOrDefault(ch => ch is ParserRuleContext)),
                    _ when context.simpleExpr1() is { } expr => Visit(expr.children.SingleOrDefault(ch => ch is ParserRuleContext)),
                    _ => throw new NotImplementedException(),
                };

                return GetFunctionReturnType(name, argTypes, symbol);
            }
            else if (context.exprs() is { } exprs)
            {
                return new ExprTypeDeductor().Deduct(exprs.expr().SingleOrDefault(), _scope);
            }

            return base.VisitSimpleExpr1(context);
        }

        /// <summary>
        /// Get symbol name from identifier nonterminal.
        /// </summary>
        /// <param name="context"> Identifier context. </param>
        /// <returns></returns>
        public override SymbolBase VisitStableId([NotNull] StableIdContext context)
        {
            SymbolBase symbol = base.VisitStableId(context);

            string name = context.children
                .SingleOrDefault(ch => ch is TerminalNodeImpl t && t.GetText() != ".")
                ?.GetText();

            _ = name ?? throw new InvalidSyntaxException(
                "Invalid expression: symbol identifier expected.");

            return GetVariableType(name, symbol);
        }

        public override SymbolBase VisitLiteral([NotNull] LiteralContext context)
        {
            return GetLiteralType(context.GetText());
        }

        /// <summary>
        /// Do nothing to prevent type deduction for function argument expression subtrees.
        /// </summary>
        /// <param name="context"> Argument expression context. </param>
        /// <returns></returns>
        public override SymbolBase VisitArgumentExprs([NotNull] ArgumentExprsContext context)
        {
            return default;
        }
    }
}
