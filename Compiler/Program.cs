using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Compiler.CallGraph;
using Compiler.Serialization;
using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Table;
using Parser.Antlr.Grammar;
using Parser.ErrorListeners;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

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

            TableBuilder tableBuilder = new();
            tableBuilder.Build(tree);
            tableBuilder.Resolve();

            if (tableBuilder.Errors.Any())
            {
                foreach (var error in tableBuilder.Errors)
                {
                    Console.Error.WriteLine(error);
                }

                //return;
            }

            TableSerializer tableSerializer = new("table.dot");
            tableSerializer.ToDot(tableBuilder.SymbolTable, true);
            tableSerializer.Close();

            ClassTreeSerializer classTreeSerializer = new("class_tree.dot");
            classTreeSerializer.ToDot(tableBuilder.SymbolTable);
            classTreeSerializer.Close();

            CallGraphBuilder callGraphbuilder = new();
            callGraphbuilder.Build((FunctionSymbol)tableBuilder.SymbolTable.GetSymbol("f", SymbolType.Function));

            CallGraphSerializer callGraphSerializer = new("call_graph.dot");
            callGraphSerializer.ToDot(callGraphbuilder.Graph);
            callGraphSerializer.Close();
        }
    }
}
