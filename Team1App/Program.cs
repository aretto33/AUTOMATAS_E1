using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.IO;

namespace Team1App
{
    class Program
    {
        static void Main(string[] args)
        {
            // Usamos el archivo recibido por consola; si no llega uno, queda un ejemplo base.
            string input = args.Length > 0
                ? File.ReadAllText(args[0])
                : @"STAR
//crear variables de los contadores
VAR aceptados, rechazado_edad, rechazado_prom, rechazado_ingreso = 0
VAR promedio_minimo = 80.5

LOOP i IN range (5):
    OUT ""Bienvenido alumno No."" + i
    INPUT ""Edad: "" edad
    INPUT ""Promedio: "" promedio
    INPUT ""Ingreso: "" ingreso
    CHECK:
        IF (!(edad <= 18) && !(edad >= 25) && promedio >= promedio_minimo && !(ingreso >= 5000)) SO:
            aceptados = aceptados + 1
            OUT ""Beca aceptada con promedio "" + promedio
        ELSEIF (edad <= 18 || edad >= 25) SO:
            rechazado_edad = rechazado_edad + 1
            OUT ""Rechazado: EDAD""
        ELSEIF (ingreso >= 5000) SO:
            rechazado_ingreso = rechazado_ingreso + 1
            OUT ""Rechazado: INGRESO""
        ELSE:
            rechazado_prom = rechazado_prom + 1
            OUT ""Rechazado: PROMEDIO""
    END_CHECK
END_LOOP

OUT ""Total: "" + aceptados
END";

            try
            {
                Console.WriteLine("Fase 1 - Analisis lexico");
                PrintLexicalAnalysis(input);

                var inputStream = new AntlrInputStream(input);
                var lexer = new Team1Lexer(inputStream);
                var tokens = new CommonTokenStream(lexer);
                var parser = new Team1Parser(tokens);
                var program = parser.start_rule();

                Console.WriteLine();
                Console.WriteLine("Fase 2 - Analisis sintactico");

                if (parser.NumberOfSyntaxErrors > 0)
                {
                    Console.WriteLine($">>> [ERROR]: Se encontraron {parser.NumberOfSyntaxErrors} errores de sintaxis.");
                    return;
                }

                PrintSyntacticAnalysis(program);

                Console.WriteLine();
                Console.WriteLine("Fase 3 - Analisis semantico");
                var semanticAnalyzer = new Team1SemanticAnalyzer();
                var semanticResult = semanticAnalyzer.Analyze(program);
                Console.WriteLine(semanticResult.Report);

                Console.WriteLine();
                Console.WriteLine(semanticResult.IsValid
                    ? "Resultado final: codigo valido."
                    : "Resultado final: codigo no valido.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(">>> [ERROR CRITICO]: " + ex.Message);
            }
        }

        private static void PrintLexicalAnalysis(string input)
        {
            var lexer = new Team1Lexer(new AntlrInputStream(input));
            int count = 0;

            Console.WriteLine("Resultado del analisis lexico");

            IToken token;
            while ((token = lexer.NextToken()).Type != TokenConstants.EOF)
            {
                string value = CleanTokenText(token.Text);

                if ((token.Type == Team1Lexer.INT || token.Type == Team1Lexer.FLOAT) && value.StartsWith("-"))
                {
                    PrintLexicalToken(token.Line, token.Column + 1, "MINUS", "-");
                    PrintLexicalToken(token.Line, token.Column + 2, "NUMBER", value.Substring(1));
                    count += 2;
                }
                else
                {
                    PrintLexicalToken(token.Line, token.Column + 1, GetDisplayTokenName(token), value);
                    count++;
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Tokens validos: {count}");
        }

        private static void PrintLexicalToken(int line, int column, string tokenName, string value)
        {
            Console.WriteLine($"[L:{line}, C:{column}] Token: {tokenName,-10} | Valor: {value}");
        }

        private static void PrintSyntacticAnalysis(Team1Parser.Start_ruleContext program)
        {
            Console.WriteLine("Gramatica");
            PrintGrammar();
            Console.WriteLine();
            Console.WriteLine("Resultado sintactico");
            Console.WriteLine("Regla inicial: start_rule");
            Console.WriteLine("Estado: cadena aceptada");
            Console.WriteLine("Herramienta usada: ANTLR, a partir de nuestra gramatica");
            Console.WriteLine();
            Console.WriteLine("Instrucciones reconocidas");

            var instructions = BuildInstructionNodes(program.instrucciones());
            foreach (var instruction in instructions)
            {
                Console.WriteLine($"- {instruction.Rule}: {instruction.Summary}");
            }
        }

        private static void PrintGrammar()
        {
            Console.WriteLine("S = start_rule");
            Console.WriteLine("1.  start_rule      -> START instrucciones END");
            Console.WriteLine("2.  instrucciones   -> instruccion instrucciones | epsilon");
            Console.WriteLine("3.  instruccion     -> var_decl | input_datos | input_usuario | ciclo_loop | check_block | salida | asignacion_suma");
            Console.WriteLine("4.  var_decl        -> VAR ID (, ID)* = numero");
            Console.WriteLine("5.  input_datos     -> ID = numero");
            Console.WriteLine("6.  input_usuario   -> INPUT STRING ID");
            Console.WriteLine("7.  asignacion_suma -> ID = exp + exp");
            Console.WriteLine("8.  ciclo_loop      -> LOOP ID IN range ( INT ) : instrucciones END_LOOP");
            Console.WriteLine("9.  check_block     -> CHECK : IF ( condicion ) SO : instrucciones elseif_bloque* else_bloque? END_CHECK");
            Console.WriteLine("10. condicion       -> comparacion | !condicion | condicion && condicion | condicion || condicion");
            Console.WriteLine("11. comparacion     -> exp OP_REL exp");
            Console.WriteLine("12. exp             -> ID | INT | FLOAT");
            Console.WriteLine("13. numero          -> INT | FLOAT");
            Console.WriteLine("14. salida          -> OUT valor_salida (+ valor_salida)?");
        }

        private static List<InstructionNode> BuildInstructionNodes(Team1Parser.InstruccionesContext instructions)
        {
            var nodes = new List<InstructionNode>();

            foreach (var instruction in instructions.instruccion())
            {
                if (instruction.var_decl() != null)
                {
                    var declaration = instruction.var_decl();
                    nodes.Add(new InstructionNode("var_decl", $"VAR {string.Join(", ", GetIds(declaration.ID()))} = {declaration.numero().GetText()}"));
                }
                else if (instruction.input_datos() != null)
                {
                    var assignment = instruction.input_datos();
                    nodes.Add(new InstructionNode("input_datos", $"{assignment.ID().GetText()} = {assignment.numero().GetText()}"));
                }
                else if (instruction.input_usuario() != null)
                {
                    var userInput = instruction.input_usuario();
                    nodes.Add(new InstructionNode("input_usuario", $"INPUT {userInput.STRING().GetText()} {userInput.ID().GetText()}"));
                }
                else if (instruction.asignacion_suma() != null)
                {
                    var sum = instruction.asignacion_suma();
                    nodes.Add(new InstructionNode("asignacion_suma", $"{sum.ID().GetText()} = {sum.exp(0).GetText()} + {sum.exp(1).GetText()}"));
                }
                else if (instruction.salida() != null)
                {
                    nodes.Add(new InstructionNode("salida", FormatOutputInstruction(instruction.salida())));
                }
                else if (instruction.ciclo_loop() != null)
                {
                    var loop = instruction.ciclo_loop();
                    var node = new InstructionNode("ciclo_loop", $"LOOP {loop.ID().GetText()} IN range ({loop.INT().GetText()})");
                    node.Children.AddRange(BuildInstructionNodes(loop.instrucciones()));
                    nodes.Add(node);
                }
                else if (instruction.check_block() != null)
                {
                    var check = instruction.check_block();
                    var node = new InstructionNode("check_block", "CHECK con IF, ELSEIF y ELSE");
                    node.Children.AddRange(BuildInstructionNodes(check.instrucciones()));

                    foreach (var elseifBlock in check.elseif_bloque())
                    {
                        var elseifNode = new InstructionNode("elseif_bloque", "ELSEIF (condicion)");
                        elseifNode.Children.AddRange(BuildInstructionNodes(elseifBlock.instrucciones()));
                        node.Children.Add(elseifNode);
                    }

                    if (check.else_bloque() != null)
                    {
                        var elseNode = new InstructionNode("else_bloque", "ELSE");
                        elseNode.Children.AddRange(BuildInstructionNodes(check.else_bloque().instrucciones()));
                        node.Children.Add(elseNode);
                    }

                    nodes.Add(node);
                }
            }

            return nodes;
        }

        private static List<string> GetIds(IEnumerable<ITerminalNode> ids)
        {
            var names = new List<string>();

            foreach (var id in ids)
            {
                names.Add(id.GetText());
            }

            return names;
        }

        private static string FormatOutputInstruction(Team1Parser.SalidaContext output)
        {
            var values = output.valor_salida();
            string text = "OUT " + values[0].GetText();

            if (values.Length > 1)
            {
                text += " + " + values[1].GetText();
            }

            return text;
        }

        private static string GetDisplayTokenName(IToken token)
        {
            switch (token.Type)
            {
                case Team1Lexer.START:
                    return token.Text == "STAR" ? "STAR" : "START";
                case Team1Lexer.OP_ASIG:
                    return "ASSIGN";
                case Team1Lexer.OP_ARIT:
                    return "PLUS";
                case Team1Lexer.OP_REL:
                    return token.Text == "<=" ? "LE" : "GE";
                case Team1Lexer.OP_OR:
                    return "OR";
                case Team1Lexer.OP_AND:
                    return "AND";
                case Team1Lexer.OP_NOT:
                    return "NOT";
                case Team1Lexer.PARENT_I:
                    return "LPAR";
                case Team1Lexer.PARENT_D:
                    return "RPAR";
                case Team1Lexer.INT:
                case Team1Lexer.FLOAT:
                    return "NUMBER";
                default:
                    return Team1Lexer.DefaultVocabulary.GetSymbolicName(token.Type)
                        ?? token.Type.ToString();
            }
        }

        private static string CleanTokenText(string text)
        {
            return text
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        private class InstructionNode
        {
            public InstructionNode(string rule, string summary)
            {
                Rule = rule;
                Summary = summary;
            }

            public string Rule { get; }
            public string Summary { get; }
            public List<InstructionNode> Children { get; } = new List<InstructionNode>();
        }
    }
}
