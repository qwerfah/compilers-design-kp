using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Symbol.Class;
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
        private bool _isMinimal; 

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

        public void ToDot(SymbolTable.Table.SymbolTable table, bool isMinimal = false)
        {
            _ = table ?? throw new ArgumentNullException(nameof(table));
            _ = _writer ?? throw new ArgumentNullException("File not opened.");

            _isMinimal = isMinimal;

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

            if (_isMinimal 
                && StandartTypes.Types.Select(t => t.Name).Contains(scope.Owner?.Name ?? ""))
            {
                return null;
            }

            string args = string.Join("\n", scope.ParamMap.Values.Select(c => c.ToString()));
            string classes = string.Join("\n", scope.ClassMap.Values.Select(c => c.ToString()));
            string objects = string.Join("\n", scope.ObjectMap.Values.Select(c => c.ToString()));
            string types = string.Join("\n", scope.TypeMap.Values.Select(c => c.ToString()));
            string traits = string.Join("\n", scope.TraitMap.Values.Select(c => c.ToString()));
            string variables = string.Join("\n", scope.VariableMap.Values.Select(c => c.ToString()));
            string functions = string.Join("\n", scope.FunctionMap.Values.Select(c => c.ToString()));

            string label = 
                (string.IsNullOrWhiteSpace(args) ? string.Empty : $"Params:\n{args}") +
                (string.IsNullOrWhiteSpace(classes) ? string.Empty : $"\nClasses:\n{classes}") +
                (string.IsNullOrWhiteSpace(objects) ? string.Empty : $"\nObjects:\n{objects}") +
                (string.IsNullOrWhiteSpace(types) ? string.Empty : $"\nTypes:\n{types}") +
                (string.IsNullOrWhiteSpace(traits) ? string.Empty : $"\nTraits:\n{traits}") +
                (string.IsNullOrWhiteSpace(variables) ? string.Empty : $"\nVariables:\n{variables}") +
                (string.IsNullOrWhiteSpace(functions) ? string.Empty : $"\nFunctions:\n{functions}");

            string name = scope.Owner switch
            {
                null => "Global scope\n",
                ClassSymbol cs => $"class {cs.Name}\n",
                ObjectSymbol os => $"object {os.Name}\n",
                FunctionSymbol fs => $"function {fs.Name}\n",
                _ => throw new NotImplementedException(),
            };
            DotNode node = new(scope.GetHashCode().ToString())
            {
                Shape = DotNodeShape.Rectangle,
                Label = name + label,
            };

            graph.Elements.Add(node);
            
            foreach (var symbol in scope.ObjectMap.Values)
            {
                DotNode child = ToDotRecursive(graph, symbol.InnerScope);

                if (child is null) continue;

                DotEdge edge = new(node, child)
                {
                    //Label = symbol.Name,
                };

                graph.Elements.Add(edge);
            }

            foreach (var func in scope.FunctionMap.Values)
            {
                foreach (var overload in func.Overloads)
                {
                    DotNode child = ToDotRecursive(graph, overload.InnerScope);

                    if (child is null) continue;

                    DotEdge edge = new(node, child)
                    {
                        // Label = overload.InnerScope.Owner.Name,
                    };
                    graph.Elements.Add(edge);
                }
            }

            return node;
        }
    }
}
