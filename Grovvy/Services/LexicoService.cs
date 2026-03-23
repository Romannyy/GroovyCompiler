using System.Text;
using Grovvy.Models;

namespace Grovvy.Services;

public class LexicoService
{
    private readonly HashSet<string> palabrasReservadas = new()
    {
        "if", "else", "while", "for", "class", "def", "return",
        "true", "false", "null", "new", "in", "int", "String", "List"
    };

    public (List<Token> tokens, List<string> errores) AnalizarCodigo(string codigo)
    {
        var tokens = new List<Token>();
        var errores = new List<string>();

        int linea = 1;
        int i = 0;

        while (i < codigo.Length)
        {
            char actual = codigo[i];

            if (actual == '\n') { linea++; i++; continue; }
            if (char.IsWhiteSpace(actual)) { i++; continue; }

            if (actual == '/')
            {
                if (i + 1 < codigo.Length && codigo[i + 1] == '/')
                {
                    while (i < codigo.Length && codigo[i] != '\n') i++;
                    continue;
                }
                else if (i + 1 < codigo.Length && codigo[i + 1] == '*')
                {
                    i += 2;
                    bool cerrado = false;
                    while (i < codigo.Length - 1)
                    {
                        if (codigo[i] == '\n') linea++;
                        if (codigo[i] == '*' && codigo[i + 1] == '/')
                        {
                            cerrado = true;
                            i += 2;
                            break;
                        }
                        i++;
                    }
                    if (!cerrado) errores.Add($"Línea {linea}: Comentario de bloque /* no cerrado.");
                    continue;
                }
            }

            if (char.IsLetter(actual) || actual == '_')
            {
                var sb = new StringBuilder();
                while (i < codigo.Length && (char.IsLetterOrDigit(codigo[i]) || codigo[i] == '_'))
                {
                    sb.Append(codigo[i]);
                    i++;
                }
                string palabra = sb.ToString();
                tokens.Add(new Token
                {
                    Tipo = palabrasReservadas.Contains(palabra) ? "RESERVADA" : "IDENTIFICADOR",
                    Valor = palabra,
                    Linea = linea
                });
                continue;
            }

            if (char.IsDigit(actual))
            {
                var sb = new StringBuilder();
                bool tienePunto = false;
                while (i < codigo.Length && (char.IsDigit(codigo[i]) || (codigo[i] == '.' && !tienePunto)))
                {
                    if (codigo[i] == '.') tienePunto = true;
                    sb.Append(codigo[i]);
                    i++;
                }
                tokens.Add(new Token { Tipo = "NUMERO", Valor = sb.ToString(), Linea = linea });
                continue;
            }

            // 5. Strings
            if (actual == '"' || actual == '\'')
            {
                char comilla = actual;
                var sb = new StringBuilder();
                i++; // Saltar la comilla de apertura
                bool cerrado = false;

                while (i < codigo.Length)
                {
                    if (codigo[i] == comilla) { cerrado = true; i++; break; }
                    if (codigo[i] == '\n') linea++;
                    sb.Append(codigo[i]);
                    i++;
                }

                if (!cerrado) errores.Add($"Línea {linea}: Cadena de texto no cerrada.");
                else tokens.Add(new Token { Tipo = "STRING", Valor = sb.ToString(), Linea = linea });
                continue;
            }

            // 6. Operadores
            if ("+-*/=><!&|".Contains(actual))
            {
                string op = actual.ToString();
                if (i + 1 < codigo.Length)
                {
                    char sig = codigo[i + 1];
                    if ((actual == '&' && sig == '&') || (actual == '|' && sig == '|') || "=".Contains(sig))
                    {
                        op += sig;
                        i++;
                    }
                }
                tokens.Add(new Token { Tipo = "OPERADOR", Valor = op, Linea = linea });
                i++;
                continue;
            }

            // 7. Símbolos de Puntuación
            switch (actual)
            {
                case '(':
                case ')':
                case '{':
                case '}':
                case '[':
                case ']':
                case ';':
                case '.':
                case ',':
                case ':':
                    tokens.Add(new Token { Tipo = actual.ToString(), Valor = actual.ToString(), Linea = linea });
                    break;
                default:
                    errores.Add($"Línea {linea}: Carácter léxico no reconocido '{actual}'");
                    break;
            }
            i++;
        }

        return (tokens, errores);
    }
}