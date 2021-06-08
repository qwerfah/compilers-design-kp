using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol.Variable
{
    /// <summary>
    /// Represents local variable or class field definition or declaration.
    /// Symbol may be constructed only from ValDclContext, VarDclContex and PatVarDefContext. 
    /// </summary>
    class VariableSymbol : VariableSymbolBase
    {
        public VariableSymbol(ParserRuleContext context, Scope scope)
            : base(context, scope)
        {
            AccessMod = GetAccessModifier(context);
            IsMutable = CheckMutability(context);
            Type = GetType(context, scope);
            Name = GetName(context, scope);
        }

        public VariableSymbol(string name, bool isMutable, SymbolBase type, AccessModifier? access)
            : base(name, isMutable, type, access)
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
                ParserRuleContext c when (c is ValDclContext || c is VarDclContext) => context.Parent.GetChild(0),
                PatVarDefContext => context.GetChild(0),
                _ => throw new NotImplementedException(),
            };

            _ = subtree ?? throw new InvalidParseTreeException($"Invalid subtree for val declaration {Guid}.");

            if (!"valvar".Contains(subtree.GetText()))
            {
                throw new InvalidKeywordException($"Invalid keyword in val definition {Guid}.");
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
        private string GetName(ParserRuleContext context, Scope scope)
        {
            return context switch
            {
                ValDclContext valDcl => GetName(valDcl.ids(), scope),
                VarDclContext varDcl => GetName(varDcl.ids(), scope),
                PatVarDefContext patVarDef => GetName(patVarDef),
                _ => throw new NotImplementedException(),
            }
            ?? throw new InvalidParseTreeException($"Invalid subtree for variable definition {Guid}.");
        }

        /// <summary>
        /// Get varialbe name from val or var single/multiple declaration.
        /// If declaration is multiple, creates symbols for all varialbes in specified scope except first.
        /// In this case it is assumed that current variable mutability and type already defined.
        /// </summary>
        /// <param name="ids"> Context that contains variable identifiers. </param>
        /// <param name="scope"> Scope of variable declaration. </param>
        /// <returns> Name of the first declared variable. </returns>
        private string GetName(IdsContext ids, Scope scope)
        {
            if (ids is null || !ids.children.Any())
            {
                throw new InvalidParseTreeException("Invalid multiple variables declaration.");
            }

            for (int i = 2; i < ids.ChildCount; i += 2)
            {
                string name = ids.GetChild(i).GetText();
                scope.Define(new VariableSymbol(name, IsMutable, Type, AccessMod));
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
                ?? throw new InvalidParseTreeException($"Invalid subtree for variable definition {Guid}.");
        }

        /// <summary>
        /// Get type symbol from mutable/immutable varialbe definition/declaration context.
        /// </summary>
        /// <param name="context"> Mutable/immutable varialbe declaration/definition 
        /// context that may be ValDclContext, VarDclContext or PatVarDefContext.</param>
        /// <param name="scope"> Scope of variable declaration/definition. </param>
        /// <returns> Symbol of variable type. </returns>
        private SymbolBase GetType(ParserRuleContext context, Scope scope)
        {
            SymbolBase type = context switch
            {
                ValDclContext valDcl => GetType(valDcl.type_(), scope),
                VarDclContext varDcl => GetType(varDcl.type_(), scope),
                PatVarDefContext patVarDef => GetType(patVarDef.varDef()?.patDef()?.type_() ?? patVarDef.patDef()?.type_(), scope),
                _ => throw new NotImplementedException(),
            };

            if (type is null)
            {
                if (context is PatVarDefContext patVarDef)
                {
                    type = DeductType(patVarDef, scope);

                    if (type is null)
                    {
                        Console.Error.WriteLine($"Can't deduct type for variable {Guid}");
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Variable declaration {Guid} does not have  explicitly specified type.");
                }
            }

            return type;
        }

        /// <summary>
        /// Get variable type from type context.
        /// </summary>
        /// <param name="context"> Type context. </param>
        /// <param name="scope"> Scope of variable declaration/definition. </param>
        /// <returns> Symbol of declared type. </returns>
        private SymbolBase GetType(Type_Context context, Scope scope)
        {
            if (context is null) return null;

            string typeName = context
                ?.infixType()
                ?.compoundType()?.First()
                ?.annotType()?.First()
                ?.simpleType()
                ?.stableId()?.GetText();

            _ = typeName ?? throw new InvalidParseTreeException($"Invalid type subtree for variable definition {Guid}.");

            SymbolBase typeSymbol = scope.GetSymbol(typeName, SymbolType.Class)
                ?? scope.GetSymbol(typeName, SymbolType.Type)
                ?? scope.GetSymbol(typeName, SymbolType.Trait);

            if (typeSymbol is null)
            {
                Console.Error.WriteLine($"Undefined type {typeName} for variable definition {Guid}.");
                _unresolvedTypeName = typeName;
            }

            return typeSymbol;
        }

        /// <summary>
        /// Deduct varialbe type if it is not stated in its definition.
        /// Works only with variable definition (declaration MUST have explicitly specified variable type).
        /// </summary>
        /// <param name="context"> Varialbe definition context. </param>
        /// <param name="scope"> Scope of variable definition. </param>
        /// <returns></returns>
        private SymbolBase DeductType(PatVarDefContext context, Scope scope)
        {
            // TODO: auto type deduction
            return null;
        }

        /// <summary>
        /// Get access modifier for variable declaration/definition.
        /// </summary>
        /// <param name="context"> Varialbe declaration/definition context. </param>
        /// <returns> Variable modifier. </returns>
        private AccessModifier? GetAccessModifier(ParserRuleContext context)
        {
            TemplateStatContext templateStat = context.Parent?.Parent as TemplateStatContext;
            
            if (templateStat is null)
            {
                Console.WriteLine("Variable is not a class member.");
                return null;
            }

            ModifierContext[] modifiers = templateStat?.modifier();
            string modifier = (modifiers is null || !modifiers.Any()) 
                ? null 
                : modifiers.First()?.accessModifier()?.GetText();

            return modifier switch
            {
                null => AccessModifier.Public,
                "private" => AccessModifier.Private,
                "protected" => AccessModifier.Protected,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
