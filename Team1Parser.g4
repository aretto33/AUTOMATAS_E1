parser grammar Team1Parser;
options { tokenVocab=Team1Lexer; }

// Programa completo: debe iniciar con STAR/START y terminar con END.
start_rule : START instrucciones END ;

// Una lista de instrucciones puede tener cero o mas lineas validas.
instrucciones : instruccion* ;

// Tipos de instrucciones que puede ejecutar el lenguaje.
instruccion : var_decl
| input_datos
| input_usuario
| ciclo_loop
| check_block
| salida
| asignacion_suma ;

// Declaracion: VAR a, b = 0 o VAR promedio = 90.5.
var_decl : VAR ID (COMMA ID)* OP_ASIG numero ;

// Asignacion directa: edad = 20 o promedio = 95.5.
input_datos : ID OP_ASIG numero ;

// Entrada por consola: INPUT "Edad: " edad.
input_usuario : INPUT STRING ID ;

// Suma simple: aceptados = aceptados + 1.
asignacion_suma : ID OP_ASIG exp OP_ARIT exp ;

// Ciclo: repite las instrucciones internas la cantidad indicada en range(...).
ciclo_loop : LOOP ID IN RANGE PARENT_I INT PARENT_D COLON instrucciones
END_LOOP ;

// Bloque condicional con IF, varios ELSEIF opcionales y un ELSE opcional.
check_block : CHECK COLON IF PARENT_I condicion PARENT_D SO COLON
instrucciones

(elseif_bloque)* (else_bloque)?
END_CHECK ;

elseif_bloque : ELSEIF PARENT_I condicion PARENT_D SO COLON instrucciones ;
else_bloque : ELSE COLON instrucciones ;

// Condiciones con prioridad: ! primero, luego &&, luego || o |.
condicion : or_cond ;
or_cond : and_cond (OP_OR and_cond)* ;
and_cond : not_cond (OP_AND not_cond)* ;
not_cond : OP_NOT not_cond
| PARENT_I condicion PARENT_D
| comparacion ;
comparacion : exp OP_REL exp ;

// Expresiones numericas usadas en comparaciones y sumas.
exp : ID | INT | FLOAT ;
numero : INT | FLOAT ;

// Valores que se pueden imprimir con OUT.
valor_salida : STRING | ID | INT | FLOAT ;
salida : OUT valor_salida (OP_ARIT valor_salida)? ;
