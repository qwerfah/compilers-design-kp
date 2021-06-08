using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Class
{
    /// <summary>
    /// Represents object definition symbol.
    /// </summary>
    public class ObjectSymbol : ClassSymbolBase
    {
        /// <summary>
        /// Constructs object definition symbol from specified definition context in given scope.
        /// </summary>
        /// <param name="context"> Object definition context. </param>
        /// <param name="scope"> Object definition scope. </param>
        public ObjectSymbol(ObjectDefContext context, Scope scope)
            : base(context.Parent as TmplDefContext, scope)
        {
            Name = GetName(context);
            (Parent, Traits) = GetParents(context.classTemplateOpt()?.classTemplate()?.classParents());
        }
    }
}
