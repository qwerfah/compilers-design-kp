﻿using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SymbolTable.Symbol
{
    public class FunctionSymbol : SymbolBase
    {
        public FunctionSymbol(ParserRuleContext context, Scope scope) : base(context, scope)
        {

        }
    }
}
