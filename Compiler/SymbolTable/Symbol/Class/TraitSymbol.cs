using Compiler.SymbolTable.Symbol.Variable;
using Compiler.SymbolTable.Table;
using System.Linq;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Class
{
    public class TraitSymbol : ClassSymbolBase
    {
        public TraitSymbol(TraitDefContext context, Scope innerScope, Scope scope) 
            : base(context, innerScope, scope)
        {

        }

        public override string ToString()
        {
            return $"{(AccessMod == AccessModifier.None ? string.Empty : AccessMod)} " +
                   $"trait {Name} " +
                   $"{(Parent is { } ? ("extends" + Parent.Name) : string.Empty)} " +
                   $"{(Traits is { } ? string.Join(" ", Traits.Select(t => "with " + t.Name)) : string.Empty)}";
        }
    }
}
