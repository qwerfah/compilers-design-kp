using Antlr4.Runtime;

namespace Parser.Errors
{
    public class SyntaxError<TSymbol>
    {
        public IRecognizer Recognizer { get; }
        public TSymbol OffendingSymbol { get; }
        public int Line { get; }
        public int CharPositionInLine { get; }
        public string Message { get; }
        public RecognitionException Exception { get; }

        public SyntaxError(
            IRecognizer recognizer,
            TSymbol offendingSymbol,
            int line,
            int charPositionInLine,
            string message,
            RecognitionException exception)
        {
            Recognizer = recognizer;
            OffendingSymbol = offendingSymbol;
            Line = line;
            CharPositionInLine = charPositionInLine;
            Message = message;
            Exception = exception;
        }

        public override string ToString()
        {
            return $"Syntax error: " +
                   $"offendingSymbol = {OffendingSymbol}; " +
                   $"line = {Line}; " +
                   $"charPositionInLine = {CharPositionInLine}; " +
                   $"message = {Message}";
        }
    }
}