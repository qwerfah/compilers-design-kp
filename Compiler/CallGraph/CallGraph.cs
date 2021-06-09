using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.CallGraph
{
    public class CallGraph
    {
        public CallGraphNode Root { get; }

        public CallGraph(CallGraphNode root)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }
    }
}
