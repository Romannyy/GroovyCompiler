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

        VerificarBalanceoGlobal();

        while (_posicion < _tokens.Count)
        {
            AnalizarEstructura();
        }

        return _errores;
    }


 
    private Token TokenActual() => _tokens[_posicion];

    private void Avanzar()
    {
        if (_posicion < _tokens.Count) _posicion++;
    }

    private void Requerir(string tipoEsperado, string mensajeError)
    {
        if (_posicion < _tokens.Count && TokenActual().Tipo == tipoEsperado)
        {
            Avanzar();
        }
        else
        {
            int linea = _posicion < _tokens.Count ? TokenActual().Linea : _tokens.Last().Linea;
            _errores.Add($"Línea {linea}: Error sintáctico - {mensajeError}");
        }
    }

    // REGLAS GRAMATICALES

    private void AnalizarEstructura()
    {
        var token = TokenActual();

        if (token.Tipo == "RESERVADA" && token.Valor == "def")
        {
            bool esFuncion = false;
            if (_posicion + 2 < _tokens.Count)
            {
                var tokenDecisivo = _tokens[_posicion + 2];
                if (tokenDecisivo.Tipo == "(")
                {
                    esFuncion = true;
                }
            }

            Avanzar();

            if (esFuncion)
            {
                Requerir("IDENTIFICADOR", "Se esperaba el nombre de la función después de 'def'.");
                Requerir("(", "Se esperaba '(' después del nombre de la función.");

                while (_posicion < _tokens.Count && TokenActual().Tipo != ")")
                {
                    Avanzar();
                }

                Requerir(")", "Se esperaba ')' para cerrar los parámetros.");
                Requerir("{", "Se esperaba '{' para iniciar el cuerpo de la función.");
            }
            else
            {
                Requerir("IDENTIFICADOR", "Se esperaba el nombre de la variable después de 'def'.");

                if (_posicion < _tokens.Count && TokenActual().Valor == "=")
                {
                    Avanzar();
                }
            }
            return;
        }

        if (token.Tipo == "RESERVADA" && token.Valor == "if")
        {
            Avanzar(); 

            Requerir("(", "Se esperaba '(' después de 'if'.");

            if (_posicion < _tokens.Count && TokenActual().Tipo == ")")
            {
                _errores.Add($"Línea {TokenActual().Linea}: La condición del 'if' no puede estar vacía.");
            }

            while (_posicion < _tokens.Count && TokenActual().Tipo != ")")
            {
                Avanzar();
            }

            Requerir(")", "Se esperaba ')' al final de la condición.");
            Requerir("{", "Se esperaba '{' después de la condición del 'if'.");
            return;
        }

        if (token.Tipo == "RESERVADA" && token.Valor == "else")
        {
            Avanzar(); 
            Requerir("{", "Se esperaba '{' después de 'else'.");
            return;
        }

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