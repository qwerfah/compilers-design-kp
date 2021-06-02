using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (_writer is { }) _writer.Close();
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
        }
    }
}
