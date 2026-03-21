using Grovvy.Models;

namespace Grovvy.Services;

public class SintaxisService
{
    private List<Token> _tokens = new();
    private int _posicion = 0;
    private List<string> _errores = new();

    public List<string> AnalizarTokens(List<Token> tokens)
    {
        _tokens = tokens;
        _posicion = 0;
        _errores = new List<string>();

        // 1. Verificación global de llaves, paréntesis y corchetes
        VerificarBalanceoGlobal();

        // 2. Análisis lineal de la estructura
        while (_posicion < _tokens.Count)
        {
            AnalizarEstructura();
        }

        return _errores;
    }


 
    private Token TokenActual() => _tokens[_posicion];

    // Da un paso hacia adelante en la lista de tokens
    private void Avanzar()
    {
        if (_posicion < _tokens.Count) _posicion++;
    }

    // Verifica si el token actual es el que esperamos. Si lo es, avanza. Si no, anota un error.
    private void Requerir(string tipoEsperado, string mensajeError)
    {
        if (_posicion < _tokens.Count && TokenActual().Tipo == tipoEsperado)
        {
            Avanzar();
        }
        else
        {
            // Si nos quedamos sin tokens, tomamos la línea del último token válido
            int linea = _posicion < _tokens.Count ? TokenActual().Linea : _tokens.Last().Linea;
            _errores.Add($"Línea {linea}: Error sintáctico - {mensajeError}");
        }
    }

    // REGLAS GRAMATICALES

    private void AnalizarEstructura()
    {
        var token = TokenActual();

        // Regla: Definición de función
        if (token.Tipo == "RESERVADA" && token.Valor == "def")
        {
            Avanzar(); // Dejamos atrás el 'def'

            Requerir("IDENTIFICADOR", "Se esperaba el nombre de la función después de 'def'.");
            Requerir("(", "Se esperaba '(' después del nombre de la función.");

            // Saltamos todos los parámetros hasta encontrar el paréntesis de cierre
            while (_posicion < _tokens.Count && TokenActual().Tipo != ")")
            {
                Avanzar();
            }

            Requerir(")", "Se esperaba ')' para cerrar los parámetros.");
            Requerir("{", "Se esperaba '{' para iniciar el cuerpo de la función.");
            return;
        }

        // Regla: Condicional if
        if (token.Tipo == "RESERVADA" && token.Valor == "if")
        {
            Avanzar(); // Dejamos atrás el 'if'

            Requerir("(", "Se esperaba '(' después de 'if'.");

            if (_posicion < _tokens.Count && TokenActual().Tipo == ")")
            {
                _errores.Add($"Línea {TokenActual().Linea}: La condición del 'if' no puede estar vacía.");
            }

            // Saltamos la condición hasta encontrar el paréntesis de cierre
            while (_posicion < _tokens.Count && TokenActual().Tipo != ")")
            {
                Avanzar();
            }

            Requerir(")", "Se esperaba ')' al final de la condición.");
            Requerir("{", "Se esperaba '{' después de la condición del 'if'.");
            return;
        }

        // Regla: else
        if (token.Tipo == "RESERVADA" && token.Valor == "else")
        {
            Avanzar(); // Dejamos atrás el 'else'
            Requerir("{", "Se esperaba '{' después de 'else'.");
            return;
        }

        // Si es cualquier otra cosa (variables, matemáticas), simplemente lo pasamos
        Avanzar();
    }

    private void VerificarBalanceoGlobal()
    {
        var pila = new Stack<Token>();
        var pares = new Dictionary<string, string> { { "}", "{" }, { ")", "(" }, { "]", "[" } };

        foreach (var t in _tokens)
        {
            if (t.Tipo == "{" || t.Tipo == "(" || t.Tipo == "[") pila.Push(t);
            else if (t.Tipo == "}" || t.Tipo == ")" || t.Tipo == "]")
            {
                if (pila.Count == 0 || pila.Peek().Tipo != pares[t.Tipo])
                {
                    _errores.Add($"Línea {t.Linea}: Símbolo '{t.Tipo}' inesperado o sin apertura.");
                }
                else pila.Pop();
            }
        }

        while (pila.Count > 0)
        {
            var noCerrado = pila.Pop();
            _errores.Add($"Línea {noCerrado.Linea}: Símbolo de apertura '{noCerrado.Tipo}' sin cerrar.");
        }
    }
}