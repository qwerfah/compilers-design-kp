using Compiler.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SymbolTable.Table
{
    public class SymbolTable
    {
        /// <summary>
        /// Symbol table unique identifier.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// Scope stack using in tree lookup.
        /// </summary>
        private Stack<Scope> _scopeStack;

        /// <summary>
        /// List of all scopes in program extracting during tree lookup.
        /// </summary>
        public List<Scope> Scopes { get; }

        public SymbolTable()
        {
            Guid = Guid.NewGuid();
            _scopeStack = new();
            Scopes = new();

            Scope global = new Scope(ScopeType.Global, null);
            _scopeStack.Push(global);
            Scopes.Add(global);
        }

        /// <summary>
        /// Create new scope and push it in scope stack and add it in list of all program scopes.
        /// </summary>
        /// <returns> Newly created scope. </returns>
        public Scope PushScope()
        {
            Scope enclosingScope = _scopeStack.Peek();
            Scope scope = new Scope(ScopeType.Local, enclosingScope);

            _scopeStack.Push(scope);
            Scopes.Add(scope);

            return scope;
        }

        /// <summary>
        /// Push existing scope to scope stack and add it in list of all program scopes.
        /// Top scope in stack will be used as enclosing for newly added scope. 
        /// </summary>
        /// <param name="scope"> Scope instance. </param>
        public void PushScope(Scope scope)
        {
            if (scope is null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            scope.EnclosingScope = _scopeStack.Peek();

            _scopeStack.Push(scope);
            Scopes.Add(scope);
        }

        /// <summary>
        /// Extract top scope from stack.
        /// </summary>
        public void PopScope()
        {
            _scopeStack.Pop();
        }

        /// <summary>
        /// Get current top element of scope stack.
        /// </summary>
        /// <returns> Top scope in scope stack. </returns>
        public Scope GetCurrentScope()
        {
            if (_scopeStack.Count > 0)
            {
                return _scopeStack.Peek();
            }

            throw new OutOfScopeException("Lookup out of any scope.");
        }

        /// <summary>
        /// Get scope in stack by its unique identifier.
        /// </summary>
        /// <param name="guid"> Scope identifier. </param>
        /// <returns> Scope instance with specified identifier. </returns>
        public Scope GetScope(Guid guid)
        {
            return _scopeStack.SingleOrDefault(s => s.Guid == guid);
        }

        /// <summary>
        /// Resolve all unresolved symbols in any symbol definition for all scopes.
        /// </summary>
        public void Resolve()
        {
            foreach (var scope in Scopes)
            {
                scope.Resolve();
            }
        }
    }
}
