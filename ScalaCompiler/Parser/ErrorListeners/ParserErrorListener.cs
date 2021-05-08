﻿using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.ErrorListeners
{
    public class ParserErrorListener : BaseErrorListener
    {
        public override void ReportAmbiguity(
            [NotNull] Antlr4.Runtime.Parser recognizer,
            [NotNull] DFA dfa, int startIndex, int stopIndex, bool exact,
            [Nullable] BitSet ambigAlts,
            [NotNull] ATNConfigSet configs)
        {

        }

        public override void ReportAttemptingFullContext(
            [NotNull] Antlr4.Runtime.Parser recognizer,
            [NotNull] DFA dfa,
            int startIndex,
            int stopIndex,
            [Nullable] BitSet conflictingAlts,
            [NotNull] SimulatorState conflictState)
        {

        }

        public override void ReportContextSensitivity(
            [NotNull] Antlr4.Runtime.Parser recognizer,
            [NotNull] DFA dfa,
            int startIndex,
            int stopIndex,
            int prediction,
            [NotNull] SimulatorState acceptState)
        {

        }

        public override void SyntaxError(
            [NotNull] IRecognizer recognizer,
            [Nullable] IToken offendingSymbol,
            int line,
            int charPositionInLine,
            [NotNull] string msg,
            [Nullable] RecognitionException e)
        {
            Console.WriteLine($"Syntax error: offendingSymbol = {offendingSymbol}; line = {line}; charPositionInLine = {charPositionInLine}; message = {msg}");
        }
    }
}