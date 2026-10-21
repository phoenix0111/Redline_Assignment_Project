using UnityEngine;

public class Bin : MonoBehaviour
{
    [Header("Bin Settings")]
    [SerializeField] private ItemColor binColor;

    public void Receive(ConveyorItem item)
    {
        if (item.ItemColor == binColor)
            Debug.Log("Correct: " + item.ItemColor + " item went into the " + binColor + " bin");
        else
            Debug.Log("Wrong: " + item.ItemColor + " item went into the " + binColor + " bin");

        Destroy(item.gameObject);
    }
}
