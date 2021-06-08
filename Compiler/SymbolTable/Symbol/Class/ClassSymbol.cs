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
    /// Represents class symbol definition.
    /// </summary>
    public class ClassSymbol : ClassSymbolBase
    {
        /// <summary>
        /// Constructs class symbol with specified name, parent, traits in given scope.
        /// </summary>
        /// <param name="name"> Class name. </param>
        /// <param name="parent"> Explicitly inherited parent class. </param>
        /// <param name="traits"> Traits that implemented by current class. </param>
        /// <param name="scope"> Class definition scope. </param>
        public ClassSymbol(
            string name,
            ParserRuleContext context = null,
            SymbolBase parent = null, 
            List<SymbolBase> traits = null, 
            Scope scope = null) 
            : base(name, context, scope)
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

        /// <summary>
        /// Constructs class symbol from given class definition context in given scope.
        /// </summary>
        /// <param name="context"> Class definition context. </param>
        /// <param name="scope"> Class definition scope. </param>
        public ClassSymbol(ClassDefContext context, Scope scope = null)
            : base(context.Parent as TmplDefContext, scope)
        {
            Name = GetName(context);
            (Parent, Traits) = GetParents(context.classTemplateOpt()?.classTemplate()?.classParents());
        }
    }
}
