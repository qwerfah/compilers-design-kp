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
        /// <summary>
        /// Contains name of first class/trait in extends-chain
        /// (that stated after "extends" keyword) if it wasn't resolved during first pass.
        /// </summary>
        protected string _unresolvedParent = null;

        /// <summary>
        /// Contains names of traits in with-chain that weren't resolved during first pass.
        /// </summary>
        protected List<string> _unresolvedTraits = new();

        /// <summary>
        /// Type symbol for parent class/trait (that stated after "extends" keyword).
        /// </summary>
        public SymbolBase Parent { get; set; } = null;

        /// <summary>
        /// Type symbols for traits in with-chain (that stated after "with" keywords).
        /// </summary>
        public List<SymbolBase> Traits { get; set; } = null;

        /// <summary>
        /// Constructs class/object/template/trait symbol with given name in specified scope.
        /// </summary>
        /// <param name="name"> Class/object/template/trait name. </param>
        /// <param name="scope"> Scope of class/object/template/trait definition. </param>
        public ClassSymbolBase(string name, Scope scope = null) : base(name, scope)
        {
        }

        /// <summary>
        /// Constructs class/object/template/trait symbol 
        /// from given definition context in specified scope.
        /// </summary>
        /// <param name="context"> Class/object/template/trait definition context. </param>
        /// <param name="scope"> Scope of class/object/template/trait definition. </param>
        public ClassSymbolBase(TmplDefContext context, Scope scope) : base(context, scope)
        {
        }

        /// <summary>
        /// Get class/object/trait/template name from definition context.
        /// </summary>
        /// <param name="context"> Class/object/trait/template definition context. </param>
        /// <returns> Definition name. </returns>
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
        protected (SymbolBase, List<SymbolBase>) GetParents(ClassParentsContext context)
        {
            if (context is null)
            {
                Console.WriteLine($"Class {Name} don't have parents.");
                return (null, null);
            }

            string parent = context.constr()?.annotType()?.simpleType()?.stableId().GetText();
            List<string> traits = context.annotType()?.Select(t => t?.GetText()).ToList();

            if (parent is null || traits.Any(t => t is null))
            {
                throw new InvalidParseTreeException("Invalid subtree for class parent definition.");
            }

            SymbolBase parentSymbol = Scope.GetSymbol(
                parent, SymbolType.Class) ?? Scope.GetSymbol(parent, SymbolType.Trait);

            if (parentSymbol is null)
            {
                _unresolvedParent = parent;
            }

            List<SymbolBase> traitSymbols = new();

            foreach (var trait in traits)
            {
                SymbolBase traitSymbol = Scope.GetSymbol(trait, SymbolType.Trait);

                if (traitSymbol is null)
                {
                    _unresolvedTraits.Add(trait);
                }
                else
                {
                    traitSymbols.Add(traitSymbol);
                }
            }

            return parentSymbol is { }
                ? (parentSymbol, traitSymbols.Any() ? traitSymbols : null) 
                : (null, null);
        }

        public override void Resolve()
        {
            if (_unresolvedParent is { })
            {
                ResolveParent();
            }

            if (_unresolvedTraits is { } && _unresolvedTraits.Any())
            {
                ResolveTraits();
            }
        }

        /// <summary>
        /// Resolve parent class/trait symbol if unresolved.
        /// </summary>
        private void ResolveParent()
        {
            if (_unresolvedParent is null) return;

            SymbolBase parentSymbol = Scope.GetSymbol(
                _unresolvedParent, SymbolType.Class) ?? Scope.GetSymbol(_unresolvedParent, SymbolType.Trait);

            if (parentSymbol is null)
            {
                Console.Error.WriteLine($"Undefined symbol {_unresolvedParent} in definition of symbol {Name}");
            }
            else
            {
                Parent = parentSymbol;
            }
        }

        /// <summary>
        /// Resolve all unresolved trait symbols in class symbol definition.
        /// </summary>
        private void ResolveTraits()
        {
            if (_unresolvedTraits is null || !_unresolvedTraits.Any()) return;

            List<SymbolBase> traits = new();

            foreach (var trait in _unresolvedTraits)
            {
                SymbolBase traitSymbol = Scope.GetSymbol(trait, SymbolType.Trait);

                if (traitSymbol is null)
                {
                    Console.Error.WriteLine($"Undefined symbol {trait} in definition of symbol {Name}");
                }
                else
                {
                    traits.Add(traitSymbol);
                }
            }

            if (traits.Any())
            {
                Traits = Traits ?? traits;
            }
        }
    }
}
