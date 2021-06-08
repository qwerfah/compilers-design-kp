using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Compiler.SymbolTable.Symbol;
using Compiler.SymbolTable.Symbol.Class;
using Compiler.SymbolTable.Symbol.Variable;
using static Parser.Antlr.Grammar.ScalaParser;

namespace Compiler.SymbolTable
{
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
        public Dictionary<string, SymbolBase> ClassMap = new();

        /// <summary>
        /// Object symbol map for current scope.
        /// </summary>
        public Dictionary<string, SymbolBase> ObjectMap = new();

        /// <summary>
        /// Trait symbol map for current scope.
        /// </summary>
        public Dictionary<string, SymbolBase> TraitMap = new();

        /// <summary>
        /// Function symbol map for current scope.
        /// </summary>
        public Dictionary<string, SymbolBase> FunctionMap = new();

        /// <summary>
        /// Varialbe symbol map for current scope.
        /// </summary>
        public Dictionary<string, SymbolBase> VariableMap = new();

        /// <summary>
        /// Type symbol map for current scope.
        /// </summary>
        public Dictionary<string, SymbolBase> TypeMap = new();

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
        public void Define(ParserRuleContext context)
        {
            _ = context ?? throw new ArgumentNullException(nameof(context));

            SymbolBase symbol = context switch
            {
                ClassDefContext => new ClassSymbol(context as ClassDefContext, this),
                ObjectDefContext => new ObjectSymbol(context as ObjectDefContext, this),
                FunDefContext => new FunctionSymbol(context as FunDefContext, this),
                ParamContext => new FunctionParamSymbol(context as ParamContext, this),
                ClassParamContext => new ClassParamSymbol(context as ClassParamContext, this),
                ParserRuleContext c when (c is ValDclContext || c is VarDclContext || c is PatVarDefContext) => 
                    new VariableSymbol(context, this),
                TypeDefContext => new TypeSymbol(context as TypeDefContext, this),
                _ => throw new NotImplementedException(),
            };

            Define(symbol);
        }

        /// <summary>
        /// Define new variable symbol from its instance for current scope.
        /// </summary>
        /// <param name="symbol"> Symbol instance to define. </param>
        public void Define(SymbolBase symbol)
        {
            _ = symbol ?? throw new ArgumentNullException(nameof(symbol));
           
            symbol.Scope = this;

            switch (symbol)
            {
                case ClassSymbol: ClassMap.Add(symbol.Name, symbol); break;
                case ObjectSymbol: ObjectMap.Add(symbol.Name, symbol); break;
                case FunctionSymbol: FunctionMap.Add(symbol.Name, symbol); break;
                case VariableSymbolBase: VariableMap.Add(symbol.Name, symbol); break;
                case TypeSymbol: TypeMap.Add(symbol.Name, symbol); break;
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Looking for symbol by name in current and all enclousing scopes.
        /// </summary>
        /// <param name="name"> Symbol name. </param>
        /// /// <param name="name"> Symbol name. </param>
        /// <returns> Symbol with corresponding name in the nearest enclosing scope. </returns>
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
            foreach (var symbol in ClassMap.Values)
            {
                symbol.Resolve();
            }

            foreach (var symbol in ObjectMap.Values)
            {
                symbol.Resolve();
            }

            foreach (var symbol in VariableMap.Values)

            {
                symbol.Resolve();
            }
        }
    }
}
