using Compiler.SymbolTable.Symbol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.CallGraph
{
    public class CallGraphNode
    {
        public FunctionSymbol Function { get; set; }
        public HashSet<CallGraphNode> Calls { get; set; }
        public object Elements { get; internal set; }
    }
}
