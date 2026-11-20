namespace NoLineales.Core;

/// <summary>ABB propio sin balance automático. Las claves duplicadas se ignoran.</summary>
public sealed class ArbolBinarioBusqueda
{
    public const int Limite = 512;
    public Nodo? Raiz { get; private set; }
    public int Count { get; private set; }
    public bool Insertar(int valor)
    {
        Nodo? padre = null, actual = Raiz;
        while (actual is not null)
        {
            if (valor == actual.Valor) return false;
            padre = actual; actual = valor < actual.Valor ? actual.Izquierdo : actual.Derecho;
        }
        if (Count >= Limite) throw new InvalidOperationException("El laboratorio limita el ABB a 512 nodos.");
        var nuevo = new Nodo(valor);
        if (padre is null) Raiz = nuevo;
        else if (valor < padre.Valor) padre.Izquierdo = nuevo;
        else padre.Derecho = nuevo;
        Count++; return true;
    }
    public bool Buscar(int valor)
    {
        var actual = Raiz;
        while (actual is not null)
        {
            if (actual.Valor == valor) return true;
            actual = valor < actual.Valor ? actual.Izquierdo : actual.Derecho;
        }
        return false;
    }
    public bool Eliminar(int valor)
    {
        Nodo? padre = null, actual = Raiz;
        while (actual is not null && actual.Valor != valor)
        { padre = actual; actual = valor < actual.Valor ? actual.Izquierdo : actual.Derecho; }
        if (actual is null) return false;
        if (actual.Izquierdo is not null && actual.Derecho is not null)
        {
            // Sucesor: mínimo del subárbol derecho. Tiene como máximo un hijo.
            Nodo padreSucesor = actual, sucesor = actual.Derecho;
            while (sucesor.Izquierdo is not null) { padreSucesor = sucesor; sucesor = sucesor.Izquierdo; }
            actual.Valor = sucesor.Valor;
            padre = padreSucesor; actual = sucesor;
        }
        Nodo? reemplazo = actual.Izquierdo ?? actual.Derecho;
        if (padre is null) Raiz = reemplazo;
        else if (ReferenceEquals(padre.Izquierdo, actual)) padre.Izquierdo = reemplazo;
        else padre.Derecho = reemplazo;
        actual.Izquierdo = actual.Derecho = null;
        Count--; return true;
    }
    public int[] Inorden()
    {
        var salida = new List<int>(); var pila = new Stack<Nodo>(); var actual = Raiz;
        while (actual is not null || pila.Count > 0)
        {
            while (actual is not null) { pila.Push(actual); actual = actual.Izquierdo; }
            actual = pila.Pop(); salida.Add(actual.Valor); actual = actual.Derecho;
        }
        return salida.ToArray();
    }
    public int[] Preorden()
    {
        var salida = new List<int>(); var pila = new Stack<Nodo>();
        if (Raiz is not null) pila.Push(Raiz);
        while (pila.Count > 0)
        {
            var n = pila.Pop(); salida.Add(n.Valor);
            if (n.Derecho is not null) pila.Push(n.Derecho);
            if (n.Izquierdo is not null) pila.Push(n.Izquierdo);
        }
        return salida.ToArray();
    }
    public int[] Postorden()
    {
        var salida = new List<int>(); var pila = new Stack<(Nodo Nodo, bool Regreso)>();
        if (Raiz is not null) pila.Push((Raiz, false));
        while (pila.Count > 0)
        {
            var (n, regreso) = pila.Pop();
            if (regreso) { salida.Add(n.Valor); continue; }
            pila.Push((n, true));
            if (n.Derecho is not null) pila.Push((n.Derecho, false));
            if (n.Izquierdo is not null) pila.Push((n.Izquierdo, false));
        }
        return salida.ToArray();
    }
    /// <summary>Altura por aristas: árbol vacío -1, raíz sola 0.</summary>
    public int Altura
    {
        get
        {
            int maxima = -1; var pila = new Stack<(Nodo Nodo, int Nivel)>();
            if (Raiz is not null) pila.Push((Raiz, 0));
            while (pila.Count > 0)
            {
                var (n, nivel) = pila.Pop(); maxima = Math.Max(maxima, nivel);
                if (n.Izquierdo is not null) pila.Push((n.Izquierdo, nivel + 1));
                if (n.Derecho is not null) pila.Push((n.Derecho, nivel + 1));
            }
            return maxima;
        }
    }
    public bool ValidarIntegridad()
    {
        var vistos = new HashSet<Nodo>();
        var pila = new Stack<(Nodo Nodo, long Minimo, long Maximo)>();
        if (Raiz is not null) pila.Push((Raiz, long.MinValue, long.MaxValue));
        while (pila.Count > 0)
        {
            var (n, min, max) = pila.Pop();
            if (!vistos.Add(n) || n.Valor <= min || n.Valor >= max) return false;
            if (n.Izquierdo is not null) pila.Push((n.Izquierdo, min, n.Valor));
            if (n.Derecho is not null) pila.Push((n.Derecho, n.Valor, max));
        }
        return vistos.Count == Count;
    }
    public void Clear() { Raiz = null; Count = 0; }
}
