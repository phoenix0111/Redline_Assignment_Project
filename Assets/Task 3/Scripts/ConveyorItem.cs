using UnityEngine;

public class ConveyorItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private ItemColor itemColor;

    [Header("Only for items placed in the scene by hand")]
    [SerializeField] private ConveyorBelt startBelt;
    [SerializeField] private int firstPointToMoveTo = 1;

    private ConveyorBelt currentBelt;
    private int targetPointIndex;

    public ItemColor ItemColor => itemColor;

    private void Start()
    {
        // Items placed in the scene start moving on their chosen belt.
        // Items made by the spawner are already on a belt, so this is skipped.
        if (currentBelt == null && startBelt != null)
        {
            currentBelt = startBelt;
            targetPointIndex = firstPointToMoveTo;
        }
    }

    // Used by the spawner: put the item at the start of a belt.
    public void PlaceOnBelt(ConveyorBelt belt)
    {
        transform.position = belt.GetPoint(0).position;
        SetBelt(belt);
    }

    private void SetBelt(ConveyorBelt belt)
    {
        currentBelt = belt;
        targetPointIndex = 0;
    }

    private void Update()
    {
        if (currentBelt == null)
            return;

        Vector3 targetPosition = currentBelt.GetPoint(targetPointIndex).position;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, currentBelt.Speed * Time.deltaTime);

        // Reached this point: aim for the next one.
        if (transform.position == targetPosition)
        {
            targetPointIndex++;

            if (targetPointIndex >= currentBelt.PointCount)
                GoToWhatComesNext();
        }
    }

    private void GoToWhatComesNext()
    {
        if (currentBelt.NextBelt != null)
        {
            SetBelt(currentBelt.NextBelt);
        }
        else if (currentBelt.NextJunction != null)
        {
            SetBelt(currentBelt.NextJunction.GetActiveBelt());
        }
        else if (currentBelt.EndBin != null)
        {
            currentBelt.EndBin.Receive(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
