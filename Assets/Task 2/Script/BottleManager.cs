using System.Collections;
using UnityEngine;

public class BottleManager : MonoBehaviour
{
    public static BottleManager Instance;

    [Header("Pour Animation Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float tiltAngle = 60f;
    [SerializeField] private float tiltSpeed = 300f;
    [SerializeField] private float pourDelay = 0.15f;
    [SerializeField] private Vector3 pourOffset = new Vector3(1f, 2f, 0f);

    private BottleInput selectedBottle;
    private bool isPouring;

    private void Awake()
    {
        Instance = this;
    }

    public void OnBottleClicked(BottleInput clickedBottle)
    {
        if (isPouring)
            return;

        // First tap: select a bottle that has liquid.
        if (selectedBottle == null)
        {
            if (clickedBottle.Bottle.IsEmpty)
                return;

            selectedBottle = clickedBottle;
            selectedBottle.Select();
            return;
        }

        // Same bottle tapped again: put it back down.
        if (clickedBottle == selectedBottle)
        {
            selectedBottle.Deselect();
            selectedBottle = null;
            return;
        }

        // Second tap on another bottle: pour if allowed.
        BottleInput fromBottle = selectedBottle;
        selectedBottle = null;

        LiquidColor topColor = fromBottle.Bottle.GetTopColor();

        if (clickedBottle.Bottle.CanReceive(topColor))
            StartCoroutine(Pour(fromBottle, clickedBottle));
        else
            fromBottle.Deselect();
    }

    private IEnumerator Pour(BottleInput fromBottle, BottleInput toBottle)
    {
        isPouring = true;

        // Stand on the left or right of the target, depending on where we came from.
        float side = 1f;
        if (fromBottle.OriginalPosition.x < toBottle.OriginalPosition.x)
            side = -1f;

        Vector3 pourPosition = toBottle.OriginalPosition +
                               new Vector3(pourOffset.x * side, pourOffset.y, 0f);

        yield return MoveTo(fromBottle.transform, pourPosition);
        yield return RotateTo(fromBottle.transform, tiltAngle * side);

        // Pour as many same-colored segments as fit.
        int freeSpace = toBottle.Bottle.Capacity - toBottle.Bottle.LiquidCount;
        int amountToPour = Mathf.Min(fromBottle.Bottle.GetTopGroupCount(), freeSpace);

        for (int i = 0; i < amountToPour; i++)
        {
            LiquidColor color = fromBottle.Bottle.GetTopColor();

            fromBottle.Bottle.RemoveTopLiquid();
            toBottle.Bottle.AddLiquid(color);

            fromBottle.Bottle.RefreshVisuals();
            toBottle.Bottle.RefreshVisuals();

            yield return new WaitForSeconds(pourDelay);
        }

        yield return RotateTo(fromBottle.transform, 0f);
        yield return MoveTo(fromBottle.transform, fromBottle.OriginalPosition);

        isPouring = false;
    }

    private IEnumerator MoveTo(Transform target, Vector3 destination)
    {
        while (target.position != destination)
        {
            target.position = Vector3.MoveTowards(target.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator RotateTo(Transform target, float angle)
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        while (target.rotation != targetRotation)
        {
            target.rotation = Quaternion.RotateTowards(target.rotation, targetRotation, tiltSpeed * Time.deltaTime);
            yield return null;
        }
    }
}