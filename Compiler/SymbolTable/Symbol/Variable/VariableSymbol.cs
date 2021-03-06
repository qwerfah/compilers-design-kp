using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Table;
using Compiler.Types;
using System;
using System.Linq;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Variable
{
    /// <summary>
    /// Represents local variable or class field definition or declaration.
    /// Symbol may be constructed only from ValDclContext, VarDclContex and PatVarDefContext. 
    /// </summary>
    public class VariableSymbol : VariableSymbolBase
    {
        public VariableSymbol(ParserRuleContext context, Scope scope)
            : base(context, scope)
        {
            if (context is not ValDclContext
                && context is not VarDclContext
                && context is not PatVarDefContext)
            {
                throw new ArgumentException(
                    "Invalid context type: only VarDcl, ValDcl or PatVarDef context are acceptable.");
            }

            IsMutable = CheckMutability(context);
            Type = GetType(context);
            Name = GetName(context);
        }

        public VariableSymbol(
            string name,
            AccessModifier accessMod,
            ParserRuleContext context,
            bool isMutable,
            SymbolBase type,
            Scope scope)
            : base(name, accessMod, context, isMutable, type, scope)
        {
        }

        public VariableSymbol(ParamSymbol symbol) : 
            base(symbol.Name,
                 symbol.AccessMod, 
                 symbol.Context, 
                 symbol.IsMutable, 
                 symbol.Type, 
                 symbol.Scope)
        {
        }

        /// <summary>
        /// Check mutability of variable that is declared/defined.
        /// </summary>
        /// <param name="context"> Variable declaration/definition context 
        /// that may be ValDclContext, VarDclContext or PatVarDefContext.  </param>
        /// <returns> True if variable is mutable, otherwise - false. </returns>
        private bool CheckMutability(ParserRuleContext context)
        {
            IParseTree subtree = context switch
            {
                ParserRuleContext c when
                    (c is ValDclContext || c is VarDclContext) => context.Parent.GetChild(0),
                PatVarDefContext => context.GetChild(0),
                _ => throw new NotImplementedException(),
            };

            _ = subtree ?? throw new InvalidSyntaxException(
                $"Invalid variable declaration/definition: val/var expected.");

            if (!DefKeywords.Contains(subtree.GetText()))
            {
                throw new InvalidKeywordException(
                    $"Invalid variable declaration/definition: val/var expected.");
            }

            return subtree.GetText() == "var";
        }

        /// <summary>
        /// Get variable name from mutable/immutable variable declaration/definition.
        /// </summary>
        /// <param name="context"> Mutable/immutable variable declaration/definition 
        /// context that may be ValDclContext, VarDclContext or PatVarDefContext. </param>
        /// <param name="scope"> Scope of variable declaration/definition. </param>
        /// <returns> Name of current variable. </returns>
        private string GetName(ParserRuleContext context)
        {
            return context switch
            {
                ValDclContext valDcl => GetName(valDcl.ids()),
                VarDclContext varDcl => GetName(varDcl.ids()),
                PatVarDefContext patVarDef => GetName(patVarDef),
                _ => throw new NotImplementedException(),
            }
            ?? throw new InvalidSyntaxException(
                $"Invalid variable declaration/definition: name expected.");
        }

        /// <summary>
        /// Get varialbe name from val or var single/multiple declaration.
        /// If declaration is multiple, creates symbols for all varialbes in specified scope except first.
        /// In this case it is assumed that current variable mutability and type already defined.
        /// </summary>
        /// <param name="ids"> Context that contains variable identifiers. </param>
        /// <param name="scope"> Scope of variable declaration. </param>
        /// <returns> Name of the first declared variable. </returns>
        private string GetName(IdsContext ids)
        {
            if (ids is null || !ids.children.Any())
            {
                throw new InvalidSyntaxException(
                    $"Invalid variable declaration/definition: name expected.");
            }

            for (int i = 2; i < ids.ChildCount; i += 2)
            {
                string name = ids.GetChild(i).GetText();
                Scope.Define(new VariableSymbol(name, AccessMod, Context, IsMutable, Type, Scope));
            }

            return ids.GetChild(0).GetText();
        }

        /// <summary>
        /// Get name from mutable/immutable varialbe definition context.
        /// </summary>
        /// <param name="context"> Variable definition context. </param>
        /// <returns> Name of the defined variable. </returns>
        private string GetName(PatVarDefContext context)
        {
            if (context is null) return null;

            PatDefContext patDef = context.varDef()?.patDef() ?? context.patDef();

            return patDef?.pattern2()?.First()?.GetText()
                ?? throw new InvalidSyntaxException(
                    $"Invalid variable declaration/definition: name expected.");
        }

        /// <summary>
        /// Get type symbol from mutable/immutable varialbe definition/declaration context.
        /// </summary>
        /// <param name="context"> Mutable/immutable varialbe declaration/definition 
        /// context that may be ValDclContext, VarDclContext or PatVarDefContext.</param>
        /// <param name="scope"> Scope of variable declaration/definition. </param>
        /// <returns> Symbol of variable type. </returns>
        private SymbolBase GetType(ParserRuleContext context)
        {
            // Get type from explicit type declaration.
            Type_Context type = context switch
            {
                ValDclContext valDcl => valDcl.type_()
                    ?? throw new InvalidSyntaxException($"Invalid variable declaration: type expected."),
                VarDclContext varDcl => varDcl.type_()
                    ?? throw new InvalidSyntaxException($"Invalid variable declaration: type expected."),
                PatVarDefContext patVarDef => patVarDef.varDef()?.patDef()?.type_()
                    ?? patVarDef.patDef()?.type_(),
                _ => throw new NotImplementedException(),
            };

            return GetType(type, Scope, out _unresolvedTypeName);
        }

        public override void Resolve()
        {
            SymbolBase resolvedType = ResolveType(ref _unresolvedTypeName) ?? Type;
            // Check if variable must have definition
            if (Context is ValDclContext or VarDclContext)
            {
                _ = Scope.Owner switch
                {
                    ClassSymbolBase classSymbol when (!classSymbol.IsAbstract) =>
                        throw new InvalidSyntaxException($"Define variable {Name} or mark class as abstract."),
                    TypeSymbol typeSymbol when (!typeSymbol.AliasingType.IsAbstract) =>
                        throw new InvalidSyntaxException($"Define variable {Name} or mark class as abstract."),
                    FunctionSymbol =>
                        throw new InvalidSyntaxException($"Define function loacal variable {Name}."),
                    _ => true,
                };

                return;
            }
            // Check variable definition and compare declared and deducted types
            if (Context is PatVarDefContext context)
            {
                SymbolBase deductedType = DeductType(context);

                Type = (resolvedType, deductedType) switch
                {
                    (null, null) => throw new InvalidSyntaxException(
                        "Invalid variable definition: can't define variable type."),
                    ({ },  null) => resolvedType,
                    (null,  { }) => deductedType,
                    ({ },   { }) => (resolvedType == deductedType)
                        ? resolvedType
                        : throw new InvalidSyntaxException(
                            $"Invalid variable definition: specified type {resolvedType.Name} " +
                            $"does not match with deducted type {deductedType.Name}."),
                };
            }
        }

        /// <summary>
        /// Deduct varialbe type if it is not stated in its definition.
        /// Works only with variable definition (declaration MUST have explicitly specified variable type).
        /// </summary>
        /// <param name="context"> Varialbe definition context. </param>
        /// <param name="scope"> Scope of variable definition. </param>
        /// <returns> Deducted variable type if variable definition has expression, otherwise null. </returns>
        private SymbolBase DeductType(PatVarDefContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            Expr1Context expr = context.varDef()?.patDef()?.expr()?.expr1()
                ?? context.patDef()?.expr()?.expr1();

            if (expr is null) throw new InvalidSyntaxException(
                "Invalid variable definition: expression expected.");

            return new ExprTypeDeductor().Deduct(expr, Scope)
                ?? throw new InvalidSyntaxException(
                    "Invalid variable definition: expression expected.");
        }
    }
}
