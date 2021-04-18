whiteSpace       ::=  ‘\u0020’ | ‘\u0009’ | ‘\u000D’ | ‘\u000A’
upper            ::=  ‘A’ | … | ‘Z’ | ‘$’ | ‘_’  // and Unicode category Lu
lower            ::=  ‘a’ | … | ‘z’ // and Unicode category Ll
letter           ::=  upper | lower // and Unicode categories Lo, Lt, Nl
digit            ::=  ‘0’ | … | ‘9’
paren            ::=  ‘(’ | ‘)’ | ‘[’ | ‘]’ | ‘{’ | ‘}’
delim            ::=  ‘`’ | ‘'’ | ‘"’ | ‘.’ | ‘;’ | ‘,’
opchar           ::= // printableChar not matched by (whiteSpace | upper | lower |
                     // letter | digit | paren | delim | opchar | Unicode_Sm | Unicode_So)
printableChar    ::= // all characters in [\u0020, \u007F] inclusive
charEscapeSeq    ::= ‘\‘ (‘b‘ | ‘t‘ | ‘n‘ | ‘f‘ | ‘r‘ | ‘"‘ | ‘'‘ | ‘\‘)

op               ::=  opchar {opchar}
varid            ::=  lower idrest
plainid          ::=  upper idrest
                 |  varid
                 |  op
id               ::=  plainid
                 |  ‘`’ stringLiteral ‘`’
idrest           ::=  {letter | digit} [‘_’ op]

integerLiteral   ::=  (decimalNumeral | hexNumeral) [‘L’ | ‘l’]
decimalNumeral   ::=  ‘0’ | nonZeroDigit {digit}
hexNumeral       ::=  ‘0’ (‘x’ | ‘X’) hexDigit {hexDigit}
digit            ::=  ‘0’ | nonZeroDigit
nonZeroDigit     ::=  ‘1’ | … | ‘9’

floatingPointLiteral
                 ::=  digit {digit} ‘.’ digit {digit} [exponentPart] [floatType]
                 |  ‘.’ digit {digit} [exponentPart] [floatType]
                 |  digit {digit} exponentPart [floatType]
                 |  digit {digit} [exponentPart] floatType
exponentPart     ::=  (‘E’ | ‘e’) [‘+’ | ‘-’] digit {digit}
floatType        ::=  ‘F’ | ‘f’ | ‘D’ | ‘d’

booleanLiteral   ::=  ‘true’ | ‘false’

characterLiteral ::=  ‘'’ (printableChar | charEscapeSeq) ‘'’

stringLiteral    ::=  ‘"’ {stringElement} ‘"’
                 |  ‘"""’ multiLineChars ‘"""’
stringElement    ::=  (printableChar except ‘"’)
                 |  charEscapeSeq
multiLineChars   ::=  {[‘"’] [‘"’] charNoDoubleQuote} {‘"’}

symbolLiteral    ::=  ‘'’ plainid

comment          ::=  ‘/*’ “any sequence of characters; nested comments are allowed” ‘*/’
                 |  ‘//’ “any sequence of characters up to end of line”

nl               ::=  “newlinecharacter”
semi             ::=  ‘;’ |  nl {nl}