using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Table;
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
    public class ExprTypeDeductor : ScalaBaseVisitor<bool>
    {
        public List<Tuple<string, SymbolType, List<SymbolBase>>> Symbols { get; } = new();
        private Scope _scope;

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
                        && char.TryParse(Symbols[i].Item1, out _) => 
                        _scope.GetSymbol("Char", SymbolType.Class),
                    SymbolType.Literal when bool.TryParse(Symbols[i].Item1, out _) => 
                        _scope.GetSymbol("Boolean", SymbolType.Class),
                    SymbolType.Literal when int.TryParse(Symbols[i].Item1, out _) => 
                        _scope.GetSymbol("Int", SymbolType.Class),
                    SymbolType.Literal when double.TryParse(Symbols[i].Item1, out _) => 
                        _scope.GetSymbol("String", SymbolType.Class),
                    SymbolType.Variable => GetVariableType(Symbols[i].Item1, prevType),
                    SymbolType.Function => GetFunctionReturnType(Symbols[i], prevType),
                    _ => throw new NotImplementedException(),
                };

                if (currType is null)
                {
                    throw new InvalidSyntaxException(
                        $"Invalid expression: type {prevType.Name} has no symbol {Symbols[i].Item1}");
                }

                prevType = currType;
            }

            return prevType;
        }

        private SymbolBase GetVariableType(string name, SymbolBase prevType)
        {
            VariableSymbolBase symbol = (VariableSymbolBase)(prevType is null ? _scope : (prevType as ClassSymbolBase).InnerScope)
                .GetSymbol(name, SymbolType.Variable);
            _ = symbol ?? throw new InvalidSyntaxException(
                    $"Invalid expression: undefined symbol {name}.");

            return symbol.Type;
        }

        private SymbolBase GetFunctionReturnType(Tuple<string, SymbolType, List<SymbolBase>> symbol, SymbolBase prevType)
        {
            FunctionSymbol func = (FunctionSymbol)(prevType is null ? _scope : (prevType as ClassSymbolBase).InnerScope)
                .GetSymbol(symbol.Item1, symbol.Item2);
            _ = func ?? throw new InvalidSyntaxException(
                    $"Invalid expression: undefined symbol {symbol.Item1}.");

            if (func.InnerScope.ParamMap.Count == symbol.Item3.Count)
            {
                return !func.InnerScope.ParamMap.Values.Select(v => v.Type).Except(symbol.Item3).Any()
                    ? func.ReturnType
                    : throw new InvalidSyntaxException(
                        $"Invalid expression: argument type mismatch in function {func.Name}.");
            }
            else
            {
                throw new InvalidSyntaxException(
                    $"Invalid expression: invalid number of arguments for function {func.Name}.");
            }
        }

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

        public override bool VisitArgumentExprs([NotNull] ArgumentExprsContext context)
        {
            return default;
        }
    }
}
