using Antlr4.Runtime;
using Compiler.Exceptions;
using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Symbol.Variable;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable.Table
{
    /// <summary>
    /// Represents symbol scope. Scope can be limited by class, 
    /// function of block body. Besides that, there is one global scope, 
    /// that can contains only class/object/trait definitions.
    /// </summary>
    public class Scope
    {
        /// <summary>
        /// Unique scope identifier.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// Scope type (global/local).
        /// </summary>
        public ScopeType Type { get; }

        /// <summary>
        /// Enclosing scope for current scope.
        /// </summary>
        public Scope EnclosingScope { get; }

        public List<Scope> InnerScopes { get; } 

        /// <summary>
        /// Contains symbol (class/object/trait/function) that current scope belongs to.
        /// For global scope contains null.
        /// </summary>
        public SymbolBase Owner { get; set; }

        /// <summary>
        /// Class symbol map for current scope.
        /// </summary>
        public Dictionary<string, ClassSymbol> ClassMap = new();

        /// <summary>
        /// Object symbol map for current scope.
        /// </summary>
        public Dictionary<string, ObjectSymbol> ObjectMap = new();

        /// <summary>
        /// Trait symbol map for current scope.
        /// </summary>
        public Dictionary<string, TraitSymbol> TraitMap = new();

        /// <summary>
        /// Function symbol map for current scope.
        /// </summary>
        public Dictionary<string, FunctionSymbol> FunctionMap = new();

        /// <summary>
        /// Varialbe symbol map for current scope.
        /// </summary>
        public Dictionary<string, VariableSymbol> VariableMap = new();

        /// <summary>
        /// Param symbol map for current scope.
        /// </summary>
        public Dictionary<string, ParamSymbol> ParamMap = new();

        /// <summary>
        /// Type symbol map for current scope.
        /// </summary>
        public Dictionary<string, TypeSymbol> TypeMap = new();

        public List<string> Errors { get; } = new();

        public Scope(ScopeType type, Scope enclosingScope = null)
        {
            if (type == ScopeType.Local && enclosingScope is null)
            {
                throw new ArgumentNullException(nameof(enclosingScope));
            }

            Guid = Guid.NewGuid();
            Type = type;
            EnclosingScope = enclosingScope;
            InnerScopes = new();
        }

        /// <summary>
        /// Define new symbol without inner scope from parse tree node context for current scope.
        /// </summary>
        /// <param name="context"> Parse tree node context. </param>
        public SymbolBase Define(ParserRuleContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            try
            {
                SymbolBase symbol = context switch
                {
                    // ClassDefContext => new ClassSymbol(context as ClassDefContext, null, this),
                    // ObjectDefContext => new ObjectSymbol(context as ObjectDefContext, null, this),
                    // FunDefContext => new FunctionSymbol(context as FunDefContext, null, this),
                    ParserRuleContext c when (c is ParamContext || c is ClassParamContext) =>
                        new ParamSymbol(context, this),
                    ParserRuleContext c when (c is ValDclContext || c is VarDclContext || c is PatVarDefContext) =>
                        new VariableSymbol(context, this),
                    TypeDefContext => new TypeSymbol(context as TypeDefContext, this),
                    _ => throw new NotImplementedException(),
                };

                return Define(symbol);
            }
            catch (Exception e)
            {
                Errors.Add(
                    $"Error at {context.Start.Line}:{context.Start.Column} - {e.Message}");
            }

            return null;
        }

        /// <summary>
        /// Define new symbol of any type from its instance for current scope.
        /// </summary>
        /// <param name="symbol"> Symbol instance to define. </param>
        public SymbolBase Define(SymbolBase symbol)
        {
            _ = symbol ?? throw new ArgumentNullException(nameof(symbol));

            if (!IsNameAvailable(symbol))
            {
                Errors.Add(
                    $"Symbol with name {symbol.Name} already defined in current scope.");
                return null;
            }

            switch (symbol)
            {
                case ClassSymbol s: ClassMap.Add(s.Name, s); break;
                case ObjectSymbol s: ObjectMap.Add(s.Name, s); break;
                case TraitSymbol s: TraitMap.Add(s.Name, s); break;
                case FunctionSymbol s: 
                    try
                    {
                        FunctionMap.Add(s.Name, s);
                    }
                    catch (ArgumentException)
                    {
                        FunctionMap[s.Name].AddOverload(s);
                    }
                    break;
                case VariableSymbol s: VariableMap.Add(s.Name, s); break;
                case ParamSymbol s: ParamMap.Add(s.Name, s); break;
                case TypeSymbol s: TypeMap.Add(s.Name, s); break;
                default: throw new NotImplementedException();
            }

            return symbol;
        }

        /// <summary>
        /// Check if symbol name doesn't conflict with other symbol names in current scope.
        /// In different scopes symbols can have nay names.
        /// </summary>
        /// <param name="symbol"> Symbol with name to check. </param>
        /// <returns> True if symbol name doesn't conflict with 
        /// others in current scope, otherwise false. </returns>
        private bool IsNameAvailable(SymbolBase symbol)
        {
            _ = symbol ?? throw new ArgumentNullException(nameof(symbol));
            string name = symbol?.Name ?? throw new ArgumentNullException(nameof(symbol));

            return symbol switch
            {
                ClassSymbol => !ClassMap.ContainsKey(name) && !TraitMap.ContainsKey(name) && !TypeMap.ContainsKey(name),
                ObjectSymbol => !ObjectMap.ContainsKey(name) && !VariableMap.ContainsKey(name),
                TraitSymbol => !TraitMap.ContainsKey(name) && !ClassMap.ContainsKey(name) && !TypeMap.ContainsKey(name),
                FunctionSymbol => true, // Overloads are allowed
                VariableSymbol => !VariableMap.ContainsKey(name),
                ParamSymbol => !ParamMap.ContainsKey(name),
                TypeSymbol => !TypeMap.ContainsKey(name) && !ClassMap.ContainsKey(name) && !TraitMap.ContainsKey(name),
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// Looking for symbol by name in current and all enclousing scopes.
        /// If scope is class scope, search performs also in class parent inner scope.
        /// </summary>
        /// <param name="name"> Symbol name. </param>
        /// <param name="name"> Symbol name. </param>
        /// <param name="enclose"> If true, searching symbol also in enclosing scope. </param>
        /// <returns> Symbol with corresponding name in the 
        /// nearest enclosing scope or null if not found. </returns>
        public SymbolBase GetSymbol(string name,
                                    SymbolType type,
                                    bool enclose = true,
                                    IEnumerable<SymbolBase> args = null)
        {
            SymbolBase symbol = type switch
            {
                SymbolType.Class => ClassMap.ContainsKey(name)
                    ? ClassMap[name] : enclose ? EnclosingScope?.GetSymbol(name, type) : null,
                SymbolType.Object => ObjectMap.ContainsKey(name) 
                    ? ObjectMap[name] : enclose ? EnclosingScope?.GetSymbol(name, type) : null,
                SymbolType.Trait => TraitMap.ContainsKey(name) 
                    ? TraitMap[name] : enclose ? EnclosingScope?.GetSymbol(name, type) : null,
                SymbolType.Function => FunctionMap.ContainsKey(name)
                    ? args is { } 
                        ? FunctionMap[name].GetOverload(args) 
                        : FunctionMap[name]
                    : enclose 
                        ? EnclosingScope?.GetSymbol(name, type, true, args) 
                        : null,
                SymbolType.Variable => VariableMap.ContainsKey(name) 
                    ? VariableMap[name] : enclose ? EnclosingScope?.GetSymbol(name, type) : null,
                SymbolType.Type => TypeMap.ContainsKey(name) 
                    ? TypeMap[name] : enclose ? EnclosingScope?.GetSymbol(name, type) : null,
                _ => throw new NotImplementedException(),
            };

            if (symbol is null && Owner is { })
            {
                var owner = Owner;
                while (owner is not null && owner is not ClassSymbolBase) owner = owner.Scope.Owner;
                if (owner is null) return null;

                symbol = ((ClassSymbolBase)owner)
                    .Parent
                    ?.InnerScope
                    .GetSymbol(name, type, enclose, args);

                symbol = symbol switch
                {
                    null => null,
                    _ when (symbol.AccessMod == AccessModifier.Public) => symbol,
                    _ when (symbol.AccessMod == AccessModifier.Protected) => symbol,
                    _ => null,
                };
            }

            return symbol;
        }

        /// <summary>
        /// Resolve all unresolved symbols in current scope.
        /// </summary>
        public void Resolve()
        {
            Resolve(ClassMap);
            Resolve(ObjectMap);
            Resolve(FunctionMap);
            Resolve(VariableMap);
            Resolve(ParamMap);
            Resolve(TypeMap);
        }

        /// <summary>
        /// Perform post-resolve actions for all symbols in current scope.
        /// </summary>
        public void PostResolve()
        {
            Resolve(ClassMap, true);
            Resolve(ObjectMap, true);
            Resolve(FunctionMap, true);
            Resolve(VariableMap, true);
            Resolve(ParamMap, true);
            Resolve(TypeMap, true);
        }

        /// <summary>
        /// Resolve all symbols in specified dictionary.
        /// </summary>
        /// <typeparam name="TEntity"> Symbol type. </typeparam>
        /// <param name="dict"> Symbol dictionary. </param>
        private void Resolve<TEntity>(Dictionary<string, TEntity> dict, bool post = false)
            where TEntity : SymbolBase
        {
            foreach (var symbol in dict.Values)
            {
                try
                {
                    if (post)
                    {
                        symbol.PostResolve();
                    }
                    else
                    {
                        symbol.Resolve();
                    }
                }
                catch (Exception e)
                {
                    Errors.Add($"Error at " +
                        $"{symbol.Context.Start.Line}:{symbol.Context.Start.Column} - {e.Message}");
                }
            }
        }
    }
}
