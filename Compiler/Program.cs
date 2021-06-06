using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree;
using Compiler.Serialization;
using Compiler.SymbolTable;
using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Symbol.Class;
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

            var serializer = new ParseTreeSerializer("tree.dot");
            serializer.ToDot(tree);
            serializer.Close();

            Console.WriteLine(tree.ToStringTree(parser));

            var builder = new TableBuilder();

            builder.Visit(tree);

            foreach (var scope in builder.SymbolTable.Scopes)
            {
                Console.WriteLine($"Scope {scope.Guid}");

                foreach (var symbol in scope.ClassMap)
                {
                    Console.WriteLine($"Class {symbol.Key}");

                    foreach (var parent in (symbol.Value as ClassSymbol).Parents ?? new List<SymbolBase>())
                    {
                        Console.WriteLine($"Extends {parent.Name}");
                    }

                    Console.WriteLine();
                }

                foreach (var symbol in scope.ObjectMap)
                {
                    Console.WriteLine($"Object {symbol.Key}");

                    foreach (var parent in (symbol.Value as ObjectSymbol).Parents ?? new List<SymbolBase>())
                    {
                        Console.WriteLine($"Extends {parent.Name}");
                    }

                    Console.WriteLine();
                }
            }
        }
    }
}
