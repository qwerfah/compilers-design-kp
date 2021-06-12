using Compiler.SymbolTable.Table;
using DotNetGraph;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using DotNetGraph.Node;
using System;
using System.IO;
using System.Linq;

namespace Compiler.Serialization
{
    public class TableSerializer : SerializerBase
    {
        public TableSerializer(string filename)
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

        public void ToDot(SymbolTable.Table.SymbolTable table)
        {
            _ = table ?? throw new ArgumentNullException(nameof(table));
            _ = _writer ?? throw new ArgumentNullException("File not opened.");

            DotGraph graph = new("SymbolTable");
            Scope scope = table.Scopes.FirstOrDefault()
                ?? throw new ArgumentException("No any scopes in symbol table.");

            ToDotRecursive(graph, scope);
            _writer.Write(graph.Compile(true));
        }

        public DotNode ToDotRecursive(DotGraph graph, Scope scope)
        {
            _ = graph ?? throw new ArgumentNullException(nameof(graph));
            if (scope is null) return null;

            string label =
                "Params:\n" + string.Join("\n", scope.ParamMap.Values.Select(c => c.ToString())) +
                "\nClasses:\n" + string.Join("\n", scope.ClassMap.Values.Select(c => c.ToString())) +
                "\nObjects:\n" + string.Join("\n", scope.ObjectMap.Values.Select(c => c.ToString())) +
                "\nTypes:\n" + string.Join("\n", scope.TypeMap.Values.Select(c => c.ToString())) +
                "\nTraits:\n" + string.Join("\n", scope.TraitMap.Values.Select(c => c.ToString())) +
                "\nVariables:\n" + string.Join("\n", scope.VariableMap.Values.Select(c => c.ToString())) +
                "\nFunctions:\n" + string.Join("\n", scope.FunctionMap.Values.Select(c => c.ToString()));

            DotNode node = new(scope.GetHashCode().ToString())
            {
                Shape = DotNodeShape.Rectangle,
                Label = label,
            };

            graph.Elements.Add(node);

            /*
            foreach (var symbol in scope.ClassMap.Values)
            {
                DotNode child = ToDotRecursive(graph, symbol.InnerScope);
                DotEdge edge = new(node, child)
                {
                    Label = symbol.Name,
                };
                graph.Elements.Add(edge);
            }
            */

            foreach (var symbol in scope.ObjectMap.Values)
            {
                DotNode child = ToDotRecursive(graph, symbol.InnerScope);
                DotEdge edge = new(node, child)
                {
                    Label = symbol.Name,
                };
                graph.Elements.Add(edge);
            }

            foreach (var func in scope.FunctionMap.Values)
            {
                foreach (var overload in func.Overloads)
                {
                    DotNode child = ToDotRecursive(graph, overload.InnerScope);
                    DotEdge edge = new(node, child)
                    {
                        Label = overload.Name,
                    };
                    graph.Elements.Add(edge);
                }
            }

            return node;
        }
    }
}
