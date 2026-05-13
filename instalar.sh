#!/usr/bin/env bash
set -e

echo "== Team1 =="
echo "Revisando dependencias..."
echo

missing_dotnet=0
missing_java=0

if command -v dotnet >/dev/null 2>&1; then
  echo "OK: dotnet encontrado: $(dotnet --version)"
else
  echo "FALTA: dotnet SDK"
  missing_dotnet=1
fi

if command -v java >/dev/null 2>&1; then
  echo "OK: java encontrado"
else
  echo "AVISO: java no encontrado"
  echo "Java solo se necesita si vas a regenerar los archivos de ANTLR."
  missing_java=1
fi

echo

if [ "$missing_dotnet" -eq 1 ]; then
  echo "Instala .NET SDK desde:"
  echo "https://dotnet.microsoft.com/download"
  echo
  echo "En macOS con Homebrew:"
  echo "brew install dotnet"
  echo
  echo "Despues vuelve a correr:"
  echo "./instalar.sh"
  exit 1
fi

echo "Restaurando paquetes del proyecto..."
dotnet restore "Team1App/Team1App.csproj"

echo
echo "Listo."
echo "Para ejecutar el programa:"
echo 'dotnet run --project Team1App/Team1App.csproj -- codigo.team1'

if [ "$missing_java" -eq 1 ]; then
  echo
  echo "Nota: instala Java si necesitas modificar las gramaticas .g4 y regenerar ANTLR."
fi
