using System;
using System.Collections.Generic;
using UnityEngine;

public class CityGenerator : MonoBehaviour
{
    public KingdomController kingdomController;
    public int width = 20;  // Ancho de la cuadr�cula
    public int height = 20; // Alto de la cuadr�cula
    public GameObject poorRoadPrefab;  // Prefab para los caminos pobres
    public GameObject roadPrefab;  // Prefab para los caminos
    public GameObject plazaPrefab;  // Prefab para los caminos
    public GameObject[] buildingPrefabs; // Prefab para las zonas sin caminos
    public GameObject[] buildingPrefabsVariants; 
    public GameObject[] buildingPrefabsVariants2;
    public GameObject tallEmptyPrefab; // Prefab para las zonas Altas
    private Tile[,] grid;
    private List<GameObject> objects = new List<GameObject>();

    public enum ZoneType { Poor, Middle, Rich }

    public void CityStart()
    {
        if (kingdomController == null) return;
        UnityEngine.Random.InitState(kingdomController.ownSeed);
        switch (kingdomController.size)
        {
            case KingdomController.Size.Peque�o:
                width = 50;
                height = 50;
                break;
            case KingdomController.Size.Mediano:
                width = 66;
                height = 66;
                break;
            case KingdomController.Size.Grande:
                width = 80;
                height = 80;
                break;
        }
        foreach(GameObject obj in objects)
        {
            Destroy(obj);
        }
        objects.Clear();
        GenerateCity();
    }

    void GenerateCity()
    {
        grid = new Tile[width, height];

        // Inicializar casillas con posibles estados
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                ZoneType zone = GetZoneType(x, y);
                grid[x, y] = new Tile(x, y, zone);
            }
        }

        // Aplicar Wave Function Collapse
        CollapseWaveFunction();

        // Segunda pasada: conectar todos los caminos
        ConnectAllPaths();

        // Tercera pasada: conectar casillas diagonales
        ConnectDiagonalRoads();

        // Cuarta pasada: eliminar caminos aislados
        RemoveIsolatedRoads();

        // Quinta pasada: Crear caminos rectos adicionales en zona rica
        RandomRichPaths();

        // Sexta pasada: Crear estructuras por distribuci�n
        TallBuildings();
        FindPlazas(3);

        // Septima pasada: Detectar grupos y asignar tipos
        ClusterDetection(5);

        // Octava pasada: Aplicar m�scara para que no sea un cuadrado
        ApplyMask(5);

        // Instanciar los caminos en la escena
        InstantiateGrid();
    }

    void CollapseWaveFunction()
    {
        List<Tile> tilesToProcess = new List<Tile>();
        foreach (var tile in grid)
        {
            tilesToProcess.Add(tile);
        }

        while (tilesToProcess.Count > 0)
        {
            // Seleccionar la casilla con menos opciones posibles
            tilesToProcess.Sort((a, b) => a.PossibleStates.Count.CompareTo(b.PossibleStates.Count));
            Tile current = tilesToProcess[0];
            tilesToProcess.RemoveAt(0);

            if (current.PossibleStates.Count == 0) continue; // Evitar errores

            // Colapsar el tile asegurando conexiones
            current.Collapse(GetNeighbors(current,true));

            // Actualizar los vecinos
            foreach (Tile neighbor in GetNeighbors(current))
            {
                if (!neighbor.IsCollapsed)
                {
                    neighbor.UpdatePossibleStates(grid);
                }
            }
        }
    }

    void RemoveIsolatedRoads()
    {
        List<Tile> tilesToCheck = new List<Tile>();

        foreach (var tile in grid)
        {
            if (tile.IsRoad)
            {
                tilesToCheck.Add(tile);
            }
        }

        foreach (Tile tile in tilesToCheck)
        {
            int roadNeighbors = 0;
            foreach (Tile neighbor in GetNeighbors(tile))
            {
                if (neighbor.IsRoad) roadNeighbors++;
            }
            int aux = 2;
            if (tile.zone == ZoneType.Middle) aux = 1;
            // Si el camino tiene menos de aux conexiones, se eliminama
            if (roadNeighbors < aux)
            {
                tile.IsRoad = false;
                tile.IsCollapsed = true;
            }
        }
    }

    void ConnectDiagonalRoads()
    {
        List<Tile> tilesToCheck = new List<Tile>();

        foreach (var tile in grid)
        {
            if (!tile.IsRoad)
            {
                tilesToCheck.Add(tile);
            }
        }

        foreach (Tile tile in tilesToCheck)
        {
            List<Tile> diagonalNeighbors = GetNeighbors(tile, true);
            int diagonalRoads = 0;

            foreach (Tile neighbor in diagonalNeighbors)
            {
                if (neighbor.IsRoad) diagonalRoads++;
            }

            // Si hay exactamente dos caminos en diagonal, convertir la casilla en camino
            if (diagonalRoads == 2)
            {
                tile.IsRoad = true;
                tile.IsCollapsed = true;
            }
        }
    }

    void ConnectAllPaths() //Kruskal modificado
    {
        HashSet<Tile> visited = new HashSet<Tile>();
        List<HashSet<Tile>> roadClusters = new List<HashSet<Tile>>();

        // Encontrar todas las componentes de caminos desconectados
        foreach (var tile in grid)
        {
            if (tile.IsRoad && !visited.Contains(tile))
            {
                HashSet<Tile> cluster = new HashSet<Tile>();
                ExploreCluster(tile, cluster, visited);
                roadClusters.Add(cluster);
            }
        }

        // Conectar componentes desconectadas
        while (roadClusters.Count > 1)
        {
            var clusterA = roadClusters[0];
            var clusterB = roadClusters[1];

            Tile closestA = null, closestB = null;
            float minDistance = float.MaxValue;

            // Encontrar el par de caminos m�s cercanos entre los dos clusters
            foreach (var tileA in clusterA)
            {
                foreach (var tileB in clusterB)
                {
                    float distance = Mathf.Abs(tileA.x - tileB.x) + Mathf.Abs(tileA.y - tileB.y);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestA = tileA;
                        closestB = tileB;
                    }
                }
            }

            // Crear un camino entre los dos clusters
            CreatePathBetween(closestA, closestB);

            // Fusionar los clusters
            clusterA.UnionWith(clusterB);
            roadClusters.RemoveAt(1);
        }
    }

    void ExploreCluster(Tile start, HashSet<Tile> cluster, HashSet<Tile> visited)
    {
        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();
            cluster.Add(current);

            foreach (var neighbor in GetNeighbors(current, false))
            {
                if (neighbor.IsRoad && !visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
    }

    void CreatePathBetween(Tile start, Tile end)
    {
        int x = start.x;
        int y = start.y;

        while (x != end.x || y != end.y)
        {
            if (UnityEngine.Random.Range(0, 1f) < 0.5f)
            {
                if (x < end.x) x++;
                else if (x > end.x) x--;
                else
                {
                    if (y < end.y) y++;
                    else if (y > end.y) y--;
                }
            }
            else
            {
                if (y < end.y) y++;
                else if (y > end.y) y--;
                else
                {
                    if (x < end.x) x++;
                    else if (x > end.x) x--;
                }
            }
            if (!grid[x, y].IsRoad)
            {
                grid[x, y].IsRoad = true;
                grid[x, y].IsCollapsed = true;
            }
        }
    }

    public void RandomRichPaths()
    {
        List<Tile> richTiles = new List<Tile>(); // Lista para almacenar todos los tiles que pertenecen a la zona rica
        List<Tile> richPaths = new List<Tile>(); // Lista para almacenar los tiles de zona rica que ya son caminos
        // Recorrer toda la rejilla y a�adir a richTiles aquellos que est�n en zona rica
        foreach (Tile tile in grid)
        {
            if (tile.zone == ZoneType.Rich)
            {
                richTiles.Add(tile);
            }
        }
        // De los tiles ricos, seleccionar aquellos que son caminos y a�adirlos a richPaths
        foreach (Tile tile in richTiles)
        {
            if (tile.IsRoad)
            {
                richPaths.Add(tile);
            }
        }
        // Si hay pocos caminos en la zona rica, no se puede crear una conexi�n �til
        if (richPaths.Count<=1)
        {
            print("No rich path found");
            return;
        }
        int aux = richTiles.Count / 100;
        for (int i=0; i< aux; i++)
        {
            // Selecciona aleatoriamente dos caminos distintos de la zona rica
            Tile r1 = richPaths[UnityEngine.Random.Range(0, richPaths.Count)];
            Tile r2 = richPaths[UnityEngine.Random.Range(0, richPaths.Count)];

            // Crea un camino entre ambos
            CreatePathBetween(r1, r2);
        }
    }

    private void TallBuildings()
    {
        List<Tile> isolatedEmptyTiles = new List<Tile>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = grid[x, y];

                // Solo busca en la zona media y rica
                if (!tile.IsRoad && (tile.zone != ZoneType.Poor))
                {
                    int aux = 2;
                    if(tile.zone == ZoneType.Rich)
                    {
                        aux = 1;
                    }
                    if (IsSurroundedByEmptyTiles(tile, aux))
                    {
                        isolatedEmptyTiles.Add(tile);
                    }
                }
            }
        }

        foreach(Tile tile in isolatedEmptyTiles)
        {
            tile.tall = true;
        }
    }

    private bool IsSurroundedByEmptyTiles(Tile tile, int radius)
    {
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int nx = tile.x + dx;
                int ny = tile.y + dy;

                // Evitar verificar fuera de los l�mites del mapa
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    return false;

                // Si encontramos un camino dentro del radio, no est� rodeado completamente
                if (grid[nx, ny].IsRoad)
                    return false;
            }
        }

        return true; // Si todas las casillas en el radio son vac�as, entonces est� rodeado
    }

    private void FindPlazas(int size)
    {
        List<List<Tile>> roadBlocks = new List<List<Tile>>();

        for (int x = 0; x <= width - size; x++)
        {
            for (int y = 0; y <= height - size; y++)
            {
                List<Tile> block = GetRoadBlock(x, y, size);
                if (block != null)
                {
                    roadBlocks.Add(block);
                }
            }
        }

        foreach(List<Tile> list in roadBlocks)
        {
            foreach(Tile tile in list)
            {
                if (tile.zone != ZoneType.Poor)
                {
                    tile.plaza = true;
                }
            }
        }
    }

    private void ClusterDetection(int maxClusterSize)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        List<List<(int, int)>> clusters = new List<List<(int, int)>>();
        int initMaxClusterSize = maxClusterSize;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (!grid[r, c].clusterChecked && !grid[r,c].IsRoad)
                {
                    List<(int, int)> cluster = new List<(int, int)>();
                    DFS(grid, r, c, cluster, maxClusterSize);

                    // Si se excede el tama�o, recortar y liberar celdas sobrantes
                    if (cluster.Count > maxClusterSize)
                    {
                        for (int i = maxClusterSize; i < cluster.Count; i++)
                            grid[cluster[i].Item1, cluster[i].Item2].clusterChecked = false;

                        cluster.RemoveRange(maxClusterSize, cluster.Count - maxClusterSize);
                        maxClusterSize = initMaxClusterSize + UnityEngine.Random.Range(-2, 2);
                    }

                    clusters.Add(cluster);
                }
            }
        }

        foreach (var cluster in clusters)
        {
            if (cluster.Count >= 2 || (cluster.Count >= 1 && UnityEngine.Random.Range(0, 3) <= 1)) // solo clusters grandes
            {
                KingdomController.KingdomType chosenType = KingdomController.KingdomType.None;
                int chooseVariant=0;
                if(grid[cluster[0].Item1, cluster[0].Item2].zone==ZoneType.Poor)
                {
                    switch (UnityEngine.Random.Range(0,10))
                    {
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            chosenType = kingdomController.type;
                            break;
                        case 8:
                        case 9:
                            chosenType = kingdomController.secondaryType;
                            break;
                    }
                    int r = UnityEngine.Random.Range(0, 10);
                    if (r <= 6)
                    {
                        chooseVariant = 0;
                    }
                    else if(r <= 7)
                    {
                        chooseVariant = 1;
                    }
                    else
                    {
                        chooseVariant = 2;
                    }
                }
                else if (grid[cluster[0].Item1, cluster[0].Item2].zone == ZoneType.Middle)
                {
                    switch (UnityEngine.Random.Range(0, 10))
                    {
                        case 3:
                        case 4:
                        case 5:
                            chosenType = kingdomController.type;
                            break;
                        case 8:
                        case 9:
                            chosenType = kingdomController.secondaryType;
                            break;
                    }
                    int r = UnityEngine.Random.Range(0, 10);
                    if (r <= 1)
                    {
                        chooseVariant = 0;
                    }
                    else if (r <= 6)
                    {
                        chooseVariant = 1;
                    }
                    else
                    {
                        chooseVariant = 2;
                    }
                }
                else if(grid[cluster[0].Item1, cluster[0].Item2].zone == ZoneType.Rich && cluster.Count <= maxClusterSize * 0.75f)
                {
                    switch (UnityEngine.Random.Range(0, 10))
                    {
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            chosenType = kingdomController.type;
                            break;
                        case 6:
                        case 7:
                        case 8:
                        case 9:
                            chosenType = kingdomController.secondaryType;
                            break;
                    }
                    int r = UnityEngine.Random.Range(0, 10);
                    if (r <= 1)
                    {
                        chooseVariant = 1;
                    }
                    else
                    {
                        chooseVariant = 2;
                    }
                }
                foreach (var (r, c) in cluster)
                {
                    grid[r, c].type = chosenType;
                    grid[r, c].typeVariant = chooseVariant;
                }
            }
        }
    }

    static void DFS(
        Tile[,] grid,
        int r,
        int c,
        List<(int, int)> cluster,
        int maxSize)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        if (r < 0 || c < 0 || r >= rows || c >= cols)
            return;
        if (grid[r, c].clusterChecked || grid[r, c].IsRoad)
            return;
        if (cluster.Count >= maxSize)
            return;

        grid[r, c].clusterChecked = true;
        cluster.Add((r, c));

        DFS(grid, r - 1, c, cluster, maxSize);
        DFS(grid, r + 1, c, cluster, maxSize);
        DFS(grid, r, c - 1, cluster, maxSize);
        DFS(grid, r, c + 1, cluster, maxSize);
    }

    private void ApplyMask(int maxDepth)
    {
        int aux = 0;
        //Arriba (Derecha a izquierda)
        aux = UnityEngine.Random.Range((int)(maxDepth * 0.33f), (int)(maxDepth * 0.66f));
        for (int x = 0; x < width; x++)
        {
            for(int i=0; i<= aux;i++)
            {
                grid[x, height - 1 - i].empty = true;
            }
            if (x % 2 == 0 || UnityEngine.Random.Range(0, 2)==1)
            {
                switch (UnityEngine.Random.Range(0, 5))
                {
                    case 0:
                    case 1:
                        if (aux!=0)
                        {
                            aux--;
                        }
                        else
                        {
                            aux++;
                        }
                        break;
                    case 3:
                    case 4:
                        if (aux != maxDepth)
                        {
                            aux++;
                        }
                        else
                        {
                            aux--;
                        }
                        break;
                }
            }
        }
        //Abajo (Derecha a izquierda)
        aux = UnityEngine.Random.Range((int)(maxDepth * 0.33f), (int)(maxDepth * 0.66f));
        for (int x = 0; x < width; x++)
        {
            for (int i = 0; i <= aux; i++)
            {
                grid[x, i].empty = true;
            }
            if (x % 2 == 0 || UnityEngine.Random.Range(0, 2) == 1)
            {
                switch (UnityEngine.Random.Range(0, 5))
                {
                    case 0:
                    case 1:
                        if (aux != 0)
                        {
                            aux--;
                        }
                        else
                        {
                            aux++;
                        }
                        break;
                    case 3:
                    case 4:
                        if (aux != maxDepth)
                        {
                            aux++;
                        }
                        else
                        {
                            aux--;
                        }
                        break;
                }
            }
        }
        //Izquierda (Abajo a arriba)
        aux = UnityEngine.Random.Range((int)(maxDepth * 0.33f), (int)(maxDepth * 0.66f));
        for (int y = 0; y < width; y++)
        {
            for (int i = 0; i <= aux; i++)
            {
                grid[i, y].empty = true;
            }
            if (y % 2 == 0 || UnityEngine.Random.Range(0, 2) == 1)
            {
                switch (UnityEngine.Random.Range(0, 5))
                {
                    case 0:
                    case 1:
                        if (aux != 0)
                        {
                            aux--;
                        }
                        else
                        {
                            aux++;
                        }
                        break;
                    case 3:
                    case 4:
                        if (aux != maxDepth)
                        {
                            aux++;
                        }
                        else
                        {
                            aux--;
                        }
                        break;
                }
            }
        }
        //Derecha (Abajo a arriba)
        aux = UnityEngine.Random.Range((int)(maxDepth * 0.33f), (int)(maxDepth * 0.66f));
        for (int y = 0; y < width; y++)
        {
            for (int i = 0; i <= aux; i++)
            {
                grid[width - 1 - i, y].empty = true;
            }
            if (y % 2 == 0 || UnityEngine.Random.Range(0, 2) == 1)
            {
                switch (UnityEngine.Random.Range(0, 5))
                {
                    case 0:
                    case 1:
                        if (aux != 0)
                        {
                            aux--;
                        }
                        else
                        {
                            aux++;
                        }
                        break;
                    case 3:
                    case 4:
                        if (aux != maxDepth)
                        {
                            aux++;
                        }
                        else
                        {
                            aux--;
                        }
                        break;
                }
            }
        }
    }

    private List<Tile> GetRoadBlock(int startX, int startY, int size)
    {
        List<Tile> block = new List<Tile>();

        for (int dx = 0; dx < size; dx++)
        {
            for (int dy = 0; dy < size; dy++)
            {
                Tile tile = grid[startX + dx, startY + dy];
                if (!tile.IsRoad || tile.zone == ZoneType.Poor)
                {
                    return null; // Si alguna casilla no es camino, no es un bloque v�lido
                }
                block.Add(tile);
            }
        }

        return block;
    }

    void InstantiateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y].empty) continue;
                int connectValue=0;
                bool U = false, UR = false, R = false, DR = false, D = false, DL = false, L = false, UL = false;
                GameObject obj=null;
                if (grid[x, y].IsRoad)
                {
                    if (grid[x, y].plaza)
                    {
                        obj = plazaPrefab;
                    }
                    else if(grid[x, y].zone == ZoneType.Poor)
                    {
                        obj = poorRoadPrefab;
                    }
                    else if(grid[x, y].zone == ZoneType.Middle )
                    {
                        if(UnityEngine.Random.Range(0, 1f) < 0.85)
                        {
                            obj = roadPrefab;
                        }
                        else
                        {
                            obj = poorRoadPrefab;
                        }
                    }
                    else
                    {
                        obj = roadPrefab;
                    }

                    if (y < height - 1 && grid[x, y + 1].IsRoad)
                    {
                        connectValue += 1;
                        U = true;
                        if (x < width - 1 && grid[x + 1, y + 1].IsRoad)
                        {
                            UR = true;
                        }
                        if (x > 0 && grid[x - 1, y + 1].IsRoad)
                        {
                            UL = true;
                        }
                    }
                    if (x < width - 1 && grid[x+1,y].IsRoad)
                    {
                        connectValue += 2;
                        R = true;
                    }
                    if (y > 0 && grid[x, y - 1].IsRoad)
                    {
                        connectValue += 4;
                        D = true;
                        if (x < width - 1 && grid[x + 1, y - 1].IsRoad)
                        {
                            DR = true;
                        }
                        if (x > 0 && grid[x - 1, y - 1].IsRoad)
                        {
                            DL = true;
                        }
                    }
                    if (x > 0 && grid[x - 1, y].IsRoad)
                    {
                        connectValue += 8;
                        L = true;
                    }
                }
                else
                {
                    if(grid[x, y].tall)
                    {
                        obj = tallEmptyPrefab;

                        if (y < height - 1 && grid[x, y + 1].tall)
                        {
                            connectValue += 1;
                            U = true;
                            if (x < width - 1 && grid[x + 1, y + 1].tall)
                            {
                                UR = true;
                            }
                            if (x > 0 && grid[x - 1, y + 1].tall)
                            {
                                UL = true;
                            }
                        }
                        if (x < width - 1 && grid[x + 1, y].tall)
                        {
                            connectValue += 2;
                            R = true;
                        }
                        if (y > 0 && grid[x, y - 1].tall)
                        {
                            connectValue += 4;
                            D = true;
                            if (x < width - 1 && grid[x + 1, y - 1].tall)
                            {
                                DR = true;
                            }
                            if (x > 0 && grid[x - 1, y - 1].tall)
                            {
                                DL = true;
                            }
                        }
                        if (x > 0 && grid[x - 1, y].tall)
                        {
                            connectValue += 8;
                            L = true;
                        }
                    }
                    else
                    {
                        if (grid[x, y].typeVariant == 0)
                        {
                            obj = buildingPrefabs[(int)grid[x, y].type];
                        }
                        else if(grid[x, y].typeVariant==1)
                        {
                            obj = buildingPrefabsVariants[(int)grid[x, y].type];
                        }
                        else if (grid[x, y].typeVariant == 2)
                        {
                            obj = buildingPrefabsVariants2[(int)grid[x, y].type];
                        }

                        if (y < height - 1 && !grid[x, y + 1].IsRoad && grid[x, y].typeVariant == grid[x, y + 1].typeVariant && grid[x, y].type == grid[x, y + 1].type && !grid[x, y+1].empty)
                        {
                            connectValue += 1;
                            U = true;
                            if (x < width - 1 && !grid[x + 1, y + 1].IsRoad && grid[x, y].typeVariant== grid[x + 1, y + 1].typeVariant && grid[x, y].type == grid[x + 1, y + 1].type && !grid[x+1, y+1].empty)
                            {
                                UR = true;
                            }
                            if (x > 0 && !grid[x - 1, y + 1].IsRoad && grid[x, y].typeVariant == grid[x - 1, y + 1].typeVariant && grid[x, y].type == grid[x - 1, y + 1].type && !grid[x-1, y+1].empty)
                            {
                                UL = true;
                            }
                        }
                        if (x < width - 1 && !grid[x + 1, y].IsRoad && grid[x, y].typeVariant == grid[x + 1, y].typeVariant && grid[x, y].type == grid[x + 1, y].type && !grid[x+1, y].empty)
                        {
                            connectValue += 2;
                            R = true;
                        }
                        if (y > 0 && !grid[x, y - 1].IsRoad && grid[x, y].typeVariant == grid[x, y - 1].typeVariant && grid[x, y].type == grid[x, y - 1].type && !grid[x, y-1].empty)
                        {
                            connectValue += 4;
                            D = true;
                            if (x < width - 1 && !grid[x + 1, y - 1].IsRoad && grid[x, y].typeVariant == grid[x + 1, y - 1].typeVariant && grid[x, y].type == grid[x + 1, y - 1].type && !grid[x+1, y-1].empty)
                            {
                                DR = true;
                            }
                            if (x > 0 && !grid[x - 1, y - 1].IsRoad && grid[x, y].typeVariant == grid[x - 1, y - 1].typeVariant && grid[x, y].type == grid[x - 1, y - 1].type && !grid[x-1, y-1].empty)
                            {
                                DL = true;
                            }
                        }
                        if (x > 0 && !grid[x - 1, y].IsRoad && grid[x, y].typeVariant == grid[x - 1, y].typeVariant && grid[x, y].type == grid[x - 1, y].type && !grid[x-1, y].empty)
                        {
                            connectValue += 8;
                            L = true;
                        }
                    }
                }
                float tileHeight = 0;
                GameObject aux =Instantiate(obj,transform.position + new Vector3(x - width / 2, tileHeight, y - height / 2),Quaternion.identity);
                if (aux.GetComponent<SpriteBitmaskConexion>() != null)
                {
                    aux.GetComponent<SpriteBitmaskConexion>().Set(connectValue);
                    aux.GetComponent<SpriteBitmaskConexion>().tile = new Vector2(x, y);
                }
                if (aux.GetComponent<ModelBitmaskConexion>() != null)
                {
                    aux.GetComponent<ModelBitmaskConexion>().Set(connectValue);
                    aux.GetComponent<ModelBitmaskConexion>().tile = new Vector2(x, y);
                }
                if (aux.GetComponent<SegmentedModelBitmaskConexion>() != null)
                {
                    aux.GetComponent<SegmentedModelBitmaskConexion>().Set(U,UR,R,DR,D,DL,L,UL);
                    aux.GetComponent<SegmentedModelBitmaskConexion>().tile = new Vector2(x, y);
                }
                objects.Add(aux);
                aux.transform.SetParent(transform);
            }
        }
    }

    public void AddaptHeights()
    {
        foreach (GameObject tile in objects)
        {
            Vector3 origenRaycast = new Vector3(
             tile.transform.position.x,
             tile.transform.position.y + 50,
             tile.transform.position.z
            );

            RaycastHit hit;

            if (Physics.Raycast(origenRaycast, Vector3.down, out hit, Mathf.Infinity))
            {
                Vector3 nuevaPos = tile.transform.position;
                if (hit.point.y <= 0.6f)
                {
                    Destroy(tile.gameObject);
                    continue;
                }
                nuevaPos.y = hit.point.y-0.05f;
                tile.transform.position = nuevaPos;
            }
            else
            {
                Destroy(tile.gameObject);
            }
        }
        objects.RemoveAll(tile => tile == null);
    }

    List<Tile> GetNeighbors(Tile tile, bool includeDiagonals=false)
    {
        List<Tile> neighbors = new List<Tile>();

        // Vecinos ortogonales (arriba, abajo, izquierda, derecha)
        if (tile.x > 0) neighbors.Add(grid[tile.x - 1, tile.y]); // Izquierda
        if (tile.x < width - 1) neighbors.Add(grid[tile.x + 1, tile.y]); // Derecha
        if (tile.y > 0) neighbors.Add(grid[tile.x, tile.y - 1]); // Abajo
        if (tile.y < height - 1) neighbors.Add(grid[tile.x, tile.y + 1]); // Arriba

        // Vecinos diagonales
        if (includeDiagonals)
        {
            if (tile.x > 0 && tile.y > 0) neighbors.Add(grid[tile.x - 1, tile.y - 1]); // Izq-Abajo
            if (tile.x > 0 && tile.y < height - 1) neighbors.Add(grid[tile.x - 1, tile.y + 1]); // Izq-Arriba
            if (tile.x < width - 1 && tile.y > 0) neighbors.Add(grid[tile.x + 1, tile.y - 1]); // Der-Abajo
            if (tile.x < width - 1 && tile.y < height - 1) neighbors.Add(grid[tile.x + 1, tile.y + 1]); // Der-Arriba
        }

        return neighbors;
    }

    private ZoneType GetZoneType(int x, int y)
    {
        // Calcular la distancia al centro del mapa
        int centerX = width / 2;
        int centerY = height / 2;
        float distance = Mathf.Sqrt(Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2));
        float richPercentage=0.2f;
        float middlePercentage=0.35f;
        switch (kingdomController.size)
        {
            case KingdomController.Size.Peque�o:
                richPercentage = 0.3f;
                break;
            case KingdomController.Size.Mediano:
                richPercentage = 0.225f;
                middlePercentage = 0.35f;
                break;
            case KingdomController.Size.Grande:
                richPercentage = 0.25f;
                middlePercentage = 0.4f;
                break;
        }

        // Definir radios para cada zona
        float richRadius = Mathf.Min(width, height) * richPercentage;  // 20% del mapa m�s cercano al centro
        float middleRadius = Mathf.Min(width, height) * middlePercentage; // 40% del mapa rodeando el centro
        ZoneType aux;
        if (distance < richRadius)
        {
            aux = ZoneType.Rich;
        }
        else if (distance < middleRadius)
        {
            aux = ZoneType.Middle;
        }
        else
        {
            aux = ZoneType.Poor;
        }
        if (kingdomController.size== KingdomController.Size.Peque�o)
        {
            if (aux == ZoneType.Rich)
            {
                aux = ZoneType.Middle;
            }
            else if (aux == ZoneType.Middle)
            {
                aux = ZoneType.Poor;
            }
        }
        return aux;
    }
}

public class Tile
{
    public int x, y;
    public bool IsCollapsed;
    public bool IsRoad;
    public List<bool> PossibleStates; // true = camino, false = vac�o
    public CityGenerator.ZoneType zone;
    public bool empty;
    public bool tall;
    public bool plaza;
    public bool clusterChecked;
    public KingdomController.KingdomType type=KingdomController.KingdomType.None;
    public int typeVariant;

    public Tile(int x, int y, CityGenerator.ZoneType zone)
    {
        this.x = x;
        this.y = y;
        this.zone = zone;
        IsCollapsed = false;

        PossibleStates = new List<bool> { true, false };
    }

    public void Collapse(List<Tile> neighbors)
    {
        if (PossibleStates.Count > 0)
        {
            int max=3;
            // Asegurar que haya entre 2 y 3 conexiones
            int roadNeighbors = 0;
            foreach (Tile neighbor in neighbors)
            {
                if (neighbor.IsRoad) roadNeighbors++;
            }
            switch (zone)
            {
                case CityGenerator.ZoneType.Poor:
                    max = 5;
                    break;
                case CityGenerator.ZoneType.Middle:
                    max = 3;
                    break;
                case CityGenerator.ZoneType.Rich:
                    max = 3;
                    break;
            }
            if (roadNeighbors > max)
            {
                IsRoad = false; // Evitar islas de caminos
            }
            else
            {
                if(zone != CityGenerator.ZoneType.Poor && roadNeighbors == max)
                {
                    if(zone == CityGenerator.ZoneType.Middle && UnityEngine.Random.value<0.1)
                    {
                        IsRoad = false;
                    }
                    else if (zone == CityGenerator.ZoneType.Rich && UnityEngine.Random.value < 0.2)
                    {
                        IsRoad = false;
                    }
                    else
                    {
                        IsRoad = true;
                    }
                }
                else
                {
                    IsRoad = true;
                }
            }
            IsCollapsed = true;
            PossibleStates.Clear();
        }
    }

    public void UpdatePossibleStates(Tile[,] grid)
    {
        List<bool> newPossibleStates = new List<bool>();

        foreach (bool state in PossibleStates)
        {
            if (IsValidState(state, grid))
            {
                newPossibleStates.Add(state);
            }
        }

        PossibleStates = newPossibleStates;

        if (PossibleStates.Count == 1)
        {
            Collapse(GetNeighbors(grid));
        }
    }

    private bool IsValidState(bool state, Tile[,] grid)
    {
        // Verifica reglas de conectividad con vecinos
        int roadNeighbors = 0;
        int emptyNeighbors = 0;

        foreach (var neighbor in GetNeighbors(grid,true))
        {
            if (neighbor.IsRoad) roadNeighbors++;
            else emptyNeighbors++;
        }

        // Reglas seg�n la zona
        if (zone == CityGenerator.ZoneType.Poor) return roadNeighbors >= 1;
        if (zone == CityGenerator.ZoneType.Middle) return roadNeighbors >= 1;
        if (zone == CityGenerator.ZoneType.Rich) return roadNeighbors >= 1 && emptyNeighbors>=5;

        return true;
    }

    private List<Tile> GetNeighbors(Tile[,] grid, bool includeDiagonals = false)
    {
        List<Tile> neighbors = new List<Tile>();

        if (x > 0) neighbors.Add(grid[x - 1, y]);
        if (x < grid.GetLength(0) - 1) neighbors.Add(grid[x + 1, y]);
        if (y > 0) neighbors.Add(grid[x, y - 1]);
        if (y < grid.GetLength(1) - 1) neighbors.Add(grid[x, y + 1]);

        if (includeDiagonals)
        {
            if (x > 0 && y > 0) neighbors.Add(grid[x - 1, y - 1]);
            if (x > 0 && y < grid.GetLength(1) - 1) neighbors.Add(grid[x - 1, y + 1]);
            if (x < grid.GetLength(0) - 1 && y > 0) neighbors.Add(grid[x + 1, y - 1]);
            if (x < grid.GetLength(0) - 1 && y < grid.GetLength(1) - 1) neighbors.Add(grid[x + 1, y + 1]);
        }

        return neighbors;
    }
}

