using Antlr4.Runtime.Tree;
using DotNetGraph;
using DotNetGraph.Edge;
using DotNetGraph.Extensions;
using DotNetGraph.Node;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Compiler.Serialization
{
    /// <summary>
    /// ANTLR IParseTree serializer to file in specified format.
    /// </summary>
    public class ParseTreeSerializer : SerializerBase
    {
        public ParseTreeSerializer(string filename)
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
        /// Serialize parse tree to DOT format.
        /// </summary>
        /// <param name="tree"> Instance of parse tree class implementing IParseTree interface. </param>
        public void ToDot(IParseTree tree)
        {
            _ = _writer ?? throw new ArgumentNullException("File not opened.");

            DotGraph graph = new("ParseTree");
            ToDotRecursive(graph, tree);
            _writer.Write(graph.Compile());
        }

        /// <summary>
        /// Serializing parse tree to DOT using recursive tree lookup.
        /// </summary>
        /// <param name="node"> Current parse tree node in lookup. </param>
        private DotNode ToDotRecursive(DotGraph graph, IParseTree tree)
        {
            _ = graph ?? throw new ArgumentNullException(nameof(graph));
            if (tree is null || string.IsNullOrWhiteSpace(tree.GetText())) return null;

            DotNode node = new(tree.GetHashCode().ToString())
            {
                Shape = DotNodeShape.Rectangle,
                Label = tree.ChildCount == 0
                    ? tree.GetText()
                    : tree.GetType().Name.Replace("Context", string.Empty),
                Color = tree.ChildCount == 0 ? Color.Red : Color.Black
            };

            graph.Elements.Add(node);

            for (int i = 0; i < tree.ChildCount; i++)
            {
                DotNode child = ToDotRecursive(graph, tree.GetChild(i));
                if (child is null) continue;
                graph.Elements.Add(new DotEdge(node, child));
            }

            return node;
        }

        /// <summary>
        /// Escape all special characters in input string.
        /// </summary>
        /// <param name="text"> Input string. </param>
        /// <returns> Formatted input string with escaped special characters. </returns>
        private string Escape(string str)
        {
            char[] chars = new[] { '\\', '\"', '\'' };

            string result = "";

            foreach (var ch in str)
            {
                result += chars.Contains(ch) ? "\\" + ch : ch;
            }

            return result;
        }
    }
}