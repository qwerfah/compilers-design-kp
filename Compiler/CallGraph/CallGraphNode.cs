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
        public FunctionSymbol Node { get; }
        public List<CallGraphNode> Children { get; }

        public CallGraphNode(FunctionSymbol function)
        {
            _ = function ?? throw new ArgumentNullException(nameof(function));
            Children = new();
        }
    }
}
