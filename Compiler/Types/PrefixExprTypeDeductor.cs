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
        /// Contains all functions that call in current expression.
        /// Uses in call graph builder
        /// </summary>
        public HashSet<FunctionSymbol> Calls { get; private set; } = new();

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

            SymbolBase symbol = Visit(context);

            return symbol;
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
        /// Variable is expected to be defined in inner scope 
        /// of precedence symbol type or in scope of parsed expression. 
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
        /// Function expected to be defined in inner scope of previously 
        /// stated symbol in expression or in scope of parsed expression.
        /// </summary>
        /// <param name="name"> Function name. </param>
        /// <param name="args"> Argument types. </param>
        /// <param name="prevType"> Type symbol that function call refers to. </param>
        /// <returns> Function return type symbol. </returns>
        private SymbolBase GetFunctionReturnType(string name,
                                                 IEnumerable<SymbolBase> args,
                                                 SymbolBase prevType)
        {
            FunctionSymbol func = prevType switch
            {
                null => (FunctionSymbol)_scope
                    .GetSymbol(name, SymbolType.Function, true, args),
                { } when IsScopeBelongsToSymbol(prevType, _scope) => (FunctionSymbol)_scope
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

            Calls.Add(func);

            return func.Apply(args);
        }

        private bool IsScopeBelongsToSymbol(SymbolBase symbol, Scope scope)
        {
            while (scope?.Owner is { })
            {
                if (symbol.Name == scope.Owner.Name) return true;
                scope = scope.EnclosingScope;
            }

            return false;
        }

        /// <summary>
        /// Extract infix expression result type from 
        /// its definition using InfixExprTypeDeductor.
        /// </summary>
        /// <param name="context"> Infix expression context. </param>
        /// <returns> Infix expression result type. </returns>
        public override SymbolBase VisitInfixExpr([NotNull] InfixExprContext context)
        {
            InfixExprTypeDeductor deductor = new();
            SymbolBase symbol = deductor.Deduct(context, _scope);

            Calls = Calls.Union(deductor.Calls).ToHashSet();

            return symbol;
        }

        /// <summary>
        /// Extract simple expression result type from expression definition.
        /// </summary>
        /// <param name="context"> Simple exression definition context. </param>
        /// <returns> Simple expression result type. </returns>
        public override SymbolBase VisitSimpleExpr1([NotNull] SimpleExpr1Context context)
        {
            // If node is function call
            if (context.argumentExprs() is { } argExprs)
            {
                // Get function argument types
                List<SymbolBase> argTypes = argExprs.args()?.exprs()?.expr()
                   ?.Select(arg => 
                   {
                       ExprTypeDeductor deductor = new();
                       SymbolBase symbol = deductor.Deduct(arg.expr1(), _scope);

                       Calls = Calls.Union(deductor.Calls).ToHashSet();

                       return symbol;
                   })
                   ?.ToList()
                   ?? new();

                // Get node children that contain terminal
                IList<IParseTree> children = context switch
                {
                    _ when context.simpleExpr1()?.stableId() is { } id => id.children,
                    _ when context.simpleExpr1() is { } expr => expr.children,
                    _ => throw new NotImplementedException(),
                };

                // Get function name
                string name = children?
                    .SingleOrDefault(ch => ch is TerminalNodeImpl t && !".()".Contains(t.GetText()))
                    ?.GetText() 
                    ?? throw new InvalidSyntaxException(
                        "Invalid prefix expression: function name expected.");

                // Get callable symbol if exists
                SymbolBase symbol = children.SingleOrDefault(ch => ch is ParserRuleContext) switch
                {
                    null => null,
                    { } child => Visit(child),
                };

                return GetFunctionReturnType(name, argTypes, symbol);
            }
            // If node is expression in parentheses
            else if (context.exprs() is { } exprs)
            {
                ExprTypeDeductor deductor = new();
                SymbolBase symbol = deductor.Deduct(exprs.expr().SingleOrDefault().expr1(), _scope);

                Calls = Calls.Union(deductor.Calls).ToHashSet();

                return symbol;
            }
            else if (context.simpleExpr1() is { } expr)
            {
                SymbolBase prev = Visit(expr);
                string name = context.children
                    .SingleOrDefault(ch => ch is TerminalNodeImpl && ch.GetText() != ".")
                    .GetText();

                _ = name ?? throw new InvalidSyntaxException(
                    "Invalid expression: variable name expected.");

                return GetVariableType(name, prev);
            }

            return base.VisitSimpleExpr1(context);
        }

        /// <summary>
        /// Get symbol type from identifier context.
        /// </summary>
        /// <param name="context"> Identifier context. </param>
        /// <returns> Symbol type. </returns>
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

        /// <summary>
        /// Get literal type from its context.
        /// </summary>
        /// <param name="context"> Literal context. </param>
        /// <returns> Litaral type. </returns>
        public override SymbolBase VisitLiteral([NotNull] LiteralContext context)
        {
            return GetLiteralType(context.GetText());
        }

        /// <summary>
        /// Do nothing to prevent type deduction for function argument expression subtrees.
        /// </summary>
        /// <param name="context"> Argument expression context. </param>
        /// <returns> Default value. </returns>
        public override SymbolBase VisitArgumentExprs([NotNull] ArgumentExprsContext context)
        {
            return default;
        }

        public override SymbolBase VisitSimpleExpr([NotNull] SimpleExprContext context)
        {
            return Visit(context.blockExpr());
        }

        public override SymbolBase VisitBlockExpr([NotNull] BlockExprContext context)
        {
            BlockExprTypeDeductor deductor = new();
            SymbolBase symbol = deductor.Deduct(context, _scope);

            Calls = Calls.Union(deductor.Calls).ToHashSet();

            return symbol;
        }
    }
}
