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

El archivo [codigo.becas](codigo.becas) contiene el programa que se va a interpretar.

## Ejemplo de codigo

```text
STAR
VAR aceptados, rechazado_edad, rechazado_prom, rechazado_ingreso = 0
VAR promedio_minimo = 90.5

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
# BECAS_LEXER_E1
