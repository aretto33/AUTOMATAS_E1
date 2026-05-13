parser grammar Team1Parser;
options { tokenVocab=Team1Lexer; }

// Estructura general del programa.
start_rule : START instrucciones END ;

// Lista de instrucciones.
instrucciones : instruccion* ;

// Instrucciones permitidas.
instruccion : var_decl
| input_datos
| input_usuario
| ciclo_loop
| check_block
| salida
| asignacion_suma ;

// Declaracion de variables.
var_decl : VAR ID (COMMA ID)* OP_ASIG numero ;

// Asignacion directa.
input_datos : ID OP_ASIG numero ;

// Entrada por consola.
input_usuario : INPUT STRING ID ;

// Suma simple.
asignacion_suma : ID OP_ASIG exp OP_ARIT exp ;

// Ciclo con range.
ciclo_loop : LOOP ID IN RANGE PARENT_I INT PARENT_D COLON instrucciones
END_LOOP ;

// Bloque condicional.
check_block : CHECK COLON IF PARENT_I condicion PARENT_D SO COLON
instrucciones

(elseif_bloque)* (else_bloque)?
END_CHECK ;

elseif_bloque : ELSEIF PARENT_I condicion PARENT_D SO COLON instrucciones ;
else_bloque : ELSE COLON instrucciones ;

// Condiciones con prioridad logica.
condicion : or_cond ;
or_cond : and_cond (OP_OR and_cond)* ;
and_cond : not_cond (OP_AND not_cond)* ;
not_cond : OP_NOT not_cond
| PARENT_I condicion PARENT_D
| comparacion ;
comparacion : exp OP_REL exp ;

// Expresiones numericas.
exp : ID | INT | FLOAT ;
numero : INT | FLOAT ;

// Valores de salida.
valor_salida : STRING | ID | INT | FLOAT ;
salida : OUT valor_salida (OP_ARIT valor_salida)? ;
