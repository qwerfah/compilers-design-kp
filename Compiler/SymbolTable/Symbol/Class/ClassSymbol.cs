using Antlr4.Runtime;
using Compiler.SymbolTable.Symbol.Variable;
using Compiler.SymbolTable.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Class
{
    /// <summary>
    /// Represents class symbol definition.
    /// </summary>
    public class ClassSymbol : ClassSymbolBase
    {
        /// <summary>
        /// Class constructor symbol.
        /// </summary>
        public FunctionSymbol Constructor { get; private set; }

        /// <summary>
        /// Constructs class symbol with specified name, parent, traits in given scope.
        /// </summary>
        /// <param name="name"> Class name. </param>
        /// <param name="parent"> Explicitly inherited parent class. </param>
        /// <param name="traits"> Traits that implemented by current class. </param>
        /// <param name="scope"> Class definition scope. </param>
        public ClassSymbol(
            string name,
            AccessModifier accessMod,
            ParserRuleContext context = null,
            SymbolBase parent = null,
            List<SymbolBase> traits = null,
            Scope scope = null)
            : base(name, accessMod, context, scope)
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

        public override string ToString()
        {
            return $"{(AccessMod == AccessModifier.None ? string.Empty : AccessMod)} " +
                   $"class {Name} " +
                   $"{(Parent is { } ? ("extends " + Parent.Name) : string.Empty)} " +
                   $"{(Traits is { } ? string.Join(" ", Traits.Select(t => "with " + t.Name)) : string.Empty)}";
        }

        public override void Resolve()
        {
            base.Resolve();
            // Define ctor function for current class.
            Constructor = new(Name + "_ctor", AccessMod, this, InnerScope, Context, Scope);
            Scope.Define(Constructor);
        }
    }
}
