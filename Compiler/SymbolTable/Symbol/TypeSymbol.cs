using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Table;
using System;
using System.Linq;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol
{
    /// <summary>
    /// Represents class/object/train name alias.
    /// </summary>
    public class TypeSymbol : SymbolBase
    {
        /// <summary>
        /// Aliasing type symbol.
        /// </summary>
        public SymbolBase _aliasingType;

        /// <summary>
        /// Aliasing class/object/trait symbol.
        /// </summary>
        public ClassSymbolBase AliasingType
        {
            get => _aliasingType switch
            {
                ClassSymbolBase classSymbol => classSymbol,
                TypeSymbol typeSymbol => typeSymbol.AliasingType,
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// Contains aliasing type name if it wasn't resolved during first pass.
        /// </summary>
        private string _unresolvedDefTypeName;

        /// <summary>
        /// Constructs type symbol from given type definition context in specified scope.
        /// </summary>
        /// <param name="context"> Type definition context. </param>
        /// <param name="scope"> Type definition scope. </param>
        public TypeSymbol(TypeDefContext context, Scope scope) : base(context, scope)
        {
            Name = GetName();
            _aliasingType = GetAliasingType();
        }

        /// <summary>
        /// Get type symbol name from its context.
        /// </summary>
        /// <returns> Type name. </returns>
        private string GetName() => Context.children
            .SingleOrDefault(ch => ch is TerminalNodeImpl term && term.GetText() != "=")?.GetText() 
            ?? throw new InvalidSyntaxException("Invalid type definition: type name expected.");

        /// <summary>
        /// Get aliasing type symbol from current or enclosing scope.
        /// </summary>
        /// <returns> Aliasing type symbol. </returns>
        private SymbolBase GetAliasingType() => 
            GetType((Context as TypeDefContext).type_(), Scope, out _unresolvedDefTypeName);

        public override void Resolve()
        {
            if (_unresolvedDefTypeName is null) return;

            _aliasingType = ResolveType(ref _unresolvedDefTypeName)
                ?? throw new InvalidSyntaxException(
                    "Invalid type definition: can't resolve aliasing type name.");
        }

        public override void PostResolve()
        {
            SymbolBase actualType = _aliasingType;

            do
            {
                actualType = actualType switch
                {
                    ClassSymbolBase classSymbol => classSymbol,
                    TypeSymbol typeSymbol => typeSymbol._aliasingType,
                    _ => throw new NotImplementedException(),
                };

                if (actualType == this)
                {
                    throw new InvalidSyntaxException(
                        "Invalid type definition: cyclic type reference.");
                }
            }
            while (actualType is not ClassSymbolBase);
        }

        public override string ToString() => $"type {Name} = {AliasingType?.Name}";
    }
}
