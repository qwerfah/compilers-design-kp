using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Variable
{
    /// <summary>
    /// Represents class constructor argument that may be also class field.
    /// </summary>
    class ClassParamSymbol : VariableSymbolBase
    {
        /// <summary>
        /// Constructs new symbol instance for class ctor param from its definition.
        /// </summary>
        /// <param name="context"> Class ctor param definition context. </param>
        /// <param name="scope"> Class ctor param definition scope (class inner scope). </param>
        public ClassParamSymbol(ClassParamContext context, Scope scope)
            : base(context, scope)
        {
            TerminalNodeImpl[] terminals = GetTerminals(context);
            Name = GetName(terminals);
            IsMutable = CheckMutability(terminals);
            AccessMod = GetAccessModifier(context);
            Type = GetType(context, scope);
        }

        /// <summary>
        /// Get all direct child terminal symbols from parse tree node context.
        /// </summary>
        /// <param name="context"> Parse tree node context. </param>
        /// <returns> Array of terminal symbols. </returns>
        private TerminalNodeImpl[] GetTerminals(ClassParamContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            TerminalNodeImpl[] terminals = context.children
                .Where(ch => ch is TerminalNodeImpl)
                .Select(ch => ch as TerminalNodeImpl)
                .ToArray();

            if (terminals is null || !terminals.Any())
            {
                throw new InvalidParseTreeException("Invalid parse tree for class ctor param.");
            }

            return terminals;
        }

        /// <summary>
        /// Get class ctor param name.
        /// </summary>
        /// <param name="context"> Class ctor param definition context. </param>
        /// <returns> Class ctor param name. </returns>
        private string GetName(TerminalNodeImpl[] terminals)
        {
            try
            {
                string name = terminals.SingleOrDefault(t => !new[] { "var", "val", ":" }.Contains(t.GetText())).GetText();
                return name ?? throw new InvalidParseTreeException("Invalid parse tree for class ctor param.");
            }
            catch (InvalidOperationException)
            {
                throw new InvalidParseTreeException("Invalid parse tree for class ctor param.");
            }
        }

        /// <summary>
        /// Check class ctor param mutability if it's also represents class field definition.
        /// </summary>
        /// <param name="context"> Class ctor param definition context. </param>
        /// <returns> True if field is mutable, otherwise - false. </returns>
        private bool CheckMutability(TerminalNodeImpl[] terminals)
        {
            try
            {
                return terminals.SingleOrDefault(t => new[] { "var", "val" }.Contains(t.GetText()))?.GetText() switch
                {
                    "var" => true,
                    "val" => false,
                    null  => false,
                    _     => throw new NotImplementedException(),
                };
            }
            catch (InvalidOperationException)
            {
                throw new InvalidParseTreeException("Invalid parse tree for class ctor param.");
            }
        }

        private AccessModifier GetAccessModifier(ClassParamContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            string modifier = context.modifier()?.FirstOrDefault()?.accessModifier()?.GetText();

            return modifier switch
            {
                null => AccessModifier.Public,
                "private" => AccessModifier.Private,
                "protected" => AccessModifier.Protected,
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// Get type symbol from class ctor param definition context.
        /// </summary>
        /// <param name="context"> Class ctor param definition context </param>
        /// <param name="scope"> Scope of class ctor param definition (class inner scope). </param>
        /// <returns> Type symbol. </returns>
        private SymbolBase GetType(ClassParamContext context, Scope scope)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            string typeName = context
                .paramType()
                ?.type_()
                ?.infixType()
                ?.compoundType()?.FirstOrDefault()
                ?.annotType()?.FirstOrDefault()
                ?.simpleType()
                ?.stableId()?.GetText();

            _ = typeName ?? throw new InvalidParseTreeException("Invalid subtree for class ctor param type.");

            SymbolBase typeSymbol = scope.GetSymbol(typeName, SymbolType.Class)
                ?? scope.GetSymbol(typeName, SymbolType.Type)
                ?? scope.GetSymbol(typeName, SymbolType.Trait);

            if (typeSymbol is null)
            {
                Console.Error.WriteLine($"Undefined type {typeName} for variable definition {Guid}.");
                _unresolvedTypeName = typeName;
            }

            return typeSymbol;
        }
    }
}
