using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid Settings")]
    public int width = 12;
    public int height = 12;
    public float cellSize = 1f;
    public Transform boardOrigin;

    private Character[,] characterGrid;
    private Hole[,] holeGrid;
    private List<Character> characters = new List<Character>();

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float enterHoleSpeed = 2f;

    private bool isProcessingHole;


    private Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    private void Awake()
    {
        Instance = this;

        if (boardOrigin == null)
            boardOrigin = transform;

        if (width <= 0 || height <= 0 || cellSize <= 0f)
        {
            Debug.LogError("Invalid grid settings.");
            enabled = false;
            return;
        }

        characterGrid = new Character[width, height];
        holeGrid = new Hole[width, height];
    }

    public void RegisterSceneObjects()
    {
        characters.Clear();
        characterGrid = new Character[width, height];
        holeGrid = new Hole[width, height];

        Character[] foundCharacters =
            FindObjectsByType<Character>(FindObjectsSortMode.None);

        Hole[] foundHoles =
            FindObjectsByType<Hole>(FindObjectsSortMode.None);

        foreach (Character character in foundCharacters)
        {
            Vector2Int cell = WorldToCell(character.transform.position);

            if (!IsInsideGrid(cell))
                continue;

            if (characterGrid[cell.x, cell.y] != null ||
                holeGrid[cell.x, cell.y] != null)
            {
                Debug.LogWarning("Object overlap at cell " + cell);
                continue;
            }

            character.gridPosition = cell;
            characterGrid[cell.x, cell.y] = character;
            characters.Add(character);
        }

        foreach (Hole hole in foundHoles)
        {
            Vector2Int cell = WorldToCell(hole.transform.position);

            if (!IsInsideGrid(cell))
                continue;

            if (holeGrid[cell.x, cell.y] != null ||
                characterGrid[cell.x, cell.y] != null)
            {
                Debug.LogWarning("Object overlap at cell " + cell);
                continue;
            }

            hole.gridPosition = cell;
            holeGrid[cell.x, cell.y] = hole;
        }

        Debug.Log("Grid ready. Characters: " + characters.Count);
    }

    private Vector2Int WorldToCell(Vector3 position)
    {
        Vector3 offset = position - boardOrigin.position;

        float offsetX = (width - 1) * cellSize / 2f;
        float offsetZ = (height - 1) * cellSize / 2f;

        return new Vector2Int(
            Mathf.RoundToInt((offset.x + offsetX) / cellSize),
            Mathf.RoundToInt((offset.z + offsetZ) / cellSize)
        );
    }

    private bool IsInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width &&
               cell.y >= 0 && cell.y < height;
    }

    public void OnHoleTapped(Hole targetHole)
    {
        if (isProcessingHole)
            return;

        if (targetHole == null)
            return;

        StartCoroutine(ProcessHole(targetHole));
    }

    private IEnumerator ProcessHole(Hole targetHole)
    {
        isProcessingHole = true;

        List<Character> charactersToMove = new List<Character>();
        List<List<Vector2Int>> paths = new List<List<Vector2Int>>();

        // Find all matching characters and their paths
        foreach (Character character in characters)
        {
            if (character == null)
                continue;

            if (character.characterColor != targetHole.acceptedColor)
                continue;

            List<Vector2Int> path = FindPath(character, targetHole);

            if (path != null)
            {
                charactersToMove.Add(character);
                paths.Add(path);
            }
        }

        // Start moving all characters together
        List<Coroutine> movements = new List<Coroutine>();

        for (int i = 0; i < charactersToMove.Count; i++)
        {
            Coroutine movement = StartCoroutine(
                MoveCharacterToHole(
                    charactersToMove[i],
                    paths[i],
                    targetHole
                )
            );

            movements.Add(movement);
        }

        // Wait until all characters have finished moving
        foreach (Coroutine movement in movements)
        {
            yield return movement;
        }

        // Remove characters after they have all reached the hole
        foreach (Character character in charactersToMove)
        {
            if (character == null)
                continue;

            characters.Remove(character);

            int x = character.gridPosition.x;
            int y = character.gridPosition.y;

            if (x >= 0 && x < width && y >= 0 && y < height)
                characterGrid[x, y] = null;

            Destroy(character.gameObject);
        }

        isProcessingHole = false;
    }

    private bool HasPath(
        Vector2Int start,
        Vector2Int target,
        CharacterColor movingColor)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == target)
                return true;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (!IsInsideGrid(next) || visited.Contains(next))
                    continue;

                Hole hole = holeGrid[next.x, next.y];

                if (hole != null && next != target)
                    continue;

                Character obstacle = characterGrid[next.x, next.y];

                if (obstacle != null &&
                    obstacle.characterColor != movingColor)
                    continue;

                visited.Add(next);
                queue.Enqueue(next);
            }
        }

        return false;

    }

    private List<Vector2Int> FindPath(
        Character character,
        Hole targetHole)
    {
        Vector2Int start = character.gridPosition;
        Vector2Int target = targetHole.gridPosition;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> previous =
            new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        previous[start] = start;

        Vector2Int[] directions =
        {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == target)
                break;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (next.x < 0 || next.x >= width ||
                    next.y < 0 || next.y >= height)
                    continue;

                if (previous.ContainsKey(next))
                    continue;

                Hole hole = holeGrid[next.x, next.y];

                if (hole != null && hole != targetHole)
                    continue;

                Character other = characterGrid[next.x, next.y];

                if (other != null &&
                    other != character &&
                    other.characterColor != character.characterColor)
                    continue;

                previous[next] = current;
                queue.Enqueue(next);
            }
        }

        if (!previous.ContainsKey(target))
            return null;

        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int position = target;

        while (position != start)
        {
            path.Add(position);
            position = previous[position];
        }

        path.Reverse();

        return path;
    }


    private IEnumerator MoveCharacterToHole(
    Character character,
    List<Vector2Int> path,
    Hole targetHole)
    {
        // Follow the grid path.
        foreach (Vector2Int cell in path)
        {
            Vector3 targetPosition = CellToWorld(cell);

            while (Vector3.Distance(
                character.transform.position,
                targetPosition) > 0.02f)
            {
                character.transform.position = Vector3.MoveTowards(
                    character.transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            character.transform.position = targetPosition;
        }

        // Reach the center of the hole.
        Vector3 holePosition = targetHole.transform.position;

        while (Vector3.Distance(
            character.transform.position,
            holePosition) > 0.02f)
        {
            character.transform.position = Vector3.MoveTowards(
                character.transform.position,
                holePosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Sink into the hole.
        Vector3 insideHolePosition = holePosition - Vector3.up * 0.5f;

        while (Vector3.Distance(
            character.transform.position,
            insideHolePosition) > 0.02f)
        {
            character.transform.position = Vector3.MoveTowards(
                character.transform.position,
                insideHolePosition,
                enterHoleSpeed * Time.deltaTime
            );

            yield return null;
        }
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        Vector3 origin = boardOrigin.position;

        float offsetX = (width - 1) * cellSize / 2f;
        float offsetZ = (height - 1) * cellSize / 2f;

        float x = cell.x * cellSize - offsetX;
        float z = cell.y * cellSize - offsetZ;

        return new Vector3(
            origin.x + x,
            origin.y + 0.5f,
            origin.z + z
        );
    }
}
