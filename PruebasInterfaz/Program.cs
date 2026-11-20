using System.Reflection;
using System.Drawing.Imaging;
internal static class Program
{
    private static int aprobadas;
    private static T Campo<T>(Form f, string n) where T : class =>
        (T)f.GetType().GetField(n, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(f)!;
    private static void Click(Form f, string n)
    {
        typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(Campo<Button>(f, n), new object[] { EventArgs.Empty }); Application.DoEvents();
    }
    private static void Check(bool ok, string caso)
    { if (!ok) throw new Exception("FALLO: " + caso); aprobadas++; Console.WriteLine("PASS " + caso); }
    private static IEnumerable<Control> Hijos(Control raiz)
    {
        foreach (Control c in raiz.Controls) { yield return c; foreach (var h in Hijos(c)) yield return h; }
    }
    private static void Render(Form f, string archivo)
    {
        f.Show(); Application.DoEvents(); f.PerformLayout();
        foreach (var panel in Hijos(f).OfType<FlowLayoutPanel>().Where(p => p.Visible))
            Check(panel.Controls.Cast<Control>().All(c => c.Right <= panel.ClientSize.Width && c.Bottom <= panel.ClientSize.Height), "fila sin recorte: " + panel.Name);
        using var bitmap = new Bitmap(f.Width, f.Height);
        f.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size)); bitmap.Save(archivo, ImageFormat.Png);
    }
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        string carpeta = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.Combine(Environment.CurrentDirectory, "evidencias");
        Directory.CreateDirectory(carpeta);
        using var abb = new ABB.Forms.MainForm();
        Campo<TextBox>(abb, "secuencia").Text = "50,30,70,20,40,60,80"; Click(abb, "cargar");
        var recorridos = Campo<DataGridView>(abb, "recorridos");
        Check(recorridos.Rows[1].Cells[1].Value?.ToString() == "20, 30, 40, 50, 60, 70, 80", "inorden calculado por la interfaz");
        Campo<NumericUpDown>(abb, "valor").Value = 40; Click(abb, "buscar");
        Check(Campo<TextBox>(abb, "bitacora").Text.Contains("Encontrado 40"), "búsqueda real desde botón");
        Render(abb, Path.Combine(carpeta, "ABB_inicial.png"));
        foreach (int v in new[] { 20, 30, 50 }) { Campo<NumericUpDown>(abb, "valor").Value = v; Click(abb, "eliminar"); }
        Check(recorridos.Rows[1].Cells[1].Value?.ToString() == "40, 60, 70, 80" && Campo<Label>(abb, "estado").Text.Contains("Raíz: 60"), "tres casos de eliminación en interfaz");
        Campo<NumericUpDown>(abb, "valor").Value = 60; Click(abb, "insertar");
        Check(Campo<Label>(abb, "estado").Text.Contains("Nodos: 4"), "duplicado no cambia cantidad");
        Campo<TextBox>(abb, "secuencia").Text = "10, texto"; Click(abb, "cargar");
        Check(recorridos.Rows[1].Cells[1].Value?.ToString() == "40, 60, 70, 80", "carga inválida conserva árbol anterior");
        Campo<NumericUpDown>(abb, "valor").Value = 70; Click(abb, "buscar");
        Render(abb, Path.Combine(carpeta, "ABB_eliminaciones.png")); abb.Hide();

        using var form = new Grafos.Forms.MainForm();
        Campo<TextBox>(form, "nombres").Text = "A,B,C,D,E,F"; Click(form, "crear");
        void Conectar(int u, int v, int w)
        {
            Campo<ComboBox>(form, "desde").SelectedIndex = u; Campo<ComboBox>(form, "hasta").SelectedIndex = v;
            Campo<NumericUpDown>(form, "peso").Value = w; Click(form, "conectar");
        }
        foreach (var e in new[] { (0,1,4),(0,2,2),(1,2,1),(1,3,5),(2,3,8),(2,4,10),(3,4,2),(3,5,6),(4,5,3) }) Conectar(e.Item1,e.Item2,e.Item3);
        Campo<ComboBox>(form, "origen").SelectedIndex = 0; Campo<ComboBox>(form, "destino").SelectedIndex = 5; Click(form, "resolver");
        var rutas = Campo<DataGridView>(form, "rutas");
        Check(rutas.Rows[5].Cells[1].Value?.ToString() == "13" && rutas.Rows[5].Cells[2].Value?.ToString() == "A -> C -> B -> D -> E -> F", "ruta mínima calculada en ventana");
        Check(Campo<DataGridView>(form, "traza").Rows.Count == 6, "traza completa en ventana");
        Render(form, Path.Combine(carpeta, "Grafo_ruta.png"));
        Campo<TabControl>(form, "pestanas").SelectedIndex = 1;
        var matriz = Campo<DataGridView>(form, "matriz");
        Check(matriz.Rows[0].Cells[2].Value?.ToString() == "4" && matriz.Rows[1].Cells[1].Value?.ToString() == "4", "matriz no dirigida simétrica");
        Render(form, Path.Combine(carpeta, "Grafo_matriz.png"));
        Campo<TabControl>(form, "pestanas").SelectedIndex = 2;
        Render(form, Path.Combine(carpeta, "Grafo_traza.png"));
        Conectar(0,1,0); Check(rutas.Rows.Count == 0, "cambio de arista invalida resultados");
        Click(form, "resolver"); Check(rutas.Rows[1].Cells[1].Value?.ToString() == "0", "arista de peso cero es válida");
        Conectar(0,1,-1); Check(Campo<Label>(form, "estado").Text.StartsWith("Rechazado"), "peso negativo rechazado");
        Campo<ComboBox>(form, "origen").SelectedIndex = 1; Check(rutas.Rows.Count == 0, "nuevo origen invalida resultados");
        Campo<TextBox>(form, "nombres").Text = "X,Y"; Click(form, "crear");
        Check(Campo<DataGridView>(form, "matriz").Rows.Count == 2 && rutas.Rows.Count == 0, "reemplazo de red descarta resultado anterior");
        Click(form, "resolver"); Check(rutas.Rows[1].Cells[1].Value?.ToString() == "INF", "destino desconectado visible");
        Console.WriteLine($"RESULTADO: {aprobadas} comprobaciones de interfaz aprobadas");
        Console.WriteLine("Las imágenes son renders directos de WinForms mediante DrawToBitmap, no capturas del escritorio.");
    }
}
