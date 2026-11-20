using NoLineales.Core;
using System.Text.Json;
using System.Runtime.InteropServices;

string carpeta = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.Combine(Environment.CurrentDirectory, "evidencias");
Directory.CreateDirectory(carpeta); int aprobadas = 0;
void Check(bool ok, string caso) { if (!ok) throw new Exception("FALLO: " + caso); aprobadas++; }
void Rechaza(Action accion)
{
    try { accion(); }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException) { aprobadas++; return; }
    throw new Exception("No se rechazó la entrada inválida.");
}
var arbol = new ArbolBinarioBusqueda(); int[] claves = { 50, 30, 70, 20, 40, 60, 80 };
foreach (int n in claves) arbol.Insertar(n);
var estados = new List<object>();
void Guardar(string accion) => estados.Add(new { Accion = accion, arbol.Count, arbol.Altura, Raiz = arbol.Raiz?.Valor,
    Preorden = arbol.Preorden(), Inorden = arbol.Inorden(), Postorden = arbol.Postorden(), Integro = arbol.ValidarIntegridad() });
Check(arbol.Preorden().SequenceEqual(new[] { 50, 30, 20, 40, 70, 60, 80 }), "preorden conocido");
Check(arbol.Inorden().SequenceEqual(new[] { 20, 30, 40, 50, 60, 70, 80 }), "inorden conocido");
Check(arbol.Postorden().SequenceEqual(new[] { 20, 40, 30, 60, 80, 70, 50 }), "postorden conocido");
Guardar("Inserción de 50,30,70,20,40,60,80");
foreach (int n in new[] { 20, 30, 50 })
{
    Check(arbol.Eliminar(n) && !arbol.Buscar(n) && arbol.ValidarIntegridad(), "eliminar caso " + n);
    int[] pre = n == 20 ? new[] { 50, 30, 40, 70, 60, 80 } : n == 30 ? new[] { 50, 40, 70, 60, 80 } : new[] { 60, 40, 70, 80 };
    int[] post = n == 20 ? new[] { 40, 30, 60, 80, 70, 50 } : n == 30 ? new[] { 40, 60, 80, 70, 50 } : new[] { 40, 80, 70, 60 };
    Check(arbol.Preorden().SequenceEqual(pre), "preorden tras eliminar " + n);
    Check(arbol.Postorden().SequenceEqual(post), "postorden tras eliminar " + n);
    Guardar("Eliminar " + n);
}
Check(arbol.Inorden().SequenceEqual(new[] { 40, 60, 70, 80 }) && arbol.Raiz!.Valor == 60, "sucesor sustituye raíz");
Check(!arbol.Eliminar(999) && !arbol.Insertar(60), "ausente y duplicado no mutan");
arbol.Clear(); Check(arbol.Count == 0 && arbol.Altura == -1 && arbol.Preorden().Length == 0, "árbol vacío");
arbol.Insertar(int.MinValue); arbol.Insertar(int.MaxValue); Check(arbol.ValidarIntegridad(), "extremos Int32");
arbol.Eliminar(int.MinValue); Check(arbol.Raiz!.Valor == int.MaxValue && arbol.Count == 1, "raíz con un hijo");
arbol.Eliminar(int.MaxValue); Check(arbol.Raiz is null, "raíz única eliminada");
var random = new Random(24050379); var modelo = new SortedSet<int>(); arbol.Clear();
for (int paso = 0; paso < 2000; paso++)
{
    int n = random.Next(-200, 201), op = random.Next(3);
    if (op == 0) Check(arbol.Insertar(n) == modelo.Add(n), "inserción vs conjunto");
    else if (op == 1) Check(arbol.Eliminar(n) == modelo.Remove(n), "eliminación vs conjunto");
    else Check(arbol.Buscar(n) == modelo.Contains(n), "búsqueda vs conjunto");
    Check(arbol.ValidarIntegridad() && arbol.Count == modelo.Count, "integridad global ABB");
    Check(arbol.Inorden().SequenceEqual(modelo), "inorden ordenado y completo");
    Check(arbol.Preorden().Order().SequenceEqual(modelo) && arbol.Postorden().Order().SequenceEqual(modelo), "recorridos conservan todas las claves");
}
arbol.Clear(); for (int i = 0; i < ArbolBinarioBusqueda.Limite; i++) arbol.Insertar(i);
Check(arbol.Altura == 511 && arbol.ValidarIntegridad() && arbol.Postorden().Length == 512, "ABB degenerado sin recursión implícita");
Rechaza(() => arbol.Insertar(512));

var red = new GrafoPonderado(new[] { "A", "B", "C", "D", "E", "F" }, false);
int[][] aristas = { new[] { 0, 1, 4 }, new[] { 0, 2, 2 }, new[] { 1, 2, 1 }, new[] { 1, 3, 5 }, new[] { 2, 3, 8 },
    new[] { 2, 4, 10 }, new[] { 3, 4, 2 }, new[] { 3, 5, 6 }, new[] { 4, 5, 3 } };
foreach (var a in aristas) red.Conectar(a[0], a[1], a[2]);
var resultado = red.Dijkstra(0);
Check(resultado.Distancias.SequenceEqual(new long[] { 0, 3, 2, 8, 10, 13 }), "distancias del caso conocido");
Check(resultado.CaminoHasta(5).SequenceEqual(new[] { 0, 2, 1, 3, 4, 5 }), "camino A-C-B-D-E-F");
Check(resultado.Traza.Select(p => p.Vertice).SequenceEqual(new[] { 0, 2, 1, 3, 4, 5 }), "orden de fijación");
long inf = ResultadoDijkstra.Infinito;
long[][] pasosEsperados = { new long[] {0,4,2,inf,inf,inf}, new long[] {0,3,2,10,12,inf}, new long[] {0,3,2,8,12,inf},
    new long[] {0,3,2,8,10,14}, new long[] {0,3,2,8,10,13}, new long[] {0,3,2,8,10,13} };
for (int i = 0; i < pasosEsperados.Length; i++) Check(resultado.Traza[i].Distancias.SequenceEqual(pasosEsperados[i]), "traza conocida paso " + (i+1));
Rechaza(() => red.Conectar(0, 1, -1)); Rechaza(() => red.Conectar(0, 0, 2));
Rechaza(() => new GrafoPonderado(new[] { "A", "a" }, false)); Rechaza(() => red.Dijkstra(6));
var desconectado = new GrafoPonderado(new[] { "A", "B", "C", "D" }, true);
desconectado.Conectar(0, 1, 0); desconectado.Conectar(1, 2, 2);
Check(desconectado.Dijkstra(0).Distancias[2] == 2 && desconectado.Dijkstra(0).CaminoHasta(3).Length == 0, "peso cero y desconexión");
Check(desconectado.Dijkstra(1).CaminoHasta(0).Length == 0, "dirección respetada");
Check(resultado.CaminoHasta(0).SequenceEqual(new[] { 0 }), "camino del origen a sí mismo");
Check(red.Peso(0, 1) == 4, "rechazo de peso negativo no muta red");
var grande = new GrafoPonderado(new[] { "A", "B", "C", "D" }, true);
grande.Conectar(0, 1, 1000000000); grande.Conectar(1, 2, 1000000000); grande.Conectar(2, 3, 1000000000);
Check(grande.Dijkstra(0).Distancias[3] == 3000000000L, "costos Int64 mayores al rango Int32");
var empate = new GrafoPonderado(new[] { "A", "B", "C", "D" }, false);
empate.Conectar(0, 1, 1); empate.Conectar(0, 2, 1); empate.Conectar(1, 3, 1); empate.Conectar(2, 3, 1);
Check(empate.Dijkstra(0).CaminoHasta(3).SequenceEqual(new[] { 0, 1, 3 }), "empate resuelto por orden de vértices");
Rechaza(() => grande.Conectar(0, 1, 1000000001));
desconectado.Conectar(0, 1, 7); Check(desconectado.Peso(0, 1) == 7, "actualiza sin crear aristas paralelas");
desconectado.Desconectar(0, 1); Check(desconectado.Peso(0, 1) is null, "elimina arista");
for (int caso = 0; caso < 35; caso++)
{
    int n = random.Next(1, 10); var g = new GrafoPonderado(Enumerable.Range(0, n).Select(i => "V" + i).ToArray(), caso % 2 == 0);
    for (int u = 0; u < n; u++) for (int v = 0; v < n; v++)
        if (u != v && random.Next(4) == 0) g.Conectar(u, v, random.Next(0, 21));
    var d = new long[n, n];
    for (int u = 0; u < n; u++) for (int v = 0; v < n; v++) d[u, v] = u == v ? 0 : g.Peso(u, v) ?? ResultadoDijkstra.Infinito;
    // Oráculo independiente: Floyd-Warshall, no otra ejecución de Dijkstra.
    for (int k = 0; k < n; k++) for (int u = 0; u < n; u++) for (int v = 0; v < n; v++)
        if (d[u, k] < ResultadoDijkstra.Infinito && d[k, v] < ResultadoDijkstra.Infinito)
            d[u, v] = Math.Min(d[u, v], d[u, k] + d[k, v]);
    for (int u = 0; u < n; u++)
    {
        var r = g.Dijkstra(u);
        for (int v = 0; v < n; v++)
        {
            Check(r.Distancias[v] == d[u, v], "Dijkstra vs Floyd-Warshall");
            var camino = r.CaminoHasta(v);
            if (d[u, v] == ResultadoDijkstra.Infinito) Check(camino.Length == 0, "ausencia de camino");
            else
            {
                long costo = 0; for (int j = 1; j < camino.Length; j++) costo += g.Peso(camino[j - 1], camino[j])!.Value;
                Check(camino[0] == u && camino[^1] == v && costo == d[u, v] && camino.Distinct().Count() == camino.Length, "camino reconstruido óptimo");
            }
        }
    }
}
var matriz = new long?[red.Count][];
for (int u = 0; u < red.Count; u++) { matriz[u] = new long?[red.Count]; for (int v = 0; v < red.Count; v++) matriz[u][v] = red.Peso(u, v); }
var evidencia = new { Aprobadas = aprobadas, Runtime = RuntimeInformation.FrameworkDescription,
    SO = RuntimeInformation.OSDescription, Arquitectura = RuntimeInformation.ProcessArchitecture.ToString(),
    EstadosABB = estados, Nombres = red.Nombres, Aristas = aristas, Matriz = matriz,
    Distancias = resultado.Distancias, Predecesores = resultado.Predecesores, CaminoF = resultado.CaminoHasta(5), Traza = resultado.Traza };
File.WriteAllText(Path.Combine(carpeta, "resultados.json"), JsonSerializer.Serialize(evidencia, new JsonSerializerOptions { WriteIndented = true }));
File.WriteAllText(Path.Combine(carpeta, "adyacencia.txt"), red.ListaAdyacencia());
File.WriteAllLines(Path.Combine(carpeta, "traza_dijkstra.txt"), resultado.Traza.Select(p => $"Paso {p.Paso}; fijo {red.Nombres[p.Vertice]}; d=[{string.Join(',', p.Distancias.Select(GrafoPonderado.Formato))}]; {p.Relajaciones}"));
Console.WriteLine($"RESULTADO: {aprobadas} comprobaciones de algoritmos aprobadas");
