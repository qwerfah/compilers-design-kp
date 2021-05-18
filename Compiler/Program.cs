using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree;
using Parser.Antlr.Grammar;
using Parser.ErrorListeners;
using System;
using System.Collections.Generic;
using System.IO;

namespace Compiler
{
    class Program
    {
        static void Lookup(IParseTree parseTree)
        {
            var tree = parseTree as ParserRuleContext;

            if (tree is null)
            {
                Console.WriteLine(parseTree.ToString());
                return;
            }

            if (tree.exception is null)
            {
                Console.WriteLine(tree.ToString());
            }
            else
            {
                Console.WriteLine($"Error in node {tree}");
            }

            foreach (var child in tree.children ?? new List<IParseTree>())
            {
                Lookup(child);
            }
        }

        static void Main(string[] args)
        {
            string workingDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
            string expression = File.ReadAllText(workingDirectory + "/Resources/code.scala");

            var inputStream = new AntlrInputStream(expression);
            var lexer = new ScalaLexer(inputStream);

            lexer.AddErrorListener(new LexerErrorListener());

            foreach (var token in lexer.GetAllTokens())
            {
                Console.WriteLine(token);
            }

            lexer.Reset();

            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ScalaParser(tokenStream);
            ScalaParser.CompilationUnitContext tree = null;

            parser.AddErrorListener(new ParserErrorListener());
            parser.Interpreter.PredictionMode = PredictionMode.Sll;
            parser.ErrorHandler = new DefaultErrorStrategy();
            tree = parser.compilationUnit();


            if (parser.NumberOfSyntaxErrors == 0)
            {
                Lookup(tree);
            }
        }
    }
}
