using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using Compiler.SymbolTable.Table;
using System;
using System.Linq;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Variable
{
    /// <summary>
    /// Represents class constructor or function argument that may be also class field.
    /// </summary>
    public class ParamSymbol : VariableSymbolBase
    {
        /// <summary>
        /// Constructs new symbol instance for ctor/func param from its definition.
        /// </summary>
        /// <param name="context"> Ctor/func param definition context. </param>
        /// <param name="scope"> Ctor/func param definition scope (class inner scope). </param>
        public ParamSymbol(ParserRuleContext context, Scope scope)
            : base(context, scope)
        {
            if (context is not ClassParamContext && context is not ParamContext)
            {
                throw new ArgumentException(
                    "Invalid context type: only ClassParam or Param context are acceptable.");
            }

            TerminalNodeImpl[] terminals = GetTerminals(context);

            Name = GetName(terminals);
            IsMutable = CheckMutability(terminals);
            AccessMod = GetAccessModifier(context as ClassParamContext);
            Type = GetType(context);

            Scope.Define(new VariableSymbol(this));
        }

        public ParamSymbol(
            string name,
            AccessModifier accessMod,
            bool isMutable,
            SymbolBase type,
            ParserRuleContext context,
            Scope scope) : base(name, accessMod, context, isMutable, type, scope)
        {
            Scope.Define(new VariableSymbol(this));
        }

        /// <summary>
        /// Get all direct child terminal symbols from parse tree node context.
        /// </summary>
        /// <param name="context"> Parse tree node context. </param>
        /// <returns> Array of terminal symbols. </returns>
        private TerminalNodeImpl[] GetTerminals(ParserRuleContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            TerminalNodeImpl[] terminals = context.children
                .Where(ch => ch is TerminalNodeImpl)
                .Select(ch => ch as TerminalNodeImpl)
                .ToArray();

            if (terminals is null || !terminals.Any())
            {
                throw new InvalidSyntaxException("Invalid ctor/func param declaration.");
            }

            return terminals;
        }

        /// <summary>
        /// Get ctor/function param name.
        /// </summary>
        /// <param name="context"> Ctor/func param definition context. </param>
        /// <returns> Ctor/func param name. </returns>
        private string GetName(TerminalNodeImpl[] terminals)
        {
            try
            {
                string name = terminals
                    .SingleOrDefault(t => !Terminals.Contains(t.GetText()))
                    .GetText();
                return name ?? throw new InvalidSyntaxException(
                    "Invalid ctor/func param declaration: param name expected.");
            }
            catch (InvalidOperationException)
            {
                throw new InvalidSyntaxException(
                    "Invalid ctor/func param declaration: param name expected.");
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
                return terminals.SingleOrDefault(t => DefKeywords.Contains(t.GetText()))?.GetText() switch
                {
                    "var" => true,
                    "val" => false,
                    null => false,
                    _ => throw new NotImplementedException(),
                };
            }
            catch (InvalidOperationException)
            {
                throw new InvalidSyntaxException(
                    "Invalid ctor param declaration: var/val keyword expected.");
            }
        }

        /// <summary>
        /// Get ctor param access modifier if stated. 
        /// </summary>
        /// <param name="context"> Ctor param definition context. </param>
        /// <returns> Ctor param access modifier if it stated in definition 
        /// and current symbol is ctor param, otherwise AccessModifier.None. </returns>
        private AccessModifier GetAccessModifier(ClassParamContext context)
        {
            if (context is null) return AccessModifier.None;

            TerminalNodeImpl[] terminals = GetTerminals(context);
            string def = terminals.SingleOrDefault(t => DefKeywords.Contains(t.GetText()))?.GetText();
            string modifier = context.modifier()?.FirstOrDefault()?.accessModifier()?.GetText();

            return modifier switch
            {
                null => def is null ? AccessModifier.None : AccessModifier.Public,
                "private" => AccessModifier.Private,
                "protected" => AccessModifier.Protected,
                _ => throw new InvalidSyntaxException(
                                  "Invalid class ctor param declaration: access modifier expected."),
            };
        }

        /// <summary>
        /// Get type symbol from class ctor param definition context.
        /// </summary>
        /// <param name="context"> Class ctor param definition context </param>
        /// <param name="scope"> Scope of class ctor param definition (class inner scope). </param>
        /// <returns> Type symbol. </returns>
        private SymbolBase GetType(ParserRuleContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            Type_Context type = context switch
            {
                ClassParamContext cp => cp.paramType()?.type_(),
                ParamContext p => p.paramType()?.type_(),
                _ => throw new NotImplementedException(),
            };

            _ = type ?? throw new InvalidSyntaxException(
                "Invalid ctor/func param declaration: type expected.");

            return GetType(type, Scope, out _unresolvedTypeName);
        }
    }
}
