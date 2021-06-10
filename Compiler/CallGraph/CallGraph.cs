using System;

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
