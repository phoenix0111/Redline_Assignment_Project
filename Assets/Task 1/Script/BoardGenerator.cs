
using UnityEngine;
using System.Collections.Generic;

public class BoardGenerator : MonoBehaviour
{
    [Header("Capsule Prefabs")]
    public GameObject redCapsulePrefab;
    public GameObject blueCapsulePrefab;
    public GameObject yellowCapsulePrefab;
    public GameObject greenCapsulePrefab;

    [Header("Hole Prefabs")]
    public GameObject redHolePrefab;
    public GameObject blueHolePrefab;
    public GameObject yellowHolePrefab;
    public GameObject greenHolePrefab;

    [Header("Generation Settings")]
    public GridManager gridManager;
    public int capsulesPerColor = 10;
    public int holeEmptySpace = 1;   // how many empty cells to keep around each hole
    [Range(0f, 1f)]
    public float emptyCellChance = 0.1f;   // chance that a cell inside a group is left empty
    public float capsuleHeight = 0.5f;
    public float holeHeight = 0.15f;

    private bool[,] occupied;

    private void Start()
    {
        if (gridManager == null)
        {
            Debug.LogError("Assign GridManager in the Inspector.");
            return;
        }

        GenerateBoard();

        // Register objects only after generating the board.
        gridManager.RegisterSceneObjects();
    }

    private void GenerateBoard()
    {
        int width = gridManager.width;
        int height = gridManager.height;

        if (width <= 0 || height <= 0 ||
            capsulesPerColor < 0 ||
            4L * capsulesPerColor + 4 > (long)width * height)
        {
            Debug.LogError("Invalid board generation settings.");
            return;
        }

        GameObject[] capsulePrefabs =
        {
            redCapsulePrefab,
            blueCapsulePrefab,
            yellowCapsulePrefab,
            greenCapsulePrefab
        };

        GameObject[] holePrefabs =
        {
            redHolePrefab,
            blueHolePrefab,
            yellowHolePrefab,
            greenHolePrefab
        };

        CharacterColor[] colors =
        {
            CharacterColor.Red,
            CharacterColor.Blue,
            CharacterColor.Yellow,
            CharacterColor.Green
        };

        string[] groupNames =
        {
            "RedGroup", "BlueGroup", "YellowGroup", "GreenGroup"
        };

        for (int i = 0; i < 4; i++)
        {
            if (capsulePrefabs[i] == null || holePrefabs[i] == null)
            {
                Debug.LogError("Assign all eight prefabs in the Inspector.");
                return;
            }
        }

        occupied = new bool[width, height];

        Transform[] groups = new Transform[4];

        for (int i = 0; i < 4; i++)
            groups[i] = CreateGroup(groupNames[i]);

        Transform holesGroup = CreateGroup("Holes");

        // Create one hole for each color.
        for (int i = 0; i < 4; i++)
        {
            Vector2Int cell = GetRandomFreeEdgeCell();

            if (cell.x < 0)
                return;

            occupied[cell.x, cell.y] = true;
            KeepSpaceAroundHole(cell);
            CreateHole(holePrefabs[i], colors[i], cell, holesGroup);
        }

        // Create each color's capsule cluster.
        for (int i = 0; i < 4; i++)
        {
            List<Vector2Int> cells = GetClusterCells(capsulesPerColor);

            if (cells.Count != capsulesPerColor)
            {
                Debug.LogError("Could not place all capsules.");
                return;
            }

            for (int j = 0; j < cells.Count; j++)
            {
                // Randomly leave some cells of the group empty, so groups are not solid blocks.
                if (Random.value < emptyCellChance)
                    continue;

                CreateCapsule(
                    capsulePrefabs[i],
                    colors[i],
                    cells[j],
                    groups[i],
                    j + 1);
            }
        }
    }

    private Transform CreateGroup(string groupName)
    {
        GameObject group = new GameObject(groupName);
        group.transform.SetParent(transform, false);
        return group.transform;
    }

    private Vector2Int GetRandomFreeCell()
    {
        List<Vector2Int> freeCells = new List<Vector2Int>();

        for (int y = 0; y < gridManager.height; y++)
        {
            for (int x = 0; x < gridManager.width; x++)
            {
                if (!occupied[x, y])
                    freeCells.Add(new Vector2Int(x, y));
            }
        }

        if (freeCells.Count == 0)
            return new Vector2Int(-1, -1);

        return freeCells[Random.Range(0, freeCells.Count)];
    }

    // Same as GetRandomFreeCell, but only picks cells on the outer border of the board.
    private Vector2Int GetRandomFreeEdgeCell()
    {
        List<Vector2Int> edgeCells = new List<Vector2Int>();

        for (int y = 0; y < gridManager.height; y++)
        {
            for (int x = 0; x < gridManager.width; x++)
            {
                bool isOnEdge = x == 0 || y == 0 ||
                                x == gridManager.width - 1 ||
                                y == gridManager.height - 1;

                if (isOnEdge && !occupied[x, y])
                    edgeCells.Add(new Vector2Int(x, y));
            }
        }

        if (edgeCells.Count == 0)
            return new Vector2Int(-1, -1);

        return edgeCells[Random.Range(0, edgeCells.Count)];
    }

    // Marks the cells around a hole as taken, so no capsule spawns there.
    private void KeepSpaceAroundHole(Vector2Int holeCell)
    {
        for (int y = holeCell.y - holeEmptySpace; y <= holeCell.y + holeEmptySpace; y++)
        {
            for (int x = holeCell.x - holeEmptySpace; x <= holeCell.x + holeEmptySpace; x++)
            {
                bool isInsideBoard = x >= 0 && y >= 0 &&
                                     x < gridManager.width &&
                                     y < gridManager.height;

                if (isInsideBoard)
                    occupied[x, y] = true;
            }
        }
    }

    private List<Vector2Int> GetClusterCells(int count)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        if (count == 0)
            return cells;

        Vector2Int firstCell = GetRandomFreeCell();

        if (firstCell.x < 0)
            return cells;

        AddCell(firstCell, cells);

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (cells.Count < count)
        {
            List<Vector2Int> availableNeighbors =
                new List<Vector2Int>();

            foreach (Vector2Int cell in cells)
            {
                foreach (Vector2Int direction in directions)
                {
                    Vector2Int next = cell + direction;

                    if (next.x < 0 || next.x >= gridManager.width ||
                        next.y < 0 || next.y >= gridManager.height)
                        continue;

                    if (occupied[next.x, next.y] ||
                        availableNeighbors.Contains(next))
                        continue;

                    availableNeighbors.Add(next);
                }
            }

            Vector2Int chosenCell;

            if (availableNeighbors.Count > 0)
            {
                chosenCell = availableNeighbors[
                    Random.Range(0, availableNeighbors.Count)];
            }
            else
            {
                // If enclosed, start another small section.
                chosenCell = GetRandomFreeCell();

                if (chosenCell.x < 0)
                    break;
            }

            AddCell(chosenCell, cells);
        }

        return cells;
    }

    private void AddCell(Vector2Int cell, List<Vector2Int> cells)
    {
        occupied[cell.x, cell.y] = true;
        cells.Add(cell);
    }

    private Vector3 CellToWorld(Vector2Int cell, float objectHeight)
    {
        Transform origin = gridManager.boardOrigin;

        float offsetX =
            (gridManager.width - 1) * gridManager.cellSize / 2f;

        float offsetZ =
            (gridManager.height - 1) * gridManager.cellSize / 2f;

        Vector3 position = origin.position;

        position.x += cell.x * gridManager.cellSize - offsetX;
        position.z += cell.y * gridManager.cellSize - offsetZ;
        position.y += objectHeight;

        return position;
    }

    private void CreateCapsule(
        GameObject prefab,
        CharacterColor color,
        Vector2Int cell,
        Transform parent,
        int number)
    {
        GameObject capsule = Instantiate(
            prefab,
            CellToWorld(cell, capsuleHeight),
            Quaternion.identity,
            parent);

        capsule.name = color + "Capsule_" + number.ToString("00");

        Character character = capsule.GetComponent<Character>();

        if (character == null)
            character = capsule.AddComponent<Character>();

        character.characterColor = color;
        character.gridPosition = cell;
    }

    private void CreateHole(
        GameObject prefab,
        CharacterColor color,
        Vector2Int cell,
        Transform parent)
    {
        GameObject holeObject = Instantiate(
            prefab,
            CellToWorld(cell, holeHeight),
            Quaternion.identity,
            parent);

        holeObject.name = color + "Hole";

        Hole hole = holeObject.GetComponent<Hole>();

        if (hole == null)
            hole = holeObject.AddComponent<Hole>();

        hole.acceptedColor = color;
        hole.gridPosition = cell;
    }
}
