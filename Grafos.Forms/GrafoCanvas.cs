using NoLineales.Core;
using System.ComponentModel;
using System.Drawing.Drawing2D;
namespace Grafos.Forms;

public sealed class GrafoCanvas : Control
{
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public GrafoPonderado? Grafo { get; set; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ResultadoDijkstra? Resultado { get; set; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Destino { get; set; }
    public GrafoCanvas() { DoubleBuffered = true; BackColor = Color.White; ResizeRedraw = true; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
        if (Grafo is null) { g.DrawString("Crea una red y agrega conexiones.", Font, Brushes.DimGray, 18, 18); return; }
        int n = Grafo.Count; var puntos = new PointF[n]; float radio = Math.Max(25, Math.Min(Width, Height) * .36f);
        for (int i = 0; i < n; i++)
        { double a = -Math.PI / 2 + 2 * Math.PI * i / n; puntos[i] = new(Width / 2f + radio * (float)Math.Cos(a), Height / 2f + radio * (float)Math.Sin(a)); }
        var elegidas = new HashSet<(int, int)>();
        if (Resultado is not null && Destino >= 0 && Destino < n)
        {
            int[] camino = Resultado.CaminoHasta(Destino);
            for (int i = 1; i < camino.Length; i++)
            { elegidas.Add((camino[i - 1], camino[i])); if (!Grafo.Dirigido) elegidas.Add((camino[i], camino[i - 1])); }
        }
        using var fuente = new Font("Segoe UI", 9F);
        for (int u = 0; u < n; u++) for (int v = 0; v < n; v++)
        {
            long? peso = Grafo.Peso(u, v); if (!peso.HasValue || (!Grafo.Dirigido && v < u)) continue;
            var a = puntos[u]; var b = puntos[v]; float dx = b.X - a.X, dy = b.Y - a.Y;
            float largo = Math.Max(1, MathF.Sqrt(dx * dx + dy * dy)), ux = dx / largo, uy = dy / largo;
            float offset = Grafo.Dirigido && Grafo.Peso(v, u).HasValue ? 8 : 0;
            float ox = -uy * offset, oy = ux * offset;
            var inicio = new PointF(a.X + ux * 27 + ox, a.Y + uy * 27 + oy);
            var fin = new PointF(b.X - ux * 27 + ox, b.Y - uy * 27 + oy);
            using var pen = new Pen(elegidas.Contains((u, v)) ? Color.ForestGreen : Color.SlateGray, elegidas.Contains((u, v)) ? 3.5f : 1.4f);
            using var flecha = new AdjustableArrowCap(4, 5, true);
            if (Grafo.Dirigido) pen.CustomEndCap = flecha;
            g.DrawLine(pen, inicio, fin);
            string texto = peso.Value.ToString(); var medida = g.MeasureString(texto, fuente);
            float x = (inicio.X + fin.X) / 2 - medida.Width / 2 - uy * 9;
            float y = (inicio.Y + fin.Y) / 2 - medida.Height / 2 + ux * 9;
            g.FillRectangle(Brushes.White, x - 2, y, medida.Width + 4, medida.Height);
            g.DrawString(texto, fuente, Brushes.Black, x, y);
        }
        for (int i = 0; i < n; i++)
        {
            var p = puntos[i]; using var relleno = new SolidBrush(Resultado?.Origen == i ? Color.DarkOrange : Color.FromArgb(40, 83, 124));
            g.FillEllipse(relleno, p.X - 26, p.Y - 26, 52, 52);
            string texto = Grafo.Nombres[i].Length > 7 ? Grafo.Nombres[i][..6] + "…" : Grafo.Nombres[i];
            var medida = g.MeasureString(texto, fuente); g.DrawString(texto, fuente, Brushes.White, p.X - medida.Width / 2, p.Y - medida.Height / 2);
        }
    }
}
