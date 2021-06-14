using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Table;
using Compiler.Types;
using DotNetGraph;
using DotNetGraph.Edge;
using DotNetGraph.Node;
using Parser.Antlr.TreeLookup.Impls;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.CallGraph
{
    /// <summary>
    /// Represents call graph builder based on 
    /// standart ANTLR parse tree visitor implemetnation.
    /// Uses symbol table to build call graph from parse tree.
    /// </summary>
    public class CallGraphBuilder
    {
        public CallGraphNode Graph { get; private set; }

        /// <summary>
        /// Build call graph for given function symbol.
        /// </summary>
        /// <param name="symbol"> Function symbol. </param>
        /// <returns></returns>
        public void Build(FunctionSymbol symbol)
        {
            _ = symbol ?? throw new ArgumentNullException(nameof(symbol));

            Graph = new CallGraphNode { Function = symbol };

            Build(Graph);
        }
        
        private void Build(CallGraphNode node)
        {
            // If function symbol is function definition
            if (node.Function.Context is FunDefContext funDef && funDef.expr() is { } expr)
            {
                ExprTypeDeductor deductor = new();

                try
                {
                    deductor.Deduct(expr, node.Function.InnerScope);
                    node.Calls = deductor.Calls.Select(c => new CallGraphNode { Function = c }).ToList();

                    foreach (var call in node.Calls)
                    {
                        if (call.Function.Guid != node.Function.Guid)
                        
                            Build(call);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(
                        $"Error at {node.Function.Context.Start.Line}:" +
                        $"{node.Function.Context.Start.Column} - {e.Message}");
                }
            }
        }
    }
}
