using Compiler.CallGraph;
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
    /// <summary>
    /// Call graph serializer to DOT format.
    /// </summary>
    public class CallGraphSerializer : SerializerBase
    {
        public CallGraphSerializer(string filename)
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
        /// Serialize given call graph to DOT file.
        /// </summary>
        /// <param name="callGraph"> Call graph to serialize. </param>
        public void ToDot(CallGraphNode callGraph)
        {
            DotGraph dotGraph = new("CallGraph");
            ToDotRecursive(callGraph, dotGraph);
            _writer.Write(dotGraph.Compile(true));
        }

        /// <summary>
        /// Recursevly build dot graph from call graph.
        /// </summary>
        /// <param name="callGraph"> Call graph current node. </param>
        /// <param name="dotGraph"> Dot graph to build. </param>
        /// <returns> Node of call graph converted to dot graph node. </returns>
        public DotNode ToDotRecursive(CallGraphNode callGraph, DotGraph dotGraph)
        {
            DotNode node = new(callGraph.Function.Guid.ToString())
            {
                Shape = DotNodeShape.Rectangle,
                Label =
                    $"{callGraph.Function.Scope.Owner.Name}." +
                    $"{callGraph.Function.Name}(" +
                    $"{string.Join(", ", callGraph.Function.InnerScope.ParamMap.Values.Select(p => p.Type.Name))})",
            };

            dotGraph.Elements.Add(node);

            foreach (var call in callGraph.Calls ?? new())
            {
                DotNode child = ToDotRecursive(call, dotGraph);
                dotGraph.Elements.Add(new DotEdge(node, child));
            }

            return node;
        }
    }
}
