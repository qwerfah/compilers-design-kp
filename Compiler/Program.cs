using Antlr4.Runtime;
using Parser.Antlr.Grammar;
using Parser.ErrorListeners;
using System;
using System.IO;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string workingDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            string expression = File.ReadAllText(workingDirectory + "/Resources/code.scala");

            var inputStream = new AntlrInputStream(expression);
            var lexer = new ScalaLexer(inputStream);

            lexer.AddErrorListener(new LexerErrorListener());

            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ScalaParser(tokenStream);

            parser.AddErrorListener(new ParserErrorListener());

            parser.BuildParseTree = true;
            Console.WriteLine(parser.compilationUnit().ToStringTree());
        }
    }
}
