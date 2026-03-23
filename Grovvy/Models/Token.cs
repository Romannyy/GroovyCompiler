namespace Grovvy.Models;

public class Token
{
    public string Tipo { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public int Linea { get; set; }
}
