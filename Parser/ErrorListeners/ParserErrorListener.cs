using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;
using Parser.Errors;
using System;
using System.Collections.Generic;

namespace Parser.ErrorListeners
{
    /// <summary>
    /// Parser error listener for gathering parser errors.
    /// </summary>
    public class ParserErrorListener : BaseErrorListener
    {
        /// <summary>
        /// List of syntax errors.
        /// </summary>
        public List<SyntaxError<IToken>> SyntaxErrors { get; } = new();

        public override void ReportAmbiguity(
            [NotNull] Antlr4.Runtime.Parser recognizer,
            [NotNull] DFA dfa, int startIndex, int stopIndex, bool exact,
            [Nullable] BitSet ambigAlts,
            [NotNull] ATNConfigSet configs)
        {
            Console.Error.WriteLine($"ReportAmbiguity\n");
        }

        public override void ReportAttemptingFullContext(
            [NotNull] Antlr4.Runtime.Parser recognizer,
            [NotNull] DFA dfa,
            int startIndex,
            int stopIndex,
            [Nullable] BitSet conflictingAlts,
            [NotNull] SimulatorState conflictState)
        {
            Console.Error.WriteLine($"ReportAttemptingFullContext\n");
        }

        public override void ReportContextSensitivity(
            [NotNull] Antlr4.Runtime.Parser recognizer,
            [NotNull] DFA dfa,
            int startIndex,
            int stopIndex,
            int prediction,
            [NotNull] SimulatorState acceptState)
        {
            Console.Error.WriteLine($"ReportContextSensitivity\n");
        }

        public override void SyntaxError(
            [NotNull] IRecognizer recognizer,
            [Nullable] IToken offendingSymbol,
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

            SyntaxErrors.Add(new SyntaxError<IToken>(
                recognizer, offendingSymbol, line, charPositionInLine, msg, e));
        }
    }
}
