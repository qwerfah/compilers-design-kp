using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol
{
    /// <summary>
    /// Represents function symbol.
    /// </summary>
    public class FunctionSymbol : SymbolBase
    {
        /// <summary>
        /// Function return type symbol.
        /// </summary>
        public SymbolBase ReturnType { get; set; }

        /// <summary>
        /// Contains function return type name if it wasn't resolved during first pass.
        /// </summary>
        private string _unresolvedReturnType = null;

        /// <summary>
        /// Constructs function symbol from function definition context.
        /// </summary>
        /// <param name="context"> Function definition context. </param>
        /// <param name="scope"></param>
        public FunctionSymbol(FunDefContext context, Scope scope) : base(context, scope)
        {
            Name = GetName(context);
            ReturnType = GetReturnType(context, scope);
        }
        
        /// <summary>
        /// Get function name from its definition context.
        /// </summary>
        /// <param name="context"> Function definition context. </param>
        /// <returns> Function name. </returns>
        private string GetName(FunDefContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            TerminalNodeImpl[] terminals = context.funSig()?.children
                ?.Where(ch => ch is TerminalNodeImpl)
                ?.Select(ch => ch as TerminalNodeImpl)
                ?.ToArray();

            if (terminals is null || terminals.Length != 1)
            {
                throw new InvalidSyntaxException(
                    $"Invalid function definition: name expected.");
            }

            return terminals.First().GetText();
        }

        /// <summary>
        /// Get function return type symbol from function definition context.
        /// </summary>
        /// <param name="context"> Function definition context. </param>
        /// <param name="scope"> Function definition scope. </param>
        /// <returns> Return type symbol or null if unable to resolve type at this moment. </returns>
        private SymbolBase GetReturnType(FunDefContext context, Scope scope)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = scope ?? throw new ArgumentNullException(nameof(scope));

            return GetType(context.type_(), scope, out _unresolvedReturnType);
        }

        public override void Resolve()
        {
            ReturnType = ResolveType(_unresolvedReturnType) ?? ReturnType 
                ?? throw new InvalidSyntaxException(
                    "Invalid function definition: can't resolve return type.");
        }
    }
}
