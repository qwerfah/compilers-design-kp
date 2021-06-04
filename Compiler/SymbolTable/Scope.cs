using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler.SymbolTable.Symbol;

namespace Compiler.SymbolTable
{
    /// <summary>
    /// Scope type (global/local).
    /// </summary>
    public enum ScopeType
    {
        Global,
        Local
    }

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

        public Scope(ScopeType type, Scope enclosingScope)
        {
            Guid = Guid.NewGuid();
            Type = type;
            EnclosingScope = enclosingScope;
        }

        /// <summary>
        /// Define new variable symbol for current scope.
        /// </summary>
        /// <param name="name"> Variable  </param>
        /// <param name="parameters"></param>
        public void Define(String name, IEnumerable<String> parameters)
        {
            string strParams = parameters.ToString();
            //Symbol symbol = new Symbol(null, name + strParams, null);
            //define(symbol);
        }

        private void define(SymbolBase symbol)
        {
            //symbol.setScope(this);
            //symbolMap.put(symbol.name, symbol);
        }
    }
}
