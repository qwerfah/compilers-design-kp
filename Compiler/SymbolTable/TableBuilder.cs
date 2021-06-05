using Antlr4.Runtime.Misc;
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
        public SymbolTable SymbolTable { get; }

        /// <summary>
        /// Visitor method for class definition (class/template/object/trait).
        /// Method adds new class definition to class table 
        /// and all definitions (fields and methods) to corresponding tables in embedded scope.
        /// </summary>
        /// <param name="context"> Class definiton tree node context. </param>
        /// <returns></returns>
        public override bool VisitTmplDef([NotNull] TmplDefContext context)
        {
            SymbolTable.GetCurrentScope().Define(context);
            SymbolTable.PushScope();

            return base.VisitTmplDef(context);
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
            SymbolTable.GetCurrentScope().Define(context);
            SymbolTable.PushScope();

            return base.VisitFunDef(context);
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
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override bool VisitTemplateStat([NotNull] TemplateStatContext context)
        {
            return base.VisitTemplateStat(context);
        }

        /// <summary>
        /// Visitor method for variable definition.
        /// Symbol may be local function variable or class field and than it can have an access modifier.
        /// </summary>
        /// <param name="context"> Variable definiton tree node context </param>
        /// <returns></returns>
        public override bool VisitPatVarDef([NotNull] PatVarDefContext context)
        {
            SymbolTable.GetCurrentScope().Define(context);

            return base.VisitPatVarDef(context);
        }
    }
}
