using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Class
{
    /// <summary>
    /// Represents class symbol definition (class/object/template/trait).
    /// </summary>
    public abstract class ClassSymbolBase : SymbolBase
    {
        public ClassSymbolBase(TmplDefContext context, Scope scope) : base(context, scope)
        {
            
        }

        protected string GetName(ParserRuleContext context)
        {
            if (context.GetChild(0) is TerminalNodeImpl impl)
            {
                return impl.GetText();
            }

            throw new InvalidCastException(
                    "Can't cast first child of TmplDefContext to TerminalNodeImpl.");
        }
    }
}
