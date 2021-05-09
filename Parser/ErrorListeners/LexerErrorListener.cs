using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parser.Errors;

namespace Parser.ErrorListeners
{
    public class LexerErrorListener : IAntlrErrorListener<int>
    {
        public List<SyntaxError<int>> SyntaxErrors { get; } = new();
        
        public void SyntaxError(
            [NotNull] IRecognizer recognizer, 
            [Nullable] int offendingSymbol, 
            int line, 
            int charPositionInLine, 
            [NotNull] string msg, 
            [Nullable] RecognitionException e)
        {
            Console.Error.WriteLine($"Syntax error: " +
                                    $"offendingSymbol = {offendingSymbol}; " +
                                    $"line = {line}; " +
                                    $"charPositionInLine = {charPositionInLine}; " +
                                    $"message = {msg}\n");
            
            SyntaxErrors.Add(new SyntaxError<int>(
                recognizer, offendingSymbol, line, charPositionInLine, msg, e));
        }
    }
}
