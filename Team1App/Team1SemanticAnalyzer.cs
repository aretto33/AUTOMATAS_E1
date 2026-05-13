using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Team1App
{
    class Team1SemanticResult
    {
        public Team1SemanticResult(bool isValid, string report)
        {
            IsValid = isValid;
            Report = report;
        }

        public bool IsValid { get; }
        public string Report { get; }
    }

    class Team1SemanticAnalyzer
    {
        private readonly Dictionary<string, SymbolInfo> symbols = new Dictionary<string, SymbolInfo>();
        private readonly List<string> validations = new List<string>();
        private readonly List<string> errors = new List<string>();

        public Team1SemanticResult Analyze(Team1Parser.Start_ruleContext program)
        {
            symbols.Clear();
            validations.Clear();
            errors.Clear();

            AnalyzeInstructions(program.instrucciones());

            var report = new StringBuilder();
            report.AppendLine("Tabla de simbolos");
            report.AppendLine("Nombre                 | Tipo   | Origen");
            report.AppendLine("-----------------------------------------------");

            foreach (var symbol in symbols.Values)
            {
                report.AppendLine($"{symbol.Name,-22} | {symbol.Type,-6} | {symbol.Origin}");
            }

            report.AppendLine();
            report.AppendLine("Validaciones semanticas");
            foreach (string validation in validations)
            {
                report.AppendLine(validation);
            }

            if (errors.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("Errores semanticos");
                foreach (string error in errors)
                {
                    report.AppendLine(error);
                }
            }

            report.AppendLine();
            report.AppendLine(errors.Count == 0 ? "Semantica: correcta." : "Semantica: con errores.");

            return new Team1SemanticResult(errors.Count == 0, report.ToString());
        }

        private void AnalyzeInstructions(Team1Parser.InstruccionesContext instructions)
        {
            foreach (var instruction in instructions.instruccion())
            {
                AnalyzeInstruction(instruction);
            }
        }

        private void AnalyzeInstruction(Team1Parser.InstruccionContext instruction)
        {
            if (instruction.var_decl() != null)
            {
                AnalyzeVarDeclaration(instruction.var_decl());
            }
            else if (instruction.input_datos() != null)
            {
                AnalyzeAssignment(instruction.input_datos());
            }
            else if (instruction.input_usuario() != null)
            {
                AnalyzeUserInput(instruction.input_usuario());
            }
            else if (instruction.asignacion_suma() != null)
            {
                AnalyzeSumAssignment(instruction.asignacion_suma());
            }
            else if (instruction.ciclo_loop() != null)
            {
                AnalyzeLoop(instruction.ciclo_loop());
            }
            else if (instruction.check_block() != null)
            {
                AnalyzeCheck(instruction.check_block());
            }
            else if (instruction.salida() != null)
            {
                AnalyzeOutput(instruction.salida());
            }
        }

        private void AnalyzeVarDeclaration(Team1Parser.Var_declContext declaration)
        {
            string value = declaration.numero().GetText();

            foreach (var id in declaration.ID())
            {
                DeclareSymbol(id.GetText(), "number", "VAR");
                validations.Add($"OK: {id.GetText()} declarado como number con valor {value}.");
            }
        }

        private void AnalyzeAssignment(Team1Parser.Input_datosContext assignment)
        {
            string name = assignment.ID().GetText();
            bool exists = ValidateVariableExists(name);
            bool typeOk = ValidateType(name, "number");
            validations.Add($"{name} = {assignment.numero().GetText()} -> existe: {Result(exists)}, tipo: {Result(typeOk)}");
        }

        private void AnalyzeUserInput(Team1Parser.Input_usuarioContext input)
        {
            string name = input.ID().GetText();

            if (!symbols.ContainsKey(name))
            {
                DeclareSymbol(name, "number", "INPUT");
            }

            validations.Add($"OK: INPUT guarda un number en {name}.");
        }

        private void AnalyzeSumAssignment(Team1Parser.Asignacion_sumaContext assignment)
        {
            string target = assignment.ID().GetText();

            bool targetExists = ValidateVariableExists(target);
            bool leftOk = ValidateExpression(assignment.exp(0));
            bool rightOk = ValidateExpression(assignment.exp(1));

            validations.Add($"{target} = {assignment.exp(0).GetText()} + {assignment.exp(1).GetText()} -> destino: {Result(targetExists)}, tipos: {Result(leftOk && rightOk)}");
        }

        private void AnalyzeLoop(Team1Parser.Ciclo_loopContext loop)
        {
            string iterator = loop.ID().GetText();
            int repetitions = int.Parse(loop.INT().GetText(), CultureInfo.InvariantCulture);

            if (!symbols.ContainsKey(iterator))
            {
                DeclareSymbol(iterator, "number", "LOOP");
            }

            validations.Add($"OK: {iterator} declarado como iterador number en range({repetitions}).");
            AnalyzeInstructions(loop.instrucciones());
        }

        private void AnalyzeCheck(Team1Parser.Check_blockContext check)
        {
            ValidateCondition(check.condicion());
            validations.Add("OK: IF usa condicion booleana.");
            AnalyzeInstructions(check.instrucciones());

            foreach (var elseifBlock in check.elseif_bloque())
            {
                ValidateCondition(elseifBlock.condicion());
                validations.Add("OK: ELSEIF usa condicion booleana.");
                AnalyzeInstructions(elseifBlock.instrucciones());
            }

            if (check.else_bloque() != null)
            {
                validations.Add("OK: ELSE no requiere condicion.");
                AnalyzeInstructions(check.else_bloque().instrucciones());
            }
        }

        private void AnalyzeOutput(Team1Parser.SalidaContext output)
        {
            bool valid = true;

            foreach (var value in output.valor_salida())
            {
                if (value.ID() != null)
                {
                    valid = ValidateVariableExists(value.ID().GetText()) && valid;
                }
            }

            validations.Add($"{output.GetText()} -> variables declaradas: {Result(valid)}");
        }

        private void ValidateCondition(Team1Parser.CondicionContext condition)
        {
            foreach (var comparison in FindComparisons(condition))
            {
                bool leftOk = ValidateExpression(comparison.exp(0));
                bool rightOk = ValidateExpression(comparison.exp(1));
                validations.Add($"{comparison.exp(0).GetText()} {comparison.OP_REL().GetText()} {comparison.exp(1).GetText()} -> tipos compatibles: {Result(leftOk && rightOk)}");
            }
        }

        private IEnumerable<Team1Parser.ComparacionContext> FindComparisons(Team1Parser.CondicionContext condition)
        {
            return condition.or_cond()
                .and_cond()
                .SelectMany(andCondition => andCondition.not_cond())
                .SelectMany(FindComparisons);
        }

        private IEnumerable<Team1Parser.ComparacionContext> FindComparisons(Team1Parser.Not_condContext condition)
        {
            if (condition.comparacion() != null)
            {
                yield return condition.comparacion();
            }
            else if (condition.condicion() != null)
            {
                foreach (var comparison in FindComparisons(condition.condicion()))
                {
                    yield return comparison;
                }
            }
            else if (condition.not_cond() != null)
            {
                foreach (var comparison in FindComparisons(condition.not_cond()))
                {
                    yield return comparison;
                }
            }
        }

        private bool ValidateExpression(Team1Parser.ExpContext expression)
        {
            if (expression.ID() != null)
            {
                string name = expression.ID().GetText();
                return ValidateVariableExists(name) && ValidateType(name, "number");
            }

            return true;
        }

        private bool ValidateVariableExists(string name)
        {
            if (symbols.ContainsKey(name))
            {
                return true;
            }

            errors.Add($"ERROR: {name} no fue declarado.");
            return false;
        }

        private bool ValidateType(string name, string expectedType)
        {
            if (!symbols.TryGetValue(name, out SymbolInfo? symbol))
            {
                return false;
            }

            if (symbol.Type == expectedType)
            {
                return true;
            }

            errors.Add($"ERROR: tipo incorrecto en {name}. Esperado {symbol.Type}, recibido {expectedType}.");
            return false;
        }

        private void DeclareSymbol(string name, string type, string origin)
        {
            if (symbols.ContainsKey(name))
            {
                errors.Add($"ERROR: {name} ya fue declarado.");
                return;
            }

            symbols[name] = new SymbolInfo(name, type, origin);
        }

        private static string Result(bool value)
        {
            return value ? "OK" : "ERROR";
        }

        private class SymbolInfo
        {
            public SymbolInfo(string name, string type, string origin)
            {
                Name = name;
                Type = type;
                Origin = origin;
            }

            public string Name { get; }
            public string Type { get; }
            public string Origin { get; }
        }
    }
}
