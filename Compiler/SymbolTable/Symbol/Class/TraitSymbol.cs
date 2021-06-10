using Compiler.SymbolTable.Table;
using System.Linq;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Class
{
    public class TraitSymbol : ClassSymbolBase
    {
        public TraitSymbol(TraitDefContext context, Scope scope) : base(context, scope)
        {

        }

        public override string ToString()
        {
            return $"trait {Name} " +
                   $"{(Parent is { } ? ("extends" + Parent.Name) : string.Empty)} " +
                   $"{(Traits is { } ? string.Join(" ", Traits.Select(t => "with " + t.Name)) : string.Empty)}";
        }
    }
}
