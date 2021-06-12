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
    public class ExprTypeDeductor : ScalaBaseVisitor<bool>
    {
        /// <summary>
        /// Exression symbol names. Order the same as in expression.
        /// </summary>
        public List<Tuple<string, SymbolType, List<SymbolBase>>> Symbols { get; } = new();

        /// <summary>
        /// Expression definition scope.
        /// </summary>
        private Scope _scope;

        /// <summary>
        /// Deduct expression type according to expression definition context.
        /// </summary>
        /// <param name="context"> Expression definition context. </param>
        /// <param name="scope"> Expression definition scope. </param>
        /// <returns> Deducted expression type. </returns>
        public SymbolBase Deduct(Expr1Context context, Scope scope)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = scope ?? throw new ArgumentNullException(nameof(scope));
            _scope = scope;

            Visit(context);

            SymbolBase prevType = null;

            for (int i = 0; i < Symbols.Count; i++)
            {
                SymbolBase currType = Symbols[i].Item2 switch
                {
                    SymbolType.Literal when (Symbols[i].Item1.First(), Symbols[i].Item1.Last()) is ('\"', '\"') =>
                        _scope.GetSymbol("String", SymbolType.Class),
                    SymbolType.Literal when (Symbols[i].Item1.First(), Symbols[i].Item1.Last()) is ('\'', '\'')
                        && char.TryParse(string.Join("", Symbols[i].Item1.Skip(1).Take(Symbols[i].Item1.Length - 2)), out _) =>
                        _scope.GetSymbol("Char", SymbolType.Class),
                    SymbolType.Literal when bool.TryParse(Symbols[i].Item1, out _) =>
                        _scope.GetSymbol("Boolean", SymbolType.Class),
                    SymbolType.Literal when int.TryParse(Symbols[i].Item1, out _) =>
                        _scope.GetSymbol("Int", SymbolType.Class),
                    SymbolType.Literal when double.TryParse(Symbols[i].Item1, NumberStyles.Any, CultureInfo.InvariantCulture, out _) =>
                        _scope.GetSymbol("Double", SymbolType.Class),
                    SymbolType.Variable => GetVariableType(Symbols[i].Item1, prevType),
                    SymbolType.Function => GetFunctionReturnType(Symbols[i], prevType),
                    _ => throw new NotImplementedException(),
                } ?? throw new InvalidSyntaxException(
                    $"Invalid expression: type {prevType.Name} has no symbol {Symbols[i].Item1}");

                prevType = currType;
            }

            return prevType;
        }

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
        private SymbolBase GetFunctionReturnType(
            Tuple<string, SymbolType, 
            List<SymbolBase>> symbol, 
            SymbolBase prevType)
        {
            FunctionSymbol func = prevType switch
            {
                null => (FunctionSymbol)_scope
                    .GetSymbol(symbol.Item1, symbol.Item2, true, symbol.Item3),
                ClassSymbolBase classSymbol => (FunctionSymbol)classSymbol
                    .GetMember(symbol.Item1, symbol.Item2, symbol.Item3),
                TypeSymbol typeSymbol => (FunctionSymbol)typeSymbol.AliasingType
                    .GetMember(symbol.Item1, symbol.Item2, symbol.Item3),
                _ => throw new NotImplementedException(),
            };

            _ = func ?? throw new InvalidSyntaxException(
                    $"Invalid expression: undefined symbol {symbol.Item1}.");

            return func.Apply(symbol.Item3);
        }

        /// <summary>
        /// Extract symbol name from expression definition.
        /// </summary>
        /// <param name="context"> Exression definition context. </param>
        /// <returns></returns>
        public override bool VisitSimpleExpr1([NotNull] SimpleExpr1Context context)
        {
            base.VisitSimpleExpr1(context);

            if (context.argumentExprs() is { } argExprs)
            {
                List<SymbolBase> argTypes = new();

                if (argExprs.args().exprs()?.expr() is { } exprs)
                {
                    argTypes = exprs
                   .Select(arg => new ExprTypeDeductor().Deduct(arg.expr1(), _scope))
                   .ToList();
                }

                Symbols[Symbols.Count - 1] =
                    new(Symbols.Last().Item1, SymbolType.Function, argTypes);
            }

            string name = context.children
                .SingleOrDefault(ch => ch is TerminalNodeImpl t && !".()".Contains(t.GetText()))
                ?.GetText();

            if (name is not null)
            {
                Symbols.Add(new(name, SymbolType.Variable, null));
            }
            else
            {
                name = context.literal()?.GetText();

                if (name is not null)
                {
                    Symbols.Add(new(name, SymbolType.Literal, null));
                }
            }

            return default;
        }

        /// <summary>
        /// Get symbol name from identifier nonterminal.
        /// </summary>
        /// <param name="context"> Identifier context. </param>
        /// <returns></returns>
        public override bool VisitStableId([NotNull] StableIdContext context)
        {
            base.VisitStableId(context);

            string name = context.children
                .SingleOrDefault(ch => ch is TerminalNodeImpl t && t.GetText() != ".")
                ?.GetText();

            _ = name ?? throw new InvalidSyntaxException(
                "Invalid expression: symbol identifier expected.");

            Symbols.Add(new(name, SymbolType.Variable, null));

            return default;
        }

        /// <summary>
        /// Do nothing to prevent type deduction for function argument expression subtrees.
        /// </summary>
        /// <param name="context"> Argument expression context. </param>
        /// <returns></returns>
        public override bool VisitArgumentExprs([NotNull] ArgumentExprsContext context)
        {
            return default;
        }
    }
}
