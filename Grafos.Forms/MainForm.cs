using NoLineales.Core;
namespace Grafos.Forms;

public partial class MainForm : Form
{
    private GrafoPonderado? grafo;
    private ResultadoDijkstra? resultado;
    public MainForm()
    {
        InitializeComponent();
        crear.Click += (_, _) => Operar(() => {
            string[] vertices = nombres.Text.Split(',').Select(n => n.Trim()).ToArray();
            if (vertices.Length > 12) throw new ArgumentException("El dibujo admite hasta 12 vértices.");
            var nuevo = new GrafoPonderado(vertices, dirigido.Checked); grafo = nuevo;
            resultado = null; tablero.Resultado = null;
            foreach (var combo in new[] { desde, hasta, origen, destino })
            { combo.Items.Clear(); combo.Items.AddRange(nuevo.Nombres.Cast<object>().ToArray()); combo.SelectedIndex = 0; }
            if (nuevo.Count > 1) { hasta.SelectedIndex = 1; destino.SelectedIndex = nuevo.Count - 1; }
            Invalidar();
        });
        conectar.Click += (_, _) => Operar(() => { Requerir().Conectar(desde.SelectedIndex, hasta.SelectedIndex, (long)peso.Value); Invalidar(); });
        quitar.Click += (_, _) => Operar(() => { Requerir().Desconectar(desde.SelectedIndex, hasta.SelectedIndex); Invalidar(); });
        resolver.Click += (_, _) => Operar(() => { resultado = Requerir().Dijkstra(origen.SelectedIndex); ActualizarResultado(); });
        destino.SelectedIndexChanged += (_, _) => { if (resultado is not null) ActualizarResultado(); };
        origen.SelectedIndexChanged += (_, _) => { if (resultado is not null) Invalidar(); };
        ActualizarRepresentacion();
    }
    private GrafoPonderado Requerir() => grafo ?? throw new InvalidOperationException("Primero crea una red.");
    private void Operar(Action accion)
    {
        try { accion(); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or OverflowException)
        { estado.Text = "Rechazado: " + ex.Message; }
    }
    private void Invalidar()
    {
        resultado = null; rutas.Rows.Clear(); traza.Rows.Clear(); tablero.Resultado = null;
        ActualizarRepresentacion(); estado.Text = "Red actualizada. Ejecuta Dijkstra para obtener resultados nuevos.";
    }
    private void ActualizarRepresentacion()
    {
        bool disponible = grafo is not null;
        foreach (Control control in new Control[] { conectar, quitar, resolver, desde, hasta, origen, destino, peso }) control.Enabled = disponible;
        matriz.Rows.Clear(); matriz.Columns.Clear(); lista.Text = grafo?.ListaAdyacencia() ?? "Crea una red para ver sus representaciones.";
        if (grafo is not null)
        {
            matriz.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "De / a", SortMode = DataGridViewColumnSortMode.NotSortable, Frozen = true });
            foreach (string n in grafo.Nombres) matriz.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = n, SortMode = DataGridViewColumnSortMode.NotSortable });
            for (int u = 0; u < grafo.Count; u++)
            {
                var fila = new object[grafo.Count + 1]; fila[0] = grafo.Nombres[u];
                for (int v = 0; v < grafo.Count; v++) fila[v + 1] = grafo.Peso(u, v)?.ToString() ?? "-";
                matriz.Rows.Add(fila);
            }
        }
        tablero.Grafo = grafo; tablero.Invalidate();
    }
    private void ActualizarResultado()
    {
        if (grafo is null || resultado is null) return;
        rutas.Rows.Clear(); traza.Rows.Clear();
        for (int v = 0; v < grafo.Count; v++)
        {
            int[] camino = resultado.CaminoHasta(v);
            rutas.Rows.Add(grafo.Nombres[v], GrafoPonderado.Formato(resultado.Distancias[v]),
                camino.Length == 0 ? "Sin ruta" : string.Join(" -> ", camino.Select(i => grafo.Nombres[i])));
        }
        foreach (var p in resultado.Traza)
        {
            string distancias = string.Join("; ", p.Distancias.Select((d, i) => $"{grafo.Nombres[i]}:{GrafoPonderado.Formato(d)}"));
            string padres = string.Join("; ", p.Predecesores.Select((a, i) => $"{grafo.Nombres[i]}:{(a < 0 ? "-" : grafo.Nombres[a])}"));
            traza.Rows.Add(p.Paso, grafo.Nombres[p.Vertice], distancias, padres, p.Relajaciones);
        }
        int fin = destino.SelectedIndex;
        if (fin >= 0)
        {
            estado.Text = $"Origen: {grafo.Nombres[resultado.Origen]}. Destino: {grafo.Nombres[fin]}. Costo mínimo: {GrafoPonderado.Formato(resultado.Distancias[fin])}. {(grafo.Dirigido ? "Dirigido" : "No dirigido")}.";
            tablero.Destino = fin;
        }
        tablero.Resultado = resultado; tablero.Invalidate();
    }
}
