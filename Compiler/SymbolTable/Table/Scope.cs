using Antlr4.Runtime;
using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Symbol.Variable;
using System;
using System.Collections.Generic;
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
        public Scope EnclosingScope { get; set; }

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
        public Dictionary<string, VariableSymbolBase> VariableMap = new();

        /// <summary>
        /// Param symbol map for current scope.
        /// </summary>
        public Dictionary<string, VariableSymbolBase> ParamMap = new();

        /// <summary>
        /// Type symbol map for current scope.
        /// </summary>
        public Dictionary<string, TypeSymbol> TypeMap = new();

        public Scope(ScopeType type, Scope enclosingScope = null)
        {
            Guid = Guid.NewGuid();
            Type = type;
            EnclosingScope = enclosingScope;
        }

        /// <summary>
        /// Define new variable symbol from parse tree node context for current scope.
        /// </summary>
        /// <param name="context"> Parse tree node context. </param>
        public SymbolBase Define(ParserRuleContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            try
            {
                SymbolBase symbol = context switch
                {
                    ClassDefContext => new ClassSymbol(context as ClassDefContext, this),
                    ObjectDefContext => new ObjectSymbol(context as ObjectDefContext, this),
                    FunDefContext => new FunctionSymbol(context as FunDefContext, this),
                    ParserRuleContext c when (c is ParamContext || c is ClassParamContext) =>
                        new ParamSymbol(context, this),
                    ParserRuleContext c when (c is ValDclContext || c is VarDclContext || c is PatVarDefContext) =>
                        new VariableSymbol(context, this),
                    TypeDefContext => new TypeSymbol(context as TypeDefContext, this),
                    _ => throw new NotImplementedException(),
                };

                return Define(symbol);
            }
            catch (ArgumentException)
            {
                Console.Error.WriteLine(
                    $"Error at {context.Start.Line}:{context.Start.Column} - symbol with such name already defined.");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(
                    $"Error at {context.Start.Line}:{context.Start.Column} - {e.Message}");
            }

            return null;
        }

        /// <summary>
        /// Define new variable symbol from its instance for current scope.
        /// </summary>
        /// <param name="symbol"> Symbol instance to define. </param>
        public SymbolBase Define(SymbolBase symbol)
        {
            _ = symbol ?? throw new ArgumentNullException(nameof(symbol));

            symbol.Scope = this;

            switch (symbol)
            {
                case ClassSymbol s: ClassMap.Add(symbol.Name, s); break;
                case ObjectSymbol s: ObjectMap.Add(symbol.Name, s); break;
                case TraitSymbol s: TraitMap.Add(symbol.Name, s); break;
                case FunctionSymbol s: FunctionMap.Add(symbol.Name, s); break;
                case VariableSymbol s: VariableMap.Add(symbol.Name, s); break;
                case ParamSymbol s: ParamMap.Add(symbol.Name, s); break;
                case TypeSymbol s: TypeMap.Add(symbol.Name, s); break;
                default: throw new NotImplementedException();
            }

            return symbol;
        }

        /// <summary>
        /// Looking for symbol by name in current and all enclousing scopes.
        /// </summary>
        /// <param name="name"> Symbol name. </param>
        /// /// <param name="name"> Symbol name. </param>
        /// <returns> Symbol with corresponding name in the 
        /// nearest enclosing scope or null if not found. </returns>
        public SymbolBase GetSymbol(string name, SymbolType type)
        {
            return type switch
            {
                SymbolType.Class => ClassMap.ContainsKey(name) ? ClassMap[name] : EnclosingScope?.GetSymbol(name, type),
                SymbolType.Object => ObjectMap.ContainsKey(name) ? ObjectMap[name] : EnclosingScope?.GetSymbol(name, type),
                SymbolType.Trait => TraitMap.ContainsKey(name) ? TraitMap[name] : EnclosingScope?.GetSymbol(name, type),
                SymbolType.Function => FunctionMap.ContainsKey(name) ? FunctionMap[name] : EnclosingScope?.GetSymbol(name, type),
                SymbolType.Variable => VariableMap.ContainsKey(name) ? VariableMap[name] : EnclosingScope?.GetSymbol(name, type),
                SymbolType.Type => TypeMap.ContainsKey(name) ? TypeMap[name] : EnclosingScope?.GetSymbol(name, type),
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// Resolve all unresolved symbols current scope.
        /// </summary>
        public void Resolve()
        {
            Resolve(ClassMap);
            Resolve(ObjectMap);
            Resolve(FunctionMap);
            Resolve(VariableMap);
        }

        /// <summary>
        /// Resolve all symbols in specified dictionary.
        /// </summary>
        /// <typeparam name="TEntity"> Symbol type. </typeparam>
        /// <param name="dict"> Symbol dictionary. </param>
        private void Resolve<TEntity>(Dictionary<string, TEntity> dict) where TEntity : SymbolBase
        {
            foreach (var symbol in dict.Values)
            {
                try
                {
                    symbol.Resolve();
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Error at {symbol.Context.Start.Line}:{symbol.Context.Start.Column} - {e.Message}");
                }
            }
        }
    }
}
