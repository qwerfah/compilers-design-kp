using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Symbol.Class;
using DotNetGraph;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using DotNetGraph.Node;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Serialization
{
    class ClassTreeSerializer : SerializerBase
    {
        private List<Tuple<int, int>> _edgeHashes = new();

        public ClassTreeSerializer(string filename)
        {
            try
            {
                _writer = new StreamWriter(filename);
            }
            catch (Exception)
            {
                _writer = null;
            }
        }

        /// <summary>
        /// Serialize class inheritence tree from symbol table to DOT.
        /// </summary>
        /// <param name="SymbolTable"> Symbol table. </param>
        public void ToDot(SymbolTable.Table.SymbolTable symbolTable)
        {
            _ = symbolTable ?? throw new ArgumentNullException("File not opened.");

            DotGraph graph = new("ParseTree");
            
            foreach (var scope in symbolTable.Scopes)
            {
                foreach (var symbol in scope.ClassMap.Values)
                {
                    ToDotRecursive(graph, symbol);
                }

                foreach (var symbol in scope.ObjectMap.Values)
                {
                    ToDotRecursive(graph, symbol);
                }
            }

            _writer.Write(graph.Compile(true));
        }

        private DotNode ToDotRecursive(DotGraph graph, ClassSymbolBase symbol)
        {
            DotNode node = new(symbol.GetHashCode().ToString())
            {
                Shape = DotNodeShape.Rectangle,
                Label = symbol.Name
            };

            graph.Elements.Add(node);

            if (symbol.Parent is not null)
            {
                DotNode parent = symbol.Parent switch
                {
                    ClassSymbolBase classSymbol => ToDotRecursive(graph, classSymbol),
                    TypeSymbol typeSymbol => ToDotRecursive(graph, typeSymbol.GetActualType()),
                    _ => throw new NotImplementedException(),
                };

                if(!_edgeHashes.Contains(new(symbol.GetHashCode(), symbol.Parent.GetHashCode())))
                {
                    graph.Elements.Add(new DotEdge(node, parent));
                    _edgeHashes.Add(new(symbol.GetHashCode(), symbol.Parent.GetHashCode()));
                }
            }

            return node;
        }
    }
}
