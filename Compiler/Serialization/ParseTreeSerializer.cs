using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Parser.Antlr.Grammar;
using System.Text.RegularExpressions;

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

            _writer.WriteLine("digraph ParseTree {");
            ToDotRecursive(tree);
            _writer.WriteLine("}");
        }

        /// <summary>
        /// Serializing parse tree to DOT using recursive tree lookup.
        /// </summary>
        /// <param name="node"> Current parse tree node in lookup. </param>
        private void ToDotRecursive(IParseTree node)
        {
            string nodeName = node.GetType().Name.Replace("Context", string.Empty);
            string nodeHash = node.GetHashCode().ToString();

            for (int i = 0; i < node.ChildCount; i++)
            {
                string childName = node.GetChild(i).GetType().Name.Replace("Context", string.Empty);
                string childHash = node.GetChild(i).GetHashCode().ToString();
                string childText = node.GetChild(i).GetText();

                _writer.WriteLine($"\"{nodeHash}\n{nodeName}\"" +
                                  $"-> \"{childHash}\n{childName}" +
                                  $"{((node.GetChild(i).ChildCount == 0) ? "\nToken = \\\"" + Escape(childText) + "\\\"" : string.Empty)}\"\n");
                ToDotRecursive(node.GetChild(i));
            }
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