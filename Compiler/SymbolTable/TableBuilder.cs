using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Parser.Antlr.Grammar;
using Parser.Antlr.TreeLookup.Impls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable
{
    public class TableBuilder : ScalaBaseVisitor<bool>
    {
        /// <summary>
        /// Table of all symbol definitions.
        /// </summary>
        public SymbolTable SymbolTable { get; } = new();

        /// <summary>
        /// Visitor method for class definition.
        /// </summary>
        /// <param name="context"> Class definiton tree node context. </param>
        /// <returns></returns>
        public override bool VisitClassDef([NotNull]ClassDefContext context)
        {
            SymbolTable.GetCurrentScope().Define(context);
            SymbolTable.PushScope();
            bool result = base.VisitClassDef(context);
            SymbolTable.PopScope();

            return result;
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
        /// Visitor method for object definition.
        /// </summary>
        /// <param name="context"> Object definiton tree node context. </param>
        /// <returns></returns>
        public override bool VisitObjectDef([NotNull] ObjectDefContext context)
        {
            SymbolTable.GetCurrentScope().Define(context);
            SymbolTable.PushScope();
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
            SymbolTable.PushScope();
            bool result = base.VisitClassParam(context);
            SymbolTable.PopScope();

            return result;
        }

        /// <summary>
        /// Visitor method for function definition.
        /// Function may be class method or embedded in other function.
        /// </summary>
        /// <param name="context"> Function definiton tree node context. </param>
        /// <returns></returns>
        public override bool VisitFunDef([NotNull] FunDefContext context)
        {
            SymbolTable.GetCurrentScope().Define(context);
            SymbolTable.PushScope();
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
            // SymbolTable.GetCurrentScope().Define(context);

            return base.VisitParam(context);
        }

        /// <summary>
        /// Visitor method for variable definition.
        /// Symbol may be local function variable or class field and than it can have an access modifier.
        /// </summary>
        /// <param name="context"> Variable definiton tree node context </param>
        /// <returns></returns>
        public override bool VisitPatVarDef([NotNull] PatVarDefContext context)
        {
            // SymbolTable.GetCurrentScope().Define(context);

            return base.VisitPatVarDef(context);
        }
    }
}
