lexer grammar Team1Lexer;

// El lexer divide el texto del programa en palabras y simbolos que el parser pueda entender.

// Palabras reservadas del lenguaje.
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

// Operadores para asignaciones, sumas, comparaciones y condiciones logicas.
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

// Valores basicos: nombres de variables, enteros, decimales y textos entre comillas.
ID : [a-zA-Z_][a-zA-Z0-9_]* ;
FLOAT : '-'? [0-9]+ '.' [0-9]+ ;
INT : '-'? [0-9]+ ;
STRING : '"' .*? '"' ;

// Los comentarios y espacios se ignoran para que no afecten la sintaxis.
COMMENT : '//' .*? '\n' -> skip ;
WS : [ \t\r\n]+ -> skip ;
