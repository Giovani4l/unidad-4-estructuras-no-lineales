using NoLineales.Core;
using System.ComponentModel;
namespace ABB.Forms;

public sealed class ArbolCanvas : Control
{
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ArbolBinarioBusqueda? Arbol { get; set; }
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int? ValorResaltado { get; set; }
    public ArbolCanvas() { DoubleBuffered = true; BackColor = Color.White; ResizeRedraw = true; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); var g = e.Graphics; g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        if (Arbol?.Raiz is null) { g.DrawString("Árbol vacío. Ingresa claves para comenzar.", Font, Brushes.DimGray, 20, 20); return; }
        var posiciones = new Dictionary<Nodo, PointF>(); var pendientes = new Stack<(Nodo Nodo, int Nivel)>();
        var orden = Arbol.Inorden(); var indice = new Dictionary<int, int>();
        for (int i = 0; i < orden.Length; i++) indice[orden[i]] = i;
        pendientes.Push((Arbol.Raiz, 0));
        while (pendientes.Count > 0)
        {
            var (n, nivel) = pendientes.Pop(); posiciones[n] = new((indice[n.Valor] + 1f) * Width / (orden.Length + 1), 36 + nivel * 78);
            if (n.Izquierdo is not null) pendientes.Push((n.Izquierdo, nivel + 1));
            if (n.Derecho is not null) pendientes.Push((n.Derecho, nivel + 1));
        }
        using var enlace = new Pen(Color.SlateGray, 2);
        foreach (var (n, punto) in posiciones)
        {
            if (n.Izquierdo is not null) g.DrawLine(enlace, punto, posiciones[n.Izquierdo]);
            if (n.Derecho is not null) g.DrawLine(enlace, punto, posiciones[n.Derecho]);
        }
        foreach (var (n, punto) in posiciones)
        {
            using var relleno = new SolidBrush(n.Valor == ValorResaltado ? Color.DarkOrange : Color.FromArgb(40, 83, 124));
            g.FillEllipse(relleno, punto.X - 24, punto.Y - 24, 48, 48);
            string texto = n.Valor.ToString(); float tamano = Math.Max(7, Math.Min(11, 65f / texto.Length));
            using var fuente = new Font("Segoe UI", tamano, FontStyle.Bold);
            var medida = g.MeasureString(texto, fuente);
            g.DrawString(texto, fuente, Brushes.White, punto.X - medida.Width / 2, punto.Y - medida.Height / 2);
        }
    }
}
