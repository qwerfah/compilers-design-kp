﻿using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Symbol.Variable;
using Parser.Antlr.TreeLookup.Impls;
using System.Linq;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Table
{
    public class TableBuilder : ScalaBaseVisitor<bool>
    {
        /// <summary>
        /// Table of all symbol definitions.
        /// </summary>
        public SymbolTable SymbolTable { get; } = new();

        public TableBuilder()
        {
            LoadStandartTypes();
        }

        /// <summary>
        /// Build symbol table from given parse tree.
        /// </summary>
        /// <param name="tree"> Parse tree. </param>
        /// <returns></returns>
        public bool Build(IParseTree tree)
        {
            return Visit(tree);
        }

        /// <summary>
        /// Resolve all unresolved symbols in any symbol definition after first pass.
        /// </summary>
        public void Resolve()
        {
            SymbolTable.Resolve();
        }

        /// <summary>
        /// Visitor method for class definition.
        /// </summary>
        /// <param name="context"> Class definiton tree node context. </param>
        /// <returns></returns>
        public override bool VisitClassDef([NotNull] ClassDefContext context)
        {
            ClassSymbol symbol = (ClassSymbol)SymbolTable.GetCurrentScope().Define(context);
            Scope innerScope = SymbolTable.PushScope();
            symbol.InnerScope = innerScope;
            bool result = base.VisitClassDef(context);
            SymbolTable.PopScope();

            return result;
        }

        /// <summary>
        /// Visitor method for object definition.
        /// </summary>
        /// <param name="context"> Object definiton tree node context. </param>
        /// <returns></returns>
        public override bool VisitObjectDef([NotNull] ObjectDefContext context)
        {
            ObjectSymbol symbol = (ObjectSymbol)SymbolTable.GetCurrentScope().Define(context);
            Scope innerScope = SymbolTable.PushScope();
            symbol.InnerScope = innerScope;
            bool result = base.VisitObjectDef(context);
            SymbolTable.PopScope();

            return result;
        }

        /// <summary>
        /// Visitor method for class constuctor argument.
        /// Symbol may be also class field if it has variable definition keyword in its definition (var/val).
        /// </summary>
        /// <param name="context"> Class param definiton tree node context. </param>
        /// <returns></returns>
        public override bool VisitClassParam([NotNull] ClassParamContext context)
        {
            SymbolTable.GetCurrentScope().Define(context);
            return base.VisitClassParam(context);
        }

        /// <summary>
        /// Visitor method for function definition.
        /// Function may be class method or embedded in other function.
        /// </summary>
        /// <param name="context"> Function definiton tree node context. </param>
        /// <returns></returns>
        public override bool VisitFunDef([NotNull] FunDefContext context)
        {
            FunctionSymbol symbol = (FunctionSymbol)SymbolTable.GetCurrentScope().Define(context);
            Scope innerScope = SymbolTable.PushScope();
            symbol.InnerScope = innerScope;
            bool result = base.VisitFunDef(context);
            SymbolTable.PopScope();

            return result;
        }

        /// <summary>
        /// Visitor method for function argument definition.
        /// </summary>
        /// <param name="context"> Function argument definiton tree node context </param>
        /// <returns></returns>
        public override bool VisitParam([NotNull] ParamContext context)
        {
            SymbolTable.GetCurrentScope().Define(context);

            return base.VisitParam(context);
        }

        /// <summary>
        /// Visitor method for mutable/immutable variable definition.
        /// Symbol may be local function variable or class field and than it can have an access modifier.
        /// </summary>
        /// <param name="context"> Variable definiton tree node context </param>
        /// <returns></returns>
        public override bool VisitPatVarDef([NotNull] PatVarDefContext context)
        {
            SymbolTable.GetCurrentScope().Define(context);

            return base.VisitPatVarDef(context);
        }

        /// <summary>
        /// Visitor method for immutable single/multiple variable declaration.
        /// </summary>
        /// <param name="context"> Immutalbe single/multiple variable declaration context. </param>
        /// <returns></returns>
        public override bool VisitValDcl([NotNull] ValDclContext context)
        {
            SymbolTable.GetCurrentScope().Define(context);

            return base.VisitValDcl(context);
        }

        /// <summary>
        /// Visitor method for mutable single/multiple variable declaration.
        /// </summary>
        /// <param name="context"> Mutable single/multiple variable declaration context. </param>
        /// <returns></returns>
        public override bool VisitVarDcl([NotNull] VarDclContext context)
        {
            SymbolTable.GetCurrentScope().Define(context);

            return base.VisitVarDcl(context);
        }

        /// <summary>
        /// Load standart Scala types to symbol table.
        /// </summary>
        public void LoadStandartTypes()
        {
            Scope global = SymbolTable.Scopes.First();
            SymbolTable.Scopes.First().Define(new ClassSymbol("Any", AccessModifier.None, new()));
            SymbolTable.Scopes.First().Define(new ClassSymbol("AnyVal", AccessModifier.None, new(), global.ClassMap["Any"]));
            SymbolTable.Scopes.First().Define(new ClassSymbol("AnyRef", AccessModifier.None, new(), global.ClassMap["Any"]));
            SymbolTable.Scopes.First().Define(new ClassSymbol("Unit", AccessModifier.None, new(), global.ClassMap["AnyVal"]));
            SymbolTable.Scopes.First().Define(new ClassSymbol("Boolean", AccessModifier.None, new(), global.ClassMap["AnyVal"]));
            SymbolTable.Scopes.First().Define(new ClassSymbol("Char", AccessModifier.None, new(), global.ClassMap["AnyVal"]));
            SymbolTable.Scopes.First().Define(new ClassSymbol("Byte", AccessModifier.None, new(), global.ClassMap["AnyVal"]));
            SymbolTable.Scopes.First().Define(new ClassSymbol("Short", AccessModifier.None, new(), global.ClassMap["AnyVal"]));
            SymbolTable.Scopes.First().Define(new ClassSymbol("Int", AccessModifier.None, new(), global.ClassMap["AnyVal"]));
            SymbolTable.Scopes.First().Define(new ClassSymbol("Long", AccessModifier.None, new(), global.ClassMap["AnyVal"]));
            SymbolTable.Scopes.First().Define(new ClassSymbol("Float", AccessModifier.None, new(), global.ClassMap["AnyVal"]));
            SymbolTable.Scopes.First().Define(new ClassSymbol("Double", AccessModifier.None, new(), global.ClassMap["AnyVal"]));
            SymbolTable.Scopes.First().Define(new ClassSymbol("String", AccessModifier.None, new(), global.ClassMap["AnyVal"]));

            foreach (var symbol in global.ClassMap.Values)
            {
                symbol.InnerScope = SymbolTable.PushScope();
                SymbolTable.PopScope();
            }
        }
    }
}