using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Class
{
    /// <summary>
    /// Represents class symbol definition (class/object/template/trait).
    /// </summary>
    public abstract class ClassSymbolBase : SymbolBase
    {
        protected string UnresolvedParent = null;
        protected List<string> UnresolvedTraits = new();

        public List<SymbolBase> Parents { get; set; } = null;

        public ClassSymbolBase(TmplDefContext context, Scope scope) : base(context, scope)
        {
        }

        protected string GetName(ParserRuleContext context)
        {
            if (context.GetChild(0) is TerminalNodeImpl impl)
            {
                return impl.GetText();
            }

            throw new InvalidCastException(
                    "Can't cast first child of TmplDefContext to TerminalNodeImpl.");
        }

        /// <summary>
        /// Get class parent (after extends keyword) and inherited traits (with-chain).
        /// </summary>
        /// <param name="context"> Class parents context. </param>
        /// <returns> List of inherited class and traits, that are resolved on this stage of analysis, 
        /// or null, if class not extends or if unable to resolve any parent symbol on this stage of analysis. </returns>
        protected List<SymbolBase> GetParents(ClassParentsContext context, Scope scope)
        {
            if (context is null)
            {
                Console.WriteLine($"Class {Name} don't have parents.");
                return null;
            }

            string parent = context.constr()?.annotType()?.simpleType()?.stableId().GetText();
            List<string> traits = context.annotType()?.Select(t => t?.GetText()).ToList();

            if (parent is null || traits.Any(t => t is null))
            {
                throw new InvalidParseTreeException("Invalid subtree for class parent definition.");
            }

            List<SymbolBase> parents = new();

            SymbolBase parentSymbol = scope.GetSymbol(parent, SymbolType.Class) ?? scope.GetSymbol(parent, SymbolType.Trait);

            if (parentSymbol is null)
            {
                UnresolvedParent = parent;
            }
            else
            {
                parents.Add(parentSymbol);
            }

            foreach (var trait in traits)
            {
                SymbolBase traitSymbol = scope.GetSymbol(trait, SymbolType.Trait);

                if (traitSymbol is null)
                {
                    UnresolvedTraits.Add(trait);
                }
                else
                {
                    parents.Add(traitSymbol);
                }
            }

            return parents.Any() ? parents : null;
        }
    }
}
