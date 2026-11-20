namespace NoLineales.Core;

/// <summary>Nodo dinámico. Los enlaces solo se modifican desde la biblioteca.</summary>
public sealed class Nodo(int valor)
{
    public int Valor { get; internal set; } = valor;
    public Nodo? Izquierdo { get; internal set; }
    public Nodo? Derecho { get; internal set; }
}
