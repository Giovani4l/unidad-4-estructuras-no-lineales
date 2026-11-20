# Unidad 4: Estructuras no lineales

Axel Giovani Gonzalez Lopez · Matrícula I24050379 · Docente Ruben Riojas.

## Requisitos y ejecución

Windows, .NET Desktop Runtime 10 para los ejecutables y SDK .NET 10 para compilar.
Abrir `Unidad4.sln` en Visual Studio compatible con .NET 10. Seleccionar como
proyecto de inicio **ABB.Forms** o **Grafos.Forms**. Los dos son Windows Forms;
`MainForm.Designer.cs` contiene el diseño y `MainForm.cs` los eventos.

```powershell
dotnet build Unidad4.sln -c Release -m:1
dotnet run --project ABB.Forms -c Release
dotnet run --project Grafos.Forms -c Release
dotnet run --project Verificaciones -c Release -- evidencias-nuevas
dotnet run --project PruebasInterfaz -c Release -- evidencias-interfaz-nuevas
```

Las últimas dos instrucciones ejecutan pruebas; no abren la aplicación de uso
normal. No se requieren paquetes NuGet externos. Las aplicaciones mantienen
la sesión en memoria; al cerrarlas se descartan los datos.

## Tarea 4.2: ABB

1. Escribir claves enteras separadas por comas y pulsar Cargar / reemplazar.
2. Usar el control numérico para insertar, buscar y eliminar claves individuales.
3. Revisar el dibujo, los tres recorridos, la cantidad, altura y verificación de integridad.

`NoLineales.Core/Nodo.cs` y `ArbolBinarioBusqueda.cs` implementan el árbol con
nodos dinámicos propios. Se ignoran duplicados. El ABB no se balancea. La altura
se mide en aristas: vacío -1 y raíz sola 0. La biblioteca admite 512 nodos y la
vista 63 para mantener un dibujo manejable; el lienzo permite desplazamiento.
Los recorridos usan pilas explícitas y no cambian el árbol. La eliminación con
dos hijos utiliza el sucesor inorden. Las operaciones básicas cuestan O(h);
recorrer y refrescar todos los resultados añade O(n).

Prueba guiada: cargar 50,30,70,20,40,60,80; eliminar 20, luego 30 y luego 50.
El resultado debe tener raíz 60, Inorden 40,60,70,80, Preorden 60,40,70,80 y
Postorden 40,80,70,60.

## Tarea 4.3: grafo y Dijkstra

1. Crear vértices con nombres separados por comas y elegir si el grafo es dirigido.
2. Agregar conexiones con los selectores y el control de peso. Una conexión
   existente actualiza su peso. Eliminar conexión retira la arista seleccionada.
3. Elegir origen y destino y calcular. Consultar las pestañas de red/rutas,
   matriz/lista y traza. Cambiar destino reutiliza el cálculo del mismo origen.

La vista admite 12 vértices y la biblioteca 30. Nombres únicos, de 1 a 18 caracteres;
no hay lazos ni aristas paralelas. Pesos enteros de 0 a 1 000 000 000. El peso cero
es válido; un guion en la matriz significa ausencia de arista. Las distancias
usan `long`. Los pesos negativos se rechazan al introducirlos, porque Dijkstra
requiere pesos no negativos. Se usa matriz de adyacencia; la lista mostrada es
una vista equivalente calculada, no un segundo almacenamiento.

Dijkstra selecciona el menor vértice pendiente por exploración de la matriz,
con desempate por índice de creación y actualización solo ante mejora estricta.
Tiempo O(V²), matriz O(V²), arreglos auxiliares O(V). Guardar las instantáneas de
la traza agrega O(V²) de memoria. Un destino inaccesible muestra INF y no tiene
camino. Cambiar conexiones, origen o red invalida los resultados previos.

Ejemplo ficticio no dirigido: A-B:4, A-C:2, B-C:1, B-D:5, C-D:8, C-E:10,
D-E:2, D-F:6, E-F:3. Desde A hasta F: A-C-B-D-E-F, costo 13. Los nombres y
pesos son editables; se resuelve la red ingresada por el usuario.

## Evidencia y documentación

`evidencias/pruebas.txt`: 10 448 comprobaciones de algoritmos aprobadas, con
comparación aleatoria del ABB contra un conjunto ordenado de referencia y de
Dijkstra contra Floyd-Warshall. Esas estructuras de referencia pertenecen solo
a las pruebas. La aplicación utiliza el ABB propio y Dijkstra propio.
`pruebas_interfaz.txt`: 27 comprobaciones de eventos y presentación.
`resultados.json`, `adyacencia.txt` y `traza_dijkstra.txt`: resultados generados
por ejecución. `compilacion.txt`: compilación sin errores ni advertencias.

Los PNG son renders de formularios ejecutados, obtenidos con `DrawToBitmap`;
no son capturas del escritorio. Los PDF del paquete general describen las
condiciones de prueba, los resultados y las referencias bibliográficas.

Repositorio público de código: https://github.com/Giovani4l/unidad-4-estructuras-no-lineales. Los enlaces de los tres videos se añadirán después de publicarlos.
Los guiones incluidos orientan la grabación con pantalla y cámara visible en
la esquina inferior derecha, usando un solo canal del alumno.
