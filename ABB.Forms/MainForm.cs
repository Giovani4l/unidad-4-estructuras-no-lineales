using NoLineales.Core;
namespace ABB.Forms;

public partial class MainForm : Form
{
    private ArbolBinarioBusqueda arbol = new();
    public MainForm()
    {
        InitializeComponent();
        cargar.Click += (_, _) => Operar(() => {
            var partes = secuencia.Text.Split(new[] { ',', ';', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var nuevo = new ArbolBinarioBusqueda();
            foreach (string parte in partes)
            {
                if (!int.TryParse(parte, out int n)) throw new ArgumentException($"'{parte}' no es un entero Int32.");
                nuevo.Insertar(n);
                if (nuevo.Count > 63) throw new ArgumentException("La ventana admite hasta 63 claves únicas para el dibujo.");
            }
            arbol = nuevo; tablero.ValorResaltado = null; Registrar($"Árbol reemplazado: {arbol.Count} claves únicas.");
        });
        insertar.Click += (_, _) => Operar(() => {
            int n = (int)valor.Value;
            if (arbol.Count >= 63 && !arbol.Buscar(n)) throw new ArgumentException("La vista admite hasta 63 nodos.");
            Registrar(arbol.Insertar(n) ? $"Insertado {n}." : $"Duplicado {n}: no se modifica el árbol.");
            tablero.ValorResaltado = n;
        });
        buscar.Click += (_, _) => Operar(() => {
            int n = (int)valor.Value; bool encontrado = arbol.Buscar(n);
            tablero.ValorResaltado = encontrado ? n : null; Registrar(encontrado ? $"Encontrado {n}." : $"No existe {n}.");
        });
        eliminar.Click += (_, _) => Operar(() => {
            int n = (int)valor.Value; Registrar(arbol.Eliminar(n) ? $"Eliminado {n}." : $"No existe {n}; sin cambios."); tablero.ValorResaltado = null;
        });
        vaciar.Click += (_, _) => Operar(() => { arbol.Clear(); tablero.ValorResaltado = null; Registrar("Árbol vacío."); });
        zonaArbol.Resize += (_, _) => AjustarLienzo(); Actualizar();
    }
    private void Operar(Action accion)
    {
        try { accion(); Actualizar(); }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        { Registrar("Rechazado: " + ex.Message); estado.Text = ex.Message; }
    }
    private void Registrar(string texto) => bitacora.AppendText($"{DateTime.Now:HH:mm:ss}  {texto}{Environment.NewLine}");
    private void AjustarLienzo()
    {
        tablero.Width = Math.Max(zonaArbol.ClientSize.Width - 22, arbol.Count * 58 + 60);
        tablero.Height = Math.Max(zonaArbol.ClientSize.Height - 22, (arbol.Altura + 1) * 78 + 45);
        tablero.Invalidate();
    }
    private void Actualizar()
    {
        recorridos.Rows.Clear();
        recorridos.Rows.Add("Preorden: raíz, izquierda, derecha", string.Join(", ", arbol.Preorden()));
        recorridos.Rows.Add("Inorden: izquierda, raíz, derecha", string.Join(", ", arbol.Inorden()));
        recorridos.Rows.Add("Postorden: izquierda, derecha, raíz", string.Join(", ", arbol.Postorden()));
        estado.Text = $"Nodos: {arbol.Count}    Altura por aristas: {arbol.Altura}    Raíz: {arbol.Raiz?.Valor.ToString() ?? "vacío"}    Integridad: {(arbol.ValidarIntegridad() ? "correcta" : "ERROR")}";
        tablero.Arbol = arbol; AjustarLienzo();
    }
}
