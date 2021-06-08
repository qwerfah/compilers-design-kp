﻿using Antlr4.Runtime;
using Compiler.SymbolTable.Symbol.Class;
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

        public VariableSymbolBase(ParserRuleContext context, Scope scope) 
            : base(context, scope)
        {
        }

        public VariableSymbolBase(string name, bool isMutable, SymbolBase type, AccessModifier access) 
            : base(name)
        {
            IsMutable = isMutable;
            Type = type;
            AccessMod = access;
        }

        public override void Resolve()
        {
            if (_unresolvedTypeName is null) return;

            Type = Scope.GetSymbol(_unresolvedTypeName, SymbolType.Class)
                ?? Scope.GetSymbol(_unresolvedTypeName, SymbolType.Type)
                ?? Scope.GetSymbol(_unresolvedTypeName, SymbolType.Trait);

            if (Type is null)
            {
                Console.Error.WriteLine($"Undefined type {_unresolvedTypeName} for variable definition {Guid}.");
            }
        }
    }
}
