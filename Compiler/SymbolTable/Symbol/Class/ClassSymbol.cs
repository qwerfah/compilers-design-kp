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
        public ClassSymbol(string name, SymbolBase parent = null, List<SymbolBase> traits = null, Scope scope = null) 
            : base(name, scope)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Parent = parent;
            Traits = traits;
            Scope = scope;
        }

        public ClassSymbol(ClassDefContext context, Scope scope = null)
            : base(context.Parent as TmplDefContext, scope)
        {
            Name = GetName(context);
            (Parent, Traits) = GetParents(context.classTemplateOpt()?.classTemplate()?.classParents());
        }
    }
}
