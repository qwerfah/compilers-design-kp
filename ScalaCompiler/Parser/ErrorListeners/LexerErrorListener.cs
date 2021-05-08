using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.ErrorListeners
{
    public class LexerErrorListener : IAntlrErrorListener<int>
    {
        public void SyntaxError(
            [NotNull] IRecognizer recognizer, 
            [Nullable] int offendingSymbol, 
            int line, 
            int charPositionInLine, 
            [NotNull] string msg, 
            [Nullable] RecognitionException e)
        {
            Console.WriteLine($"Syntax error: offendingSymbol = {offendingSymbol}; line = {line}; charPositionInLine = {charPositionInLine}; message = {msg}");
        }
    }
}
