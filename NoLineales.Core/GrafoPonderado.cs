namespace NoLineales.Core;

/// <summary>Grafo simple, sin lazos, con matriz nullable: null significa ausencia; 0 es peso válido.</summary>
public sealed class GrafoPonderado
{
    private readonly string[] nombres;
    private readonly long?[,] pesos;
    public bool Dirigido { get; }
    public int Count => nombres.Length;
    public IReadOnlyList<string> Nombres => Array.AsReadOnly(nombres);
    public GrafoPonderado(string[] vertices, bool dirigido)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        if (vertices.Length < 1 || vertices.Length > 30) throw new ArgumentException("Usa entre 1 y 30 vértices.");
        nombres = vertices.Select(v => v?.Trim() ?? "").ToArray();
        if (nombres.Any(n => n.Length < 1 || n.Length > 18 || n.Any(char.IsControl))
            || nombres.Distinct(StringComparer.OrdinalIgnoreCase).Count() != nombres.Length)
            throw new ArgumentException("Los vértices deben ser únicos y tener de 1 a 18 caracteres, sin controles.");
        Dirigido = dirigido; pesos = new long?[Count, Count];
    }
    private void Validar(int v) { if (v < 0 || v >= Count) throw new ArgumentOutOfRangeException(nameof(v)); }
    public long? Peso(int origen, int destino) { Validar(origen); Validar(destino); return pesos[origen, destino]; }
    public void Conectar(int origen, int destino, long peso)
    {
        Validar(origen); Validar(destino);
        if (origen == destino) throw new ArgumentException("El modelo no admite lazos.");
        if (peso < 0 || peso > 1000000000) throw new ArgumentException("Dijkstra admite aquí pesos de 0 a 1000000000.");
        pesos[origen, destino] = peso;
        if (!Dirigido) pesos[destino, origen] = peso;
    }
    public void Desconectar(int origen, int destino)
    {
        Validar(origen); Validar(destino); pesos[origen, destino] = null;
        if (!Dirigido) pesos[destino, origen] = null;
    }
    public string ListaAdyacencia()
    {
        var salida = new System.Text.StringBuilder();
        for (int u = 0; u < Count; u++)
        {
            salida.Append(nombres[u]).Append(": "); bool primero = true;
            for (int v = 0; v < Count; v++) if (pesos[u, v].HasValue)
            {
                if (!primero) salida.Append(", ");
                salida.Append(nombres[v]).Append('(').Append(pesos[u, v]).Append(')'); primero = false;
            }
            if (primero) salida.Append("sin vecinos"); salida.AppendLine();
        }
        return salida.ToString(); // Vista derivada de la matriz, no un segundo almacenamiento.
    }
    public ResultadoDijkstra Dijkstra(int origen)
    {
        Validar(origen); int n = Count;
        var dist = Enumerable.Repeat(ResultadoDijkstra.Infinito, n).ToArray();
        var prev = Enumerable.Repeat(-1, n).ToArray(); var fijo = new bool[n]; var traza = new List<PasoDijkstra>();
        dist[origen] = 0;
        for (int paso = 1; paso <= n; paso++)
        {
            int u = -1;
            for (int v = 0; v < n; v++)
                if (!fijo[v] && (u == -1 || dist[v] < dist[u])) u = v;
            if (u == -1 || dist[u] == ResultadoDijkstra.Infinito) break;
            fijo[u] = true; var cambios = new List<string>();
            for (int v = 0; v < n; v++)
            {
                if (fijo[v] || !pesos[u, v].HasValue) continue;
                long candidato = checked(dist[u] + pesos[u, v]!.Value);
                if (candidato >= dist[v]) continue;
                cambios.Add($"{nombres[u]}->{nombres[v]}: {Formato(dist[v])} a {candidato}");
                dist[v] = candidato; prev[v] = u;
            }
            traza.Add(new(paso, u, cambios.Count == 0 ? "Sin mejoras" : string.Join("; ", cambios),
                Array.AsReadOnly((long[])dist.Clone()), Array.AsReadOnly((int[])prev.Clone())));
        }
        return new(origen, dist, prev, traza.AsReadOnly());
    }
    public static string Formato(long valor) => valor == ResultadoDijkstra.Infinito ? "INF" : valor.ToString();
}

public sealed record PasoDijkstra(int Paso, int Vertice, string Relajaciones,
    IReadOnlyList<long> Distancias, IReadOnlyList<int> Predecesores);

public sealed class ResultadoDijkstra
{
    public const long Infinito = long.MaxValue / 4;
    private readonly int[] predecesores;
    public int Origen { get; }
    public IReadOnlyList<long> Distancias { get; }
    public IReadOnlyList<int> Predecesores => Array.AsReadOnly(predecesores);
    public IReadOnlyList<PasoDijkstra> Traza { get; }
    internal ResultadoDijkstra(int origen, long[] distancias, int[] prev, IReadOnlyList<PasoDijkstra> traza)
    { Origen = origen; Distancias = Array.AsReadOnly(distancias); predecesores = prev; Traza = traza; }
    public int[] CaminoHasta(int destino)
    {
        if (destino < 0 || destino >= Distancias.Count) throw new ArgumentOutOfRangeException(nameof(destino));
        if (Distancias[destino] == Infinito) return Array.Empty<int>();
        var camino = new List<int>(); int actual = destino;
        while (actual != -1)
        {
            camino.Add(actual);
            if (camino.Count > Distancias.Count) throw new InvalidOperationException("Predecesores inconsistentes.");
            actual = predecesores[actual];
        }
        camino.Reverse(); return camino.ToArray();
    }
}
