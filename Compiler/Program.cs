using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree;
using Compiler.Serialization;
using Parser.Antlr.Grammar;
using Parser.Antlr.TreeLookup.Impls;
using Parser.ErrorListeners;
using System;
using System.Collections.Generic;
using System.IO;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var workingDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
            var expression = File.ReadAllText(workingDirectory + "/Resources/code.scala");

            var inputStream = new AntlrInputStream(expression);
            var lexer = new ScalaLexer(inputStream);

            lexer.AddErrorListener(new LexerErrorListener());

            Console.WriteLine("\n**************************** TOKENS ****************************");
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
            
            // var serializer = new ParseTreeSerializer("tree.dot");
            // serializer.ToDot(tree);
            // serializer.Close();

            Console.WriteLine(tree.ToStringTree(parser));

            var visitor = new ScalaBaseVisitor<bool>();
            Console.WriteLine("\n***************************** VISITOR *****************************");
            visitor.Visit(tree);

            var listener = new ScalaBaseListener();
            var walker = new ParseTreeWalker();
            Console.WriteLine("\n***************************** LISTENER *****************************");
            walker.Walk(listener, tree);
        }
    }
}
