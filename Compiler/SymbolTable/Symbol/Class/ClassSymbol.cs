using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Class
{
    class ClassSymbol : ClassSymbolBase
    {
        public ClassSymbol(ClassDefContext context, Scope scope)
            : base(context.Parent as TmplDefContext, scope)
        {
            Name = GetName(context);
            Parents = GetParents(context.classTemplateOpt()?.classTemplate()?.classParents(), scope);
        }
    }
}
