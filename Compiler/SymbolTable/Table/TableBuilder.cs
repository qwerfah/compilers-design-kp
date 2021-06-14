using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Symbol.Variable;
using Parser.Antlr.TreeLookup.Impls;
using System;
using System.Collections.Generic;
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

        public TableBuilder() => LoadStandartTypes();

        /// <summary>
        /// Build symbol table from given parse tree.
        /// </summary>
        /// <param name="tree"> Parse tree. </param>
        /// <returns></returns>
        public bool Build(IParseTree tree) => Visit(tree);

        /// <summary>
        /// Resolve all unresolved symbols in any symbol definition after first pass.
        /// </summary>
        public void Resolve() => SymbolTable.Resolve();

        /// <summary>
        /// Contains errors that happen during building.
        /// </summary>
        public IEnumerable<string> Errors
        {
            get
            {
                foreach (var scope in SymbolTable.Scopes)
                {
                    foreach (var error in scope.Errors)
                    {
                        yield return error;
                    }
                }
            }
        }

        /// <summary>
        /// Visitor method for class definition.
        /// </summary>
        /// <param name="context"> Class definiton tree node context. </param>
        /// <returns></returns>
        public override bool VisitClassDef([NotNull] ClassDefContext context)
        {
            Scope currentScope = SymbolTable.GetCurrentScope();
            Scope innerScope = SymbolTable.PushScope();
            ClassSymbol symbol = new(context, innerScope, currentScope);

            currentScope.Define(symbol);
            base.VisitClassDef(context);
            SymbolTable.PopScope();

            return default;
        }

        /// <summary>
        /// Visitor method for object definition.
        /// </summary>
        /// <param name="context"> Object definiton tree node context. </param>
        /// <returns></returns>
        public override bool VisitObjectDef([NotNull] ObjectDefContext context)
        {
            Scope currentScope = SymbolTable.GetCurrentScope();
            Scope innerScope = SymbolTable.PushScope();
            ObjectSymbol symbol = new(context, innerScope, currentScope);

            currentScope.Define(symbol);
            base.VisitObjectDef(context);
            SymbolTable.PopScope();

            return default;
        }

        /// <summary>
        /// Visitor method for type alias definition.
        /// </summary>
        /// <param name="context"> Type alias definition context. </param>
        /// <returns></returns>
        public override bool VisitTypeDef([NotNull] TypeDefContext context)
        {
            SymbolTable.GetCurrentScope().Define(context);
            return base.VisitTypeDef(context);
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
            Scope currentScope = SymbolTable.GetCurrentScope();
            Scope innerScope = SymbolTable.PushScope();
            FunctionSymbol symbol = new(context, innerScope, currentScope);

            currentScope.Define(symbol);
            base.VisitFunDef(context);
            SymbolTable.PopScope();

            return default;
        }

        /// <summary>
        /// Visitor method for function declaration.
        /// Function may be class method or embedded in other function.
        /// </summary>
        /// <param name="context"> Function declaration tree node context. </param>
        /// <returns></returns>
        public override bool VisitFunDcl([NotNull] FunDclContext context)
        {
            Scope currentScope = SymbolTable.GetCurrentScope();
            Scope innerScope = SymbolTable.PushScope();
            FunctionSymbol symbol = new(context, innerScope, currentScope);

            currentScope.Define(symbol);
            base.VisitFunDcl(context);
            SymbolTable.PopScope();

            return default;
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

            foreach (var type in StandartTypes.Types)
            {
                Scope innerScope = SymbolTable.PushScope();

                ClassSymbol symbol = new(
                    type.Item1,
                    AccessModifier.None,
                    new(),
                    innerScope,
                    global,
                    type.Item2 is not null
                        ? global.ClassMap[type.Item2]
                        : null);

                global.Define(symbol);
                SymbolTable.PopScope();
            }

            foreach (var method in StandartTypes.Methods)
            {
                Scope classScope = global.ClassMap[method.Class].InnerScope;
                Scope funcScope = new Scope(ScopeType.Local, classScope);
                SymbolBase returnType = global.ClassMap[method.Signature.Returns];
                FunctionSymbol func = new(method.Signature.Func, AccessModifier.Public, 
                    returnType, funcScope, new(), classScope);

                SymbolTable.Scopes.Add(funcScope);
                classScope.Define(func);

                foreach (var arg in method.Signature.Args)
                {
                    SymbolBase type = global.ClassMap[arg];
                    ParamSymbol param = new("x", AccessModifier.None, false, type, new(), funcScope);
                    funcScope.Define(param);
                }
            }
        }
    }
}
