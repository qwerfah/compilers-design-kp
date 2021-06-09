using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.CallGraph
{
    public class CallGraph
    {
        public List<CallGraphNode> Root { get; }

        public CallGraph()
        {
            Root = new();
        }
    }
}
