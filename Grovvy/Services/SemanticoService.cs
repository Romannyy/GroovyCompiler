using Grovvy.Models;

namespace Grovvy.Services;

public class SemanticoService
{
    private readonly Dictionary<string, string> tablaSimbolos = new();

    private readonly HashSet<string> funcionesPredefinidas = new()
    {
        "println", "print"
    };

    private readonly HashSet<string> tiposDatos = new()
    {
        "int", "String", "def", "List"
    };

    public List<string> AnalizarSemantica(List<Token> tokens)
    {
        tablaSimbolos.Clear();
        var errores = new List<string>();

        for (int i = 0; i < tokens.Count; i++)
        {
            var actual = tokens[i];

            if (actual.Tipo == "RESERVADA" && tiposDatos.Contains(actual.Valor))
            {
                if (i + 1 < tokens.Count)
                {
                    var siguiente = tokens[i + 1];

                    if (siguiente.Tipo == "IDENTIFICADOR")
                    {
                        if (tablaSimbolos.ContainsKey(siguiente.Valor))
                        {
                            errores.Add($"Linea {siguiente.Linea}: Variable '{siguiente.Valor}' ya fue declarada.");
                        }
                        else
                        {
                            tablaSimbolos[siguiente.Valor] = actual.Valor;
                        }
                    }
                }
                continue;
            }

            if (actual.Tipo == "IDENTIFICADOR")
            {
                if (funcionesPredefinidas.Contains(actual.Valor))
                    continue;

                if (i > 0 && tokens[i - 1].Tipo == "RESERVADA" && tiposDatos.Contains(tokens[i - 1].Valor))
                    continue;

                if (!tablaSimbolos.ContainsKey(actual.Valor))
                {
                    errores.Add($"Linea {actual.Linea}: Variable '{actual.Valor}' no ha sido declarada.");
                }
            }
        }

        return errores;
    }
}