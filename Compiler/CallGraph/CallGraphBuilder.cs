using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Compiler.SymbolTable.Symbol.Class;
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
    public class CallGraphBuilder : ScalaBaseVisitor<bool>
    {
        /// <summary>
        /// Call graph for specified parse tree and sybol table.
        /// </summary>
        public CallGraph CallGraph { get; }

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
        public bool Build(ClassSymbolBase symbol)
        {
            foreach (var func in symbol.InnerScope.FunctionMap.Values)
            {
                CallGraph.Root.Add(new CallGraphNode(func));
                Visit(func.Context);
            }

            return true;
        }

        public override bool VisitSimpleExpr1([NotNull] SimpleExpr1Context context)
        {


            return base.VisitSimpleExpr1(context);
        }
    }
}
