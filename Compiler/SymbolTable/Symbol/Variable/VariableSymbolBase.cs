using Antlr4.Runtime;
using Compiler.Exceptions;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Variable
{
    /// <summary>
    /// Abstract class for any variable definition 
    /// (local variable, class field, class param or function argument).
    /// </summary>
    public abstract class VariableSymbolBase : SymbolBase
    {
        /// <summary>
        /// Variable definition/declaration keywords.
        /// </summary>
        public static string[] DefKeywords { get; } = new[] { "val", "var" };

        /// <summary>
        /// Terminal symbols that can contain in variable definition except its name.
        /// </summary>
        public static string[] Terminals { get; } = new[] { "var", "val", ":" };

        /// <summary>
        /// Shows if variable is defined as mutable (var) or immutable (val).
        /// </summary>
        public bool IsMutable { get; set; } = false;

        /// <summary>
        /// Reference to symbol that represents variable type.
        /// Any type represented by class, trait, template or type definition.
        /// If type is not stated in variable definition it will be deducted by value.
        /// </summary>
        public SymbolBase Type { get; set; }

        /// <summary>
        /// Contains variable type name if it wasn't resolved during first analyzer pass.
        /// </summary>
        protected string _unresolvedTypeName = null;

        /// <summary>
        /// Variable value (if stated in definition).
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Access modifier if variable is a class field.
        /// </summary>
        public AccessModifier AccessMod { get; set; } = AccessModifier.None;

        /// <summary>
        /// Constructs variable symbol from given definition/declaration context
        /// in specified scope.
        /// </summary>
        /// <param name="context"> Varialbe definition/declaration context. </param>
        /// <param name="scope"> Varialbe definition/declaration scope. </param>
        public VariableSymbolBase(ParserRuleContext context, Scope scope) 
            : base(context, scope)
        {
        }

        /// <summary>
        /// Constructs variable symbol according to given name, type, access modifier and mutability.
        /// Context is used only for reference to symbol definition in parse tree and does not used in 
        /// symbol definition.
        /// </summary>
        /// <param name="name"> Variable name. </param>
        /// <param name="context"> Variable definition context. </param>
        /// <param name="isMutable"> Variable mutability. </param>
        /// <param name="type"> Variable type symbol. </param>
        /// <param name="access"> Variable access modifier (None if not a class field). </param>
        public VariableSymbolBase(
            string name, 
            ParserRuleContext context, 
            bool isMutable, 
            SymbolBase type, 
            AccessModifier access) 
            : base(name, context)
        {
            IsMutable = isMutable;
            Type = type;
            AccessMod = access;
        }

        public override void Resolve()
        {
            Type = ResolveType(_unresolvedTypeName) ?? Type 
                ?? throw new InvalidSyntaxException(
                    "Invalid variable definition/declaration: unable to resolve variable type.");
        }

        public override string ToString()
        {
            return $"{(AccessMod is { } ? AccessMod : string.Empty)} " +
                   $"{(IsMutable ? "var" : "val")} " +
                   $"{Name} " +
                   $" : {(Type is { } ? Type.Name : "Any")}";
        }
    }
}
