using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using Compiler.SymbolTable.Symbol;
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

namespace Compiler.CallGraph
{
    /// <summary>
    /// Represents call graph builder based on 
    /// standart ANTLR parse tree visitor implemetnation.
    /// Uses symbol table to build call graph from parse tree.
    /// </summary>
    public class CallGraphBuilder : ScalaBaseVisitor<List<CallGraphNode>>
    {
        private Scope _scope;
        /// <summary>
        /// Symbol table for specified parse tree.
        /// </summary>
        private readonly SymbolTable.Table.SymbolTable _symbolTable;

        public CallGraphBuilder(SymbolTable.Table.SymbolTable symbolTable)
        {
            _ = symbolTable ?? throw new ArgumentNullException(nameof(symbolTable));

            _symbolTable = symbolTable;
        }

        /// <summary>
        /// Build call graph from given parse tree.
        /// </summary>
        /// <param name="tree"> Parse tree. </param>
        /// <returns></returns>
        public CallGraphNode Build(FunctionSymbol symbol)
        {
            CallGraphNode root = new(symbol);
            _scope = symbol.InnerScope;

            root.Children.AddRange(Visit(symbol.Context));

            return root;
        }

        /// <summary>
        /// Get function symbol from function call, build 
        /// call graph for this function and include it in result. 
        /// </summary>
        /// <param name="context"> Function call context. </param>
        /// <returns> List of childrens. </returns>
        public override List<CallGraphNode> VisitSimpleExpr1([NotNull] SimpleExpr1Context context)
        {
            CallGraphNode node = Build(GetFunctionSymbol(context));
            List<CallGraphNode> nodes = base.VisitSimpleExpr1(context);

            if (node is { })
            {
                nodes = nodes ?? new();
                nodes.Add(node);
            }

            return nodes;
        }

        /// <summary>
        /// Get function symbol from call context.
        /// </summary>
        /// <param name="context"> Function call context. </param>
        /// <returns> Function symbol if stated in call context, otherwise null. </returns>
        private FunctionSymbol GetFunctionSymbol(SimpleExpr1Context context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            TerminalNodeImpl[] terminals = context.children
                .Where(ch => ch is TerminalNodeImpl)
                .Select(ch => ch as TerminalNodeImpl)
                .ToArray();

            string name = (terminals is null || !terminals.Any())
                ? context.stableId()?.GetText()
                : terminals.SingleOrDefault(t => t.GetText() != ".").GetText();

            if (name is null) return null;

            return (FunctionSymbol)_scope.GetSymbol(name, SymbolType.Function) 
                ?? throw new InvalidSyntaxException($"Ivalid function call: no function with name {name}.");
        }
    }
}
