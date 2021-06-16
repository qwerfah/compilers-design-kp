using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Compiler.Exceptions;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Symbol.Variable;
using Compiler.SymbolTable.Table;
using Compiler.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Symbol
{
    /// <summary>
    /// Represents function symbol.
    /// </summary>
    public class FunctionSymbol : SymbolBase
    {
        /// <summary>
        /// Function return type symbol.
        /// </summary>
        public SymbolBase ReturnType { get; private set; }

        /// <summary>
        /// Function body scope.
        /// Does not set during symbol istantiation because function 
        /// signature resolution precedes function body resolution.
        /// </summary>
        public Scope InnerScope { get; }

        /// <summary>
        /// Contains all function overloads in current scope if exisit.
        /// </summary>
        private List<FunctionSymbol> _overloads = new();

        /// <summary>
        /// Contains all function overloads in current scope if exisit.
        /// </summary>
        public IEnumerable<FunctionSymbol> Overloads
        {
            get
            {
                foreach (var overload in _overloads) yield return overload;
            }
        }

        /// <summary>
        /// Contains function return type name if it wasn't resolved during first pass.
        /// </summary>
        private string _unresolvedReturnType = null;

        public bool IsVisited { get; set; } = false;

        /// <summary>
        /// Constructs function symbol from function definition context.
        /// </summary>
        /// <param name="context"> Function definition context. </param>
        /// <param name="scope"></param>
        public FunctionSymbol(ParserRuleContext context, Scope innerScope, Scope scope = null) 
            : base(context, scope)
        {
            if (context is not (FunDefContext or FunDclContext))
            {
                throw new ArgumentException("Function definition or declaration expected.");
            }

            Name = GetName(context);
            ReturnType = GetReturnType(context);
            InnerScope = innerScope ?? throw new ArgumentNullException(nameof(innerScope));
            _overloads.Add(this);
            InnerScope.Owner = InnerScope.Owner ?? this;
        }

        /// <summary>
        /// Constructs function symbol with given atttributes.
        /// </summary>
        /// <param name="name"> Function name. </param>
        /// <param name="accessMod"> Symbol access modifier (if it is a class method). </param>
        /// <param name="returnType"> Function return type. </param>
        /// <param name="innerScope"> Function inner scope. </param>
        /// <param name="context"> Expression definition context. </param>
        /// <param name="scope"> Function definition scope. </param>
        public FunctionSymbol(
            string name,
            AccessModifier accessMod,
            SymbolBase returnType,
            Scope innerScope,
            ParserRuleContext context,
            Scope scope)
            : base(name, accessMod, context, scope)
        {
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            InnerScope = innerScope ?? throw new ArgumentNullException(nameof(innerScope));
            InnerScope.Owner = InnerScope.Owner ?? this;
            _overloads.Add(this);
        }

        /// <summary>
        /// Add new overload to overload list.
        /// </summary>
        /// <param name="overload"> New current function overload 
        /// with the same name and scope. </param>
        public void AddOverload(FunctionSymbol overload)
        {
            _ = overload ?? throw new ArgumentNullException(nameof(overload));

            if (Name == overload.Name && Scope == overload.Scope)
            {
                _overloads.Add(overload);
            }
            else
            {
                throw new InvalidSyntaxException(
                    "Invalid overload: trying to add inccorect symbol as function overload.");
            }
        }

        /// <summary>
        /// Get function overload with specified argument types.
        /// </summary>
        /// <param name="args"> Overload argument types. </param>
        /// <returns> Corresponding overload. </returns>
        public SymbolBase GetOverload(IEnumerable<SymbolBase> args)
        {
            _ = args ?? throw new ArgumentNullException(nameof(args));

            return _overloads.SingleOrDefault(f =>
                f.InnerScope.ParamMap.Count == args.Count()
                && !f.InnerScope.ParamMap.Values.Select(p => p.Type).Except(args).Any());
        }

        /// <summary>
        /// Get function name from its definition context.
        /// </summary>
        /// <param name="context"> Function definition context. </param>
        /// <returns> Function name. </returns>
        private string GetName(ParserRuleContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            TerminalNodeImpl[] terminals = (context switch
            {
                FunDclContext dcl => dcl.funSig()?.children,
                FunDefContext def => def.funSig()?.children,
                _ => throw new NotImplementedException(),
            })
            ?.Where(ch => ch is TerminalNodeImpl)
            ?.Select(ch => ch as TerminalNodeImpl)
            ?.ToArray();

            if (terminals is null || terminals.Length != 1)
            {
                throw new InvalidSyntaxException(
                    $"Invalid function definition: name expected.");
            }

            return terminals.First().GetText();
        }

        /// <summary>
        /// Get function return type symbol from function definition context.
        /// </summary>
        /// <param name="context"> Function definition context. </param>
        /// <param name="scope"> Function definition scope. </param>
        /// <returns> Return type symbol or null if unable to resolve type at this moment. </returns>
        private SymbolBase GetReturnType(ParserRuleContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));
            _ = Scope ?? throw new ArgumentNullException(nameof(Scope));

            Type_Context type = context switch
            {
                FunDclContext dcl => dcl.type_(),
                FunDefContext def => def.type_(),
                _ => throw new NotImplementedException(),
            } ?? throw new InvalidSyntaxException(
                "Invalid function signature: procedure syntax is depricated.");

            return GetType(type, Scope, out _unresolvedReturnType);
        }

        /// <summary>
        /// Check function definition.
        /// Checks if it has definition in nonabstract class
        /// and if it is then performs typechecking for all expressions.
        /// </summary>
        /// <param name="context"></param>
        /// <returns> Type of last statement in definition 
        /// or null if function has no definition 
        /// (class is abstract or function marked as nonimplemented). </returns>
        private SymbolBase CheckDefinition(FunDefContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            return context.expr() switch
            {
                null => Scope.Owner switch
                {
                    ClassSymbolBase classSymbol => classSymbol.IsAbstract,
                    TypeSymbol typeSymbol => typeSymbol.AliasingType.IsAbstract,
                    _ => throw new NotImplementedException(),
                }
                ? null
                : throw new InvalidSyntaxException(
                    $"Define function {Name} or mark class as abstract."),
                { } expr when (expr.GetText() == "???") => null,
                { } expr => new ExprTypeDeductor().Deduct(expr, InnerScope),
            };
        }

        /// <summary>
        /// Resolve overload return types.
        /// </summary>
        public override void Resolve()
        {
            foreach (var overload in _overloads)
            {
                overload.ReturnType = ResolveType(ref overload._unresolvedReturnType) 
                    ?? overload.ReturnType
                    ?? throw new InvalidSyntaxException(
                        "Invalid function definition: can't resolve return type.");
            }
        }

        /// <summary>
        /// Resolve overload signatures and arguments.
        /// </summary>
        public override void PostResolve()
        {
            ResolveOverloads();
            if (Context is FunDefContext funDef)
            {
                SymbolBase type = CheckDefinition(Context as FunDefContext);
                if (type is { } && type != ReturnType)
                {
                    throw new InvalidSyntaxException(
                        $"Invalid function definition: {ReturnType.Name} " +
                        $"exprected but fucntion returns {type.Name}");
                }
            }
        }

        /// <summary>
        /// Checking overload signatures.
        /// If there are two overloads with same arguments, throws exeption.
        /// </summary>
        public void ResolveOverloads()
        {
            var pairs = _overloads.SelectMany((x, i) => _overloads.Skip(i + 1),
                (x, y) => Tuple.Create(x, y));

            foreach (var pair in pairs)
            {
                var argTypes1 = pair.Item1.InnerScope.ParamMap.Values.Select(p => p.Type).ToArray();
                var argTypes2 = pair.Item2.InnerScope.ParamMap.Values.Select(p => p.Type).ToArray();

                if (argTypes1.Length == argTypes2.Length && !argTypes1.Except(argTypes2).Any())
                {
                    throw new InvalidSyntaxException(
                        $"Invalid overload: two functions with name {Name} and same arguments.");
                }
            }
        }

        /// <summary>
        /// Defines function arguments as inner scope variables.
        /// Performs for each function overload.
        /// </summary>
        public void ResolveArguments()
        {
            foreach (var overload in _overloads)
            {
                foreach (var param in overload.InnerScope.ParamMap.Values)
                {
                    VariableSymbol symbol = new(
                        param.Name, 
                        param.AccessMod, 
                        param.Context, 
                        param.IsMutable, 
                        param.Type, 
                        param.Scope);

                    overload.InnerScope.Define(symbol);
                }
            }
        }

        /// <summary>
        /// Imitate function application to given types.
        /// </summary>
        /// <param name="argTypes"></param>
        /// <returns> Function overload return type 
        /// if argument types matches with any overload. </returns>
        public SymbolBase Apply(IEnumerable<SymbolBase> argTypes)
        {
            _ = argTypes ?? throw new ArgumentNullException(nameof(argTypes));

            foreach (var overload in _overloads)
            {
                if ((overload.InnerScope.ParamMap.Values.Count == argTypes.Count())
                    && (!overload.InnerScope.ParamMap.Values.Select(p => p.Type).Except(argTypes).Any()))
                {
                    return overload.ReturnType;
                }
            }

            throw new InvalidSyntaxException(
                $"Invalid expression: argument type mismatch for function {Name}."); ;
        }

        /// <summary>
        /// Get current function symbol instance string representation.
        /// </summary>
        /// <returns> Symbol string representation. </returns>
        public string ToStringSingle()
        {
            return $"{(AccessMod == AccessModifier.None ? string.Empty : AccessMod)} " +
                   $"def {Name}" +
                   $"({string.Join(", ", InnerScope.ParamMap.Values.Select(p => p.Type.Name))}) " +
                   $": {(ReturnType is { } ? ReturnType.Name : "None")}";
        }

        public override string ToString()
        {
            return string.Join("\n", _overloads.Select(f => f.ToStringSingle()));
        }
    }
}
