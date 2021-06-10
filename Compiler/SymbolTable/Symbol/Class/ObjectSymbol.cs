using Compiler.SymbolTable.Symbol.Variable;
using Compiler.SymbolTable.Table;
using System.Linq;
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

        public override string ToString()
        {
            return $"{(AccessMod == AccessModifier.None ? string.Empty : AccessMod)} " +
                   $"object {Name} " +
                   $"{(Parent is { } ? ("extends" + Parent.Name) : string.Empty)} " +
                   $"{(Traits is { } ? string.Join(" ", Traits.Select(t => "with " + t.Name)) : string.Empty)}";
        }
    }
}
