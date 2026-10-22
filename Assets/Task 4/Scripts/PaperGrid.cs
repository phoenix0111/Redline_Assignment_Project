using System.Collections.Generic;
using UnityEngine;

public class PaperGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 21;
    [SerializeField] private int height = 21;
    [SerializeField] private int startLandSize = 3;
    [SerializeField] private float cellSize = 1f;

    [Header("Colors")]
    [SerializeField] private Color emptyColor = new Color(0.2f, 0.22f, 0.26f);
    [SerializeField] private Color ownedColor = new Color(0.2f, 0.65f, 0.95f);
    [SerializeField] private Color trailColor = new Color(0.95f, 0.85f, 0.25f);

    private CellType[,] cells;
    private Renderer[,] cellRenderers;
    private Material emptyMaterial;
    private Material ownedMaterial;
    private Material trailMaterial;
    private int trailCount;

    public int Width => width;
    public int Height => height;
    public int StartCellX => width / 2;
    public int StartCellY => height / 2;

    private void Start()
    {
        CreateSharedMaterials();
        CreateGrid();
        MakeStartingLand();
        PlaceCamera();
    }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    public CellType GetCell(int x, int y)
    {
        return cells[x, y];
    }

    public bool HasOpenTrail()
    {
        return trailCount > 0;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        float worldX = (x - (width - 1) / 2f) * cellSize;
        float worldZ = (y - (height - 1) / 2f) * cellSize;
        return new Vector3(worldX, 0f, worldZ);
    }

    // Leave a yellow trail cell behind the player.
    public void PaintTrail(int x, int y)
    {
        if (cells[x, y] != CellType.Empty)
            return;

        SetCell(x, y, CellType.Trail);
        trailCount++;
    }

    // Player walked back onto owned land, so turn the trail into land
    // and fill every empty pocket that is fully closed off.
    public void CloseTrail()
    {
        TurnTrailIntoOwnedLand();
        FillEnclosedEmptyCells();
    }

    private void TurnTrailIntoOwnedLand()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (cells[x, y] == CellType.Trail)
                    SetCell(x, y, CellType.Owned);
            }
        }

        trailCount = 0;
    }

    // Start from empty cells on the edge of the board and walk inward.
    // Anything empty we cannot reach is trapped inside, so it becomes owned.
    private void FillEnclosedEmptyCells()
    {
        bool[,] canReachEdge = new bool[width, height];
        Queue<Vector2Int> cellsToCheck = new Queue<Vector2Int>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isOnEdge = x == 0 || y == 0 || x == width - 1 || y == height - 1;

                if (isOnEdge && cells[x, y] == CellType.Empty)
                {
                    canReachEdge[x, y] = true;
                    cellsToCheck.Enqueue(new Vector2Int(x, y));
                }
            }
        }

        int[] stepX = { 0, 0, -1, 1 };
        int[] stepY = { -1, 1, 0, 0 };

        while (cellsToCheck.Count > 0)
        {
            Vector2Int current = cellsToCheck.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int nextX = current.x + stepX[i];
                int nextY = current.y + stepY[i];

                if (!IsInside(nextX, nextY))
                    continue;

                if (canReachEdge[nextX, nextY])
                    continue;

                if (cells[nextX, nextY] != CellType.Empty)
                    continue;

                canReachEdge[nextX, nextY] = true;
                cellsToCheck.Enqueue(new Vector2Int(nextX, nextY));
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (cells[x, y] == CellType.Empty && canReachEdge[x, y] == false)
                    SetCell(x, y, CellType.Owned);
            }
        }
    }

    private void SetCell(int x, int y, CellType type)
    {
        cells[x, y] = type;
        cellRenderers[x, y].sharedMaterial = GetMaterial(type);
    }

    // One material for each color, shared by every tile of that type.
    // Using .material would copy a new material for every tile and slow the game down.
    private Material GetMaterial(CellType type)
    {
        if (type == CellType.Owned)
            return ownedMaterial;
        if (type == CellType.Trail)
            return trailMaterial;
        return emptyMaterial;
    }

    private void CreateSharedMaterials()
    {
        emptyMaterial = MakeMaterial(emptyColor);
        ownedMaterial = MakeMaterial(ownedColor);
        trailMaterial = MakeMaterial(trailColor);
    }

    private Material MakeMaterial(Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        material.enableInstancing = true;
        return material;
    }

    private void CreateGrid()
    {
        cells = new CellType[width, height];
        cellRenderers = new Renderer[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = "Cell " + x + "," + y;
                tile.transform.SetParent(transform, false);
                tile.transform.position = GetWorldPosition(x, y);
                tile.transform.localScale = new Vector3(cellSize * 0.95f, 0.2f, cellSize * 0.95f);

                Collider tileCollider = tile.GetComponent<Collider>();
                if (tileCollider != null)
                    tileCollider.enabled = false;

                cells[x, y] = CellType.Empty;
                cellRenderers[x, y] = tile.GetComponent<Renderer>();
                cellRenderers[x, y].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                cellRenderers[x, y].receiveShadows = false;
                cellRenderers[x, y].sharedMaterial = emptyMaterial;
            }
        }
    }

    private void MakeStartingLand()
    {
        int centerX = StartCellX;
        int centerY = StartCellY;
        int half = startLandSize / 2;

        for (int y = centerY - half; y <= centerY + half; y++)
        {
            for (int x = centerX - half; x <= centerX + half; x++)
            {
                if (IsInside(x, y))
                    SetCell(x, y, CellType.Owned);
            }
        }
    }

    private void PlaceCamera()
    {
        float size = Mathf.Max(width, height) * cellSize;
        Transform cameraTransform = Camera.main.transform;
        cameraTransform.position = new Vector3(0f, size * 0.85f, -size * 0.7f);
        cameraTransform.LookAt(Vector3.zero);
    }
}
