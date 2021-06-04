using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Scope type (global/local).
        /// </summary>
        public ScopeType Type { get; }

        /// <summary>
        /// 
        /// </summary>
        public Scope EnclosingScope { get; }

        /// <summary>
        /// 
        /// </summary>
        //protected Map<String, Symbol> symbolMap = new LinkedHashMap<String, Symbol>();

        public Scope(ScopeType type, Scope enclosingScope)
        {
            Type = type;
            EnclosingScope = enclosingScope;
        }
    }
}
