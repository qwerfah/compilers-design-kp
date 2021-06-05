using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Compiler.SymbolTable.Symbol;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable
{
    public class Scope
    {
        /// <summary>
        /// Unique scope identifier.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// Scope type (global/local).
        /// </summary>
        public ScopeType Type { get; }

        /// <summary>
        /// Enclosing scope for current scope.
        /// </summary>
        public Scope EnclosingScope { get; }

        /// <summary>
        /// Symbol map for current scope.
        /// </summary>
        protected Dictionary<string, SymbolBase> symbolMap = new();

        public Scope(ScopeType type, Scope enclosingScope = null)
        {
            Guid = Guid.NewGuid();
            Type = type;
            EnclosingScope = enclosingScope;
        }

        /// <summary>
        /// Define new variable symbol from parse tree node context for current scope.
        /// </summary>
        /// <param name="context"> Parse tree node context. </param>
        public void Define(ParserRuleContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            SymbolBase symbol = context switch
            {
                TmplDefContext => new ClassSymbol(context, this),
                FunDefContext => new FunctionSymbol(context, this),
                PatVarDefContext => new VariableSymbol(context, this),
                TypeDefContext => new TypeSymbol(context, this),
                _ => throw new NotImplementedException(),
            };

            symbolMap.Add(symbol.Name, symbol);
        }

        /// <summary>
        /// Define new variable symbol from its instance for current scope.
        /// </summary>
        /// <param name="symbol"> Symbol instance to define. </param>
        public void Define(SymbolBase symbol)
        {
            if (symbol is null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            symbol.Scope = this;
            symbolMap.Add(symbol.Name, symbol);
        }
    }
}
