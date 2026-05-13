lexer grammar Team1Lexer;

// Tokens que reconoce nuestro lenguaje.

// Palabras reservadas.
START : 'START' | 'STAR' ;
END : 'END' ;
VAR : 'VAR' ;
LOOP : 'LOOP' ;
IN : 'IN' ;
RANGE : 'range' ;
CHECK : 'CHECK' ;
IF : 'IF' ;
SO : 'SO' ;
ELSEIF : 'ELSEIF' ;
ELSE : 'ELSE' ;
END_CHECK : 'END_CHECK' ;
END_LOOP : 'END_LOOP' ;
OUT : 'OUT' ;
INPUT : 'INPUT' ;

// Operadores.
OP_ASIG : '=' ;
OP_ARIT : '+' ;
OP_REL : '<=' | '>=' ;
OP_OR : '||' | '|' ;
OP_AND : '&&' ;
OP_NOT : '!' ;
PARENT_I : '(' ;
PARENT_D : ')' ;
COLON : ':' ;
COMMA : ',' ;

// Valores basicos.
ID : [a-zA-Z_][a-zA-Z0-9_]* ;
FLOAT : '-'? [0-9]+ '.' [0-9]+ ;
INT : '-'? [0-9]+ ;
STRING : '"' .*? '"' ;

// Se omiten comentarios y espacios.
COMMENT : '//' .*? '\n' -> skip ;
WS : [ \t\r\n]+ -> skip ;
