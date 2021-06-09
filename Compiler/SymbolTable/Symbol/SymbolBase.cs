using Antlr4.Runtime;
using Compiler.Exceptions;
using Compiler.SymbolTable.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol
{
    /// <summary>
    /// Represents symbol (variable/function/class/object/trait/type definition) in symbol table.
    /// </summary>
    public abstract class SymbolBase
    {
        /// <summary>
        /// Unique symbol identifier.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// String symbol name.
        /// </summary>
        public string Name { get; init; }
        
        /// <summary>
        /// Symbol definition/declaration context.
        /// </summary>
        public ParserRuleContext Context { get; }

        /// <summary>
        /// Symbol scope in code.
        /// </summary>
        public Scope Scope { get; set; }

        public SymbolBase(string name, ParserRuleContext context, Scope scope = null)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Guid = Guid.NewGuid();
            Context = context;
            Name = name;
            Scope = scope;
        }

        /// <summary>
        /// Construct new symbol from specified rule context in stated scope.
        /// </summary>
        /// <param name="context"> Parse rule context of symbol in parse tree. </param>
        /// <param name="type"> Symbol type (variable/function/class). </param>
        /// <param name="scope"> Symbol scope according to parse tree lookup. </param>
        public SymbolBase(ParserRuleContext context, Scope scope = null)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            Guid = Guid.NewGuid();
            Context = context;
            Scope = scope;
        }

        /// <summary>
        /// Resolve all unresolved symbols in current symbol definition.
        /// </summary>
        public abstract void Resolve();

        /// <summary>
        /// Resolve type by its name.
        /// Type may be represented by Class, Trait or Type.
        /// </summary>
        /// <param name="typeName"> Type name (class/trait/type). </param>
        /// <returns> Type symbol if found or null if unable to resolve type. </returns>
        protected SymbolBase ResolveType(string typeName)
        {
            if (typeName is null) return null;

            SymbolBase type = Scope.GetSymbol(typeName, SymbolType.Class)
                ?? Scope.GetSymbol(typeName, SymbolType.Type)
                ?? Scope.GetSymbol(typeName, SymbolType.Trait);

            return type;
        }

        /// <summary>
        /// Get symbol type from type context.
        /// </summary>
        /// <param name="context"> Type context. </param>
        /// <param name="scope"> Scope of symbol declaration/definition. </param>
        /// <returns> Symbol of declared type or null if unable to resolve type at this moment. </returns>
        protected SymbolBase GetType(Type_Context context, Scope scope, out string unresolvedTypeName)
        {
            unresolvedTypeName = null;

            if (context is null) return null;

            string typeName = context
                ?.infixType()
                ?.compoundType()?.FirstOrDefault()
                ?.annotType()?.FirstOrDefault()
                ?.simpleType()
                ?.stableId()?.GetText();

            _ = typeName ?? throw new InvalidSyntaxException($"Invalid symbol definition: type expected.");

            SymbolBase typeSymbol = scope.GetSymbol(typeName, SymbolType.Class)
                ?? scope.GetSymbol(typeName, SymbolType.Type)
                ?? scope.GetSymbol(typeName, SymbolType.Trait);

            if (typeSymbol is null)
            {
                unresolvedTypeName = typeName;
            }

            return typeSymbol;
        }
    }
}
