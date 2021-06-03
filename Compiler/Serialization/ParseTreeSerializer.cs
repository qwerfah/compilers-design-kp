﻿using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Parser.Antlr.Grammar;

namespace Compiler.Serialization
{
    /// <summary>
    /// ANTLR IParseTree serializer to file in specified format.
    /// </summary>
    class ParseTreeSerializer
    {
        /// <summary>
        /// File stream writer.
        /// </summary>
        private StreamWriter _writer;

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
        /// Open file to write tree.
        /// </summary>
        /// <param name="filename"></param>
        public void Open(string filename)
        {
            _writer?.Close();
            _writer = new StreamWriter(filename);
        }

        /// <summary>
        /// Close file.
        /// </summary>
        public void Close()
        {
            _writer.Close();
            _writer = null;
        }

        /// <summary>
        /// Serialize parse tree to DOT format.
        /// </summary>
        /// <param name="tree"> Instance of parse tree class implementing IParseTree interface. </param>
        public void ToDot(IParseTree tree)
        {
            if (_writer is null)
            {
                throw new ArgumentNullException("File not open.");
            }

            _writer.WriteLine("digraph ParseTree {");
            ToDotRecursive(tree);
            _writer.WriteLine("}");
        }

        private void ToDotRecursive(IParseTree node)
        {
            var context = Convert.ChangeType(node, node.GetType());

            string nodeName = node.GetType().Name.Replace("Context", string.Empty);
            string nodeHash = node.GetHashCode().ToString();
            string nodeText = node.GetText();

            for (int i = 0; i < node.ChildCount; i++)
            {
                string childName = node.GetChild(i).GetType().Name.Replace("Context", string.Empty);
                string childHash = node.GetChild(i).GetHashCode().ToString();
                string childText = node.GetChild(i).GetText();

                _writer.WriteLine($"\"{nodeHash}\n{nodeName}\n{((nodeText.Length < 30) ? nodeText : string.Empty)}\" " +
                                  $"-> \"{childHash}\n{childName}\n{((childText.Length < 30) ? childText : string.Empty)}\"");
                ToDotRecursive(node.GetChild(i));
            }
        }
    }
}