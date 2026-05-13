using System;
using System.Collections.Generic;
using System.Globalization;

namespace Team1App
{
    class Team1Interpreter
    {
        // Guarda el valor actual de cada variable del programa .team1.
        private readonly Dictionary<string, double> variables = new Dictionary<string, double>();

        public void Execute(Team1Parser.Start_ruleContext program)
        {
            ExecuteInstructions(program.instrucciones());
        }

        // Recorre las instrucciones en el mismo orden en que aparecen en el archivo.
        private void ExecuteInstructions(Team1Parser.InstruccionesContext instructions)
        {
            foreach (var instruction in instructions.instruccion())
            {
                ExecuteInstruction(instruction);
            }
        }

        private void ExecuteInstruction(Team1Parser.InstruccionContext instruction)
        {
            // Cada instruccion viene del parser; aqui se decide que metodo la ejecuta.
            if (instruction.var_decl() != null)
            {
                ExecuteVarDeclaration(instruction.var_decl());
            }
            else if (instruction.input_datos() != null)
            {
                ExecuteAssignment(instruction.input_datos());
            }
            else if (instruction.input_usuario() != null)
            {
                ExecuteUserInput(instruction.input_usuario());
            }
            else if (instruction.asignacion_suma() != null)
            {
                ExecuteSumAssignment(instruction.asignacion_suma());
            }
            else if (instruction.ciclo_loop() != null)
            {
                ExecuteLoop(instruction.ciclo_loop());
            }
            else if (instruction.check_block() != null)
            {
                ExecuteCheck(instruction.check_block());
            }
            else if (instruction.salida() != null)
            {
                ExecuteOutput(instruction.salida());
            }
        }

        private void ExecuteVarDeclaration(Team1Parser.Var_declContext declaration)
        {
            // VAR a, b = 0 asigna el mismo valor inicial a todas las variables.
            double value = ParseNumber(declaration.numero().GetText());

            foreach (var id in declaration.ID())
            {
                variables[id.GetText()] = value;
            }
        }

        private void ExecuteAssignment(Team1Parser.Input_datosContext assignment)
        {
            string name = assignment.ID().GetText();
            variables[name] = ParseNumber(assignment.numero().GetText());
        }

        private void ExecuteUserInput(Team1Parser.Input_usuarioContext input)
        {
            // INPUT muestra un mensaje y guarda en una variable el numero que escribe el usuario.
            string prompt = Unquote(input.STRING().GetText());
            string variableName = input.ID().GetText();

            while (true)
            {
                Console.Write(prompt);
                string? text = Console.ReadLine();

                if (text == null)
                {
                    throw new InvalidOperationException($"No se recibio un valor para '{variableName}'.");
                }

                if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                {
                    variables[variableName] = value;
                    return;
                }

                Console.WriteLine("Ingresa un número válido. Ejemplo: 95.5");
            }
        }

        private void ExecuteSumAssignment(Team1Parser.Asignacion_sumaContext assignment)
        {
            string target = assignment.ID().GetText();
            double left = EvaluateExpression(assignment.exp(0));
            double right = EvaluateExpression(assignment.exp(1));

            variables[target] = left + right;
        }

        private void ExecuteLoop(Team1Parser.Ciclo_loopContext loop)
        {
            // LOOP i IN range (5) ejecuta el bloque 5 veces y actualiza la variable i.
            string iteratorName = loop.ID().GetText();
            int repetitions = int.Parse(loop.INT().GetText(), CultureInfo.InvariantCulture);

            for (int i = 0; i < repetitions; i++)
            {
                variables[iteratorName] = i;
                ExecuteInstructions(loop.instrucciones());
            }
        }

        private void ExecuteCheck(Team1Parser.Check_blockContext check)
        {
            // Ejecuta solo el primer bloque cuya condicion sea verdadera.
            if (EvaluateCondition(check.condicion()))
            {
                ExecuteInstructions(check.instrucciones());
                return;
            }

            foreach (var elseifBlock in check.elseif_bloque())
            {
                if (EvaluateCondition(elseifBlock.condicion()))
                {
                    ExecuteInstructions(elseifBlock.instrucciones());
                    return;
                }
            }

            if (check.else_bloque() != null)
            {
                ExecuteInstructions(check.else_bloque().instrucciones());
            }
        }

        private bool EvaluateCondition(Team1Parser.CondicionContext condition)
        {
            // La condicion se evalua respetando la prioridad definida en la gramatica.
            return EvaluateOrCondition(condition.or_cond());
        }

        private bool EvaluateOrCondition(Team1Parser.Or_condContext condition)
        {
            foreach (var andCondition in condition.and_cond())
            {
                if (EvaluateAndCondition(andCondition))
                {
                    return true;
                }
            }

            return false;
        }

        private bool EvaluateAndCondition(Team1Parser.And_condContext condition)
        {
            foreach (var notCondition in condition.not_cond())
            {
                if (!EvaluateNotCondition(notCondition))
                {
                    return false;
                }
            }

            return true;
        }

        private bool EvaluateNotCondition(Team1Parser.Not_condContext condition)
        {
            // Permite negar condiciones con ! y agruparlas con parentesis.
            if (condition.OP_NOT() != null)
            {
                return !EvaluateNotCondition(condition.not_cond());
            }

            if (condition.condicion() != null)
            {
                return EvaluateCondition(condition.condicion());
            }

            return EvaluateComparison(condition.comparacion());
        }

        private bool EvaluateComparison(Team1Parser.ComparacionContext comparison)
        {
            double left = EvaluateExpression(comparison.exp(0));
            double right = EvaluateExpression(comparison.exp(1));
            string operation = comparison.OP_REL().GetText();

            return operation == "<="
                ? left <= right
                : left >= right;
        }

        private double EvaluateExpression(Team1Parser.ExpContext expression)
        {
            if (expression.ID() != null)
            {
                return GetVariableValue(expression.ID().GetText());
            }

            return ParseNumber(expression.GetText());
        }

        private void ExecuteOutput(Team1Parser.SalidaContext output)
        {
            // OUT imprime un valor, o concatena dos valores usando +.
            var values = output.valor_salida();
            string text = FormatOutputValue(values[0]);

            if (values.Length > 1)
            {
                text += FormatOutputValue(values[1]);
            }

            Console.WriteLine(text);
        }

        private string FormatOutputValue(Team1Parser.Valor_salidaContext value)
        {
            if (value.STRING() != null)
            {
                return Unquote(value.STRING().GetText());
            }

            if (value.ID() != null)
            {
                return FormatNumber(GetVariableValue(value.ID().GetText()));
            }

            return FormatNumber(ParseNumber(value.GetText()));
        }

        private double GetVariableValue(string name)
        {
            if (!variables.TryGetValue(name, out double value))
            {
                throw new InvalidOperationException($"La variable '{name}' no existe.");
            }

            return value;
        }

        private static double ParseNumber(string text)
        {
            return double.Parse(text, CultureInfo.InvariantCulture);
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private static string Unquote(string text)
        {
            return text.Substring(1, text.Length - 2);
        }
    }
}
