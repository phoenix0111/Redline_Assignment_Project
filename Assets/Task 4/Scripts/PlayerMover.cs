using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private PaperGrid grid;
    [SerializeField] private float timeBetweenSteps = 0.12f;

    private int cellX;
    private int cellY;
    private int moveX;
    private int moveY;
    private float stepTimer;

    private void Start()
    {
        if (grid == null)
            grid = FindFirstObjectByType<PaperGrid>();

        cellX = grid.StartCellX;
        cellY = grid.StartCellY;
        transform.position = grid.GetWorldPosition(cellX, cellY) + Vector3.up * 0.5f;
    }

    private void Update()
    {
        ReadHeldDirection();

        stepTimer += Time.deltaTime;
        if (stepTimer < timeBetweenSteps)
            return;

        if (moveX == 0 && moveY == 0)
            return;

        stepTimer = 0f;
        TryMove();
    }

    private void ReadHeldDirection()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            moveX = 0;
            moveY = 1;
        }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            moveX = 0;
            moveY = -1;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveX = -1;
            moveY = 0;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveX = 1;
            moveY = 0;
        }
    }

    private void TryMove()
    {
        int nextX = cellX + moveX;
        int nextY = cellY + moveY;

        if (!grid.IsInside(nextX, nextY))
            return;

        CellType nextCell = grid.GetCell(nextX, nextY);

        // Hitting your own open trail does nothing. Closing only happens
        // when you walk back onto land you already own.
        if (nextCell == CellType.Trail)
            return;

        if (nextCell == CellType.Empty)
            grid.PaintTrail(nextX, nextY);

        if (nextCell == CellType.Owned && grid.HasOpenTrail())
            grid.CloseTrail();

        cellX = nextX;
        cellY = nextY;
        transform.position = grid.GetWorldPosition(cellX, cellY) + Vector3.up * 0.5f;
    }
}
