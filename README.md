# Team1
Créditos a Iván por elaborar la base y esqueleto del Lexer.

Lenguaje sencillo del Equipo 1 para probar reglas con variables, ciclos, condiciones, entrada de datos y salidas por consola.

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

El script revisa si tienes `dotnet` y `java`, restaura paquetes de NuGet y muestra el comando para analizar el programa.

## Ejecutar el programa

```bash
dotnet run --project Team1App/Team1App.csproj -- codigo.team1
```

El archivo [codigo.team1](codigo.team1) contiene el programa que se va a analizar.

Al ejecutar el proyecto se muestran las tres fases principales de analisis:

- `FASE 1: ANALISIS LEXICO`: muestra los tokens encontrados, con linea, columna, tipo de token y lexema.
- `FASE 2: ANALISIS SINTACTICO`: muestra la gramatica propuesta, la regla inicial aceptada por ANTLR y las instrucciones reconocidas.
- `FASE 3: ANALISIS SEMANTICO`: construye tabla de simbolos y valida existencia de variables y compatibilidad de tipos.

El proyecto no ejecuta la logica del programa al final; solo muestra los analisis solicitados.

Ejemplo de salida:

```text
Fase 1 - Analisis lexico
Resultado del analisis lexico
[L:1, C:1] Token: STAR       | Valor: STAR
[L:4, C:1] Token: VAR        | Valor: VAR
[L:4, C:5] Token: ID         | Valor: aceptados

Fase 2 - Analisis sintactico
Gramatica
S = start_rule
1. start_rule -> START instrucciones END

Resultado ANTLR
Regla inicial: start_rule
Estado: cadena aceptada

Instrucciones reconocidas
- var_decl: VAR aceptados, rechazado_edad, rechazado_prom, rechazado_ingreso = 0
- ciclo_loop: LOOP i IN range (5)
- salida: OUT "Total: " + aceptados

Fase 3 - Analisis semantico
Tabla de simbolos
Nombre                 | Tipo   | Origen
aceptados              | number | VAR
edad                   | number | INPUT

Validaciones semanticas
aceptados = aceptados + 1 -> destino: OK, tipos: OK
edad <= 18 -> tipos compatibles: OK

Semantica: correcta.

Resultado final: codigo valido.
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

El lexer esta definido en [Team1Lexer.g4](Team1Lexer.g4). Convierte el codigo fuente en tokens como:

- Palabras reservadas: `STAR`, `START`, `END`, `VAR`, `LOOP`, `CHECK`, `IF`, `ELSEIF`, `ELSE`, `OUT`, `INPUT`.
- Identificadores: nombres de variables como `edad`, `promedio` o `aceptados`.
- Numeros: `INT` y `FLOAT`, incluyendo negativos.
- Cadenas: textos entre comillas, como `"Edad: "`.
- Operadores: `=`, `+`, `<=`, `>=`, `&&`, `||`, `|`, `!`.

### Analisis sintactico y gramatica

La gramatica esta definida en [Team1Parser.g4](Team1Parser.g4). La regla principal es:

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

El analisis semantico esta implementado en [Team1SemanticAnalyzer.cs](Team1App/Team1SemanticAnalyzer.cs). Esta fase revisa significado, no solo forma.

Paso 1: tabla de simbolos.

```text
Nombre            | Tipo   | Origen
aceptados         | number | VAR
edad              | number | INPUT
```

Paso 2: validaciones semanticas.

- Si una variable se declara con `VAR`, se agrega a la tabla de simbolos.
- Si una variable viene de `INPUT`, se registra como dato numerico de entrada.
- Si una variable se usa en suma, salida o condicion, debe existir en la tabla.
- Las sumas y comparaciones deben usar valores `number`.
- Si una variable no existe, se reporta error semantico.

Ejemplo de error semantico:

```text
STAR
a = 5
END
```

Salida esperada:

```text
a = 5 -> existe: ERROR, tipo: ERROR
ERROR: a no fue declarado.
Semantica: con errores.
Resultado final: codigo no valido.
```

## Regenerar ANTLR

Si modificas [Team1Lexer.g4](Team1Lexer.g4) o [Team1Parser.g4](Team1Parser.g4), regenera los archivos C# con:

```bash
java -jar antlr-4.7.2-complete.jar -Dlanguage=CSharp -o Team1App Team1Lexer.g4 Team1Parser.g4
```

Despues ejecuta de nuevo:

```bash
dotnet run --project Team1App/Team1App.csproj -- codigo.team1
```

## Problemas comunes

Si aparece:

```text
zsh: command not found: dotnet
```

Falta instalar el SDK de .NET o agregarlo al PATH.

Si aparece un error de sintaxis, revisa que tu archivo empiece con `STAR` o `START` y termine con `END`.
