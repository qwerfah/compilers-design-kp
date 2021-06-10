using System.IO;

namespace Compiler.Serialization
{
    public abstract class SerializerBase
    {
        /// <summary>
        /// File stream writer.
        /// </summary>
        protected StreamWriter _writer;

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
    }
}
