namespace Compiler.SymbolTable.Symbol.Variable
{
    /// <summary>
    /// Class member access modifier.
    /// </summary>
    public enum AccessModifier
    {
        /// <summary>
        /// No access modifier (not a class field).
        /// </summary>
        None,
        /// <summary>
        /// Public class member.
        /// </summary>
        Public,
        /// <summary>
        /// Private class member.
        /// </summary>
        Private,
        /// <summary>
        /// Protected class member.
        /// </summary>
        Protected
    }
}
