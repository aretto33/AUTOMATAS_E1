# Proyecto Becas
Créditos a Iván por elaborar la base y esqueleto del Lexer.

Lenguaje sencillo para probar reglas de becas con variables, ciclos, condiciones, entrada de datos y salidas por consola.

## Requisitos

- .NET SDK 10.0 o compatible con el `TargetFramework` del proyecto.
- Java, solo si vas a regenerar los archivos de ANTLR desde las gramaticas `.g4`.

Para revisar si ya los tienes instalados:

```bash
dotnet --version
java -version
```

Si `dotnet` no existe, instala el SDK de .NET desde:

https://dotnet.microsoft.com/download

En macOS tambien puedes instalarlo con Homebrew:

```bash
brew install dotnet
```

## Instalacion rapida

Desde la carpeta del proyecto:

```bash
chmod +x instalar.sh
./instalar.sh
```

El script revisa si tienes `dotnet` y `java`, restaura paquetes de NuGet y muestra el comando para ejecutar el programa.

## Ejecutar el programa

```bash
dotnet run --project BecasApp/BecasApp.csproj -- codigo.becas
```

El archivo [codigo.becas](codigo.becas) contiene el programa que se va a analizar e interpretar.

Al ejecutar el programa se muestran cuatro secciones:

- `ANALISIS LEXICO`: muestra los tokens encontrados, con linea, columna, tipo de token y texto.
- `ANALISIS SINTACTICO / GRAMATICA`: muestra si el codigo cumple la gramatica, las producciones principales y el arbol sintactico.
- `ANALISIS SEMANTICO`: revisa declaraciones, variables, entradas, salidas, sumas y comparaciones.
- `EJECUCION`: ejecuta el programa si no hay errores sintacticos ni semanticos.

Ejemplo de salida:

```text
========== ANALISIS LEXICO ==========
Linea  1, Columna  0 | START        | STAR
Linea  2, Columna  0 | VAR          | VAR
Linea  2, Columna  4 | ID           | aceptados

========== ANALISIS SINTACTICO / GRAMATICA ==========
>>> [EXITO]: El codigo cumple con la gramatica.
Regla inicial: start_rule -> START instrucciones END

========== ANALISIS SEMANTICO ==========
Declaracion valida: aceptados, rechazado_edad, rechazado_prom, rechazado_ingreso = 0
Resultado: analisis semantico correcto.

========== EJECUCION ==========
>>> Ejecutando programa...
```

## Ejemplo de codigo

```text
STAR
VAR aceptados, rechazado_edad, rechazado_prom, rechazado_ingreso = 0
VAR promedio_minimo = 70.5

LOOP i IN range (5):
    OUT "Bienvenido alumno No." + i
    INPUT "Edad: " edad
    INPUT "Promedio: " promedio
    INPUT "Ingreso: " ingreso
    CHECK:
        IF (!(edad <= 18) && !(edad >= 25) && promedio >= promedio_minimo && !(ingreso >= 5000)) SO:
            aceptados = aceptados + 1
            OUT "Beca aceptada con promedio " + promedio
        ELSEIF (edad <= 18 || edad >= 25) SO:
            rechazado_edad = rechazado_edad + 1
            OUT "Rechazado: EDAD"
        ELSEIF (ingreso >= 5000) SO:
            rechazado_ingreso = rechazado_ingreso + 1
            OUT "Rechazado: INGRESO"
        ELSE:
            rechazado_prom = rechazado_prom + 1
            OUT "Rechazado: PROMEDIO"
    END_CHECK
END_LOOP

OUT "Total: " + aceptados
END
```

## Instrucciones soportadas

- `VAR nombre = 0`: declara variables.
- `VAR a, b, c = 0`: declara varias variables con el mismo valor.
- `nombre = 95.5`: asigna un numero entero o decimal.
- `nombre = -5`: asigna un numero entero negativo.
- `nombre = -95.5`: asigna un numero decimal negativo.
- `nombre = nombre + 1`: suma valores.
- `INPUT "Mensaje: " variable`: pide un numero al usuario.
- `OUT "Texto"`: imprime texto.
- `OUT "Texto" + variable`: imprime texto junto con una variable.
- `LOOP i IN range (5): ... END_LOOP`: repite instrucciones.
- `CHECK: IF (...) SO: ... ELSEIF (...) SO: ... ELSE: ... END_CHECK`: condicionales.

## Operadores soportados

Relacionales:

```text
<=
>=
```

Logicos:

```text
&&
||
|
!
```

Tambien se pueden agrupar condiciones con parentesis:

```text
IF (!(edad <= 18) && (promedio >= 90.5 || ingreso <= 3000)) SO:
```

## Numeros soportados

El lenguaje acepta enteros y decimales positivos o negativos:

```text
VAR edad = 20
VAR deuda = -5
VAR promedio = 95.5
VAR ajuste = -10.5
```

Estos valores se reconocen en el analisis lexico como `INT` o `FLOAT`.

## Analisis que realiza el proyecto

### Analisis lexico

El lexer esta definido en [BecasLexer.g4](BecasLexer.g4). Convierte el codigo fuente en tokens como:

- Palabras reservadas: `STAR`, `START`, `END`, `VAR`, `LOOP`, `CHECK`, `IF`, `ELSEIF`, `ELSE`, `OUT`, `INPUT`.
- Identificadores: nombres de variables como `edad`, `promedio` o `aceptados`.
- Numeros: `INT` y `FLOAT`, incluyendo negativos.
- Cadenas: textos entre comillas, como `"Edad: "`.
- Operadores: `=`, `+`, `<=`, `>=`, `&&`, `||`, `|`, `!`.

### Analisis sintactico y gramatica

La gramatica esta definida en [BecasParser.g4](BecasParser.g4). La regla principal es:

```antlr
start_rule : START instrucciones END ;
```

Esto significa que un programa valido debe iniciar con `STAR` o `START`, contener instrucciones y terminar con `END`.

Producciones principales:

```antlr
var_decl      : VAR ID (COMMA ID)* OP_ASIG numero ;
input_usuario : INPUT STRING ID ;
ciclo_loop    : LOOP ID IN RANGE PARENT_I INT PARENT_D COLON instrucciones END_LOOP ;
check_block   : CHECK COLON IF PARENT_I condicion PARENT_D SO COLON instrucciones
                (elseif_bloque)* (else_bloque)? END_CHECK ;
salida        : OUT valor_salida (OP_ARIT valor_salida)? ;
```

### Analisis semantico

El analisis semantico esta implementado en [BecasSemanticAnalyzer.cs](BecasApp/BecasSemanticAnalyzer.cs). Antes de ejecutar, revisa que el programa tenga sentido:

- Las variables declaradas se registran en una tabla de simbolos.
- Las variables usadas en salidas, sumas y comparaciones deben tener un valor previo.
- Las entradas `INPUT` guardan valores numericos.
- Las sumas trabajan con expresiones numericas.
- Las condiciones usan comparaciones validas con `<=` o `>=`.
- Si hay errores semanticos, el programa los muestra y no ejecuta el codigo.

## Regenerar ANTLR

Si modificas [BecasLexer.g4](BecasLexer.g4) o [BecasParser.g4](BecasParser.g4), regenera los archivos C# con:

```bash
java -jar antlr-4.7.2-complete.jar -Dlanguage=CSharp -o BecasApp BecasLexer.g4 BecasParser.g4
```

Despues ejecuta de nuevo:

```bash
dotnet run --project BecasApp/BecasApp.csproj -- codigo.becas
```

## Problemas comunes

Si aparece:

```text
zsh: command not found: dotnet
```

Falta instalar el SDK de .NET o agregarlo al PATH.

Si aparece un error de sintaxis, revisa que tu archivo empiece con `STAR` o `START` y termine con `END`.
