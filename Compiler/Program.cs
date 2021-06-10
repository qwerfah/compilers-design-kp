﻿using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Compiler.CallGraph;
using Compiler.Serialization;
using Compiler.SymbolTable.Table;
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

            CallGraphBuilder callGraphbuilder = new(builder.SymbolTable);
            //callGraphbuilder.Build(builder.SymbolTable.Scopes.Last().FunctionMap.Values.Last());
        }
    }
}
