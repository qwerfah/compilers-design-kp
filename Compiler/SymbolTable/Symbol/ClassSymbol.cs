﻿using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SymbolTable.Symbol
{
    public class ClassSymbol : SymbolBase
    {
        public ClassSymbol(ParserRuleContext context, Scope scope) : base(context, scope)
        {

        }
    }
}
