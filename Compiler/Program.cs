using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Tree;
using Compiler.Serialization;
using Compiler.SymbolTable;
using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Symbol.Variable;
using Compiler.SymbolTable.Table;
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

            var tokenStream = new CommonTokenStream(lexer);
            var parser = new ScalaParser(tokenStream);
            ScalaParser.CompilationUnitContext tree = null;

            parser.AddErrorListener(new ParserErrorListener());
            parser.Interpreter.PredictionMode = PredictionMode.Sll;
            parser.ErrorHandler = new DefaultErrorStrategy();
            tree = parser.compilationUnit();

            ParseTreeSerializer treeSerializer = new("tree.dot");
            treeSerializer.ToDot(tree);
            treeSerializer.Close();

            TableBuilder builder = new();
            builder.Build(tree);
            builder.Resolve();

            TableSerializer tableSerializer = new("table.dot");
            tableSerializer.ToDot(builder.SymbolTable);
            tableSerializer.Close();
        }
    }
}
