using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Belt Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private Transform[] points;

    [Header("What comes after this belt (set only one)")]
    [SerializeField] private ConveyorBelt nextBelt;
    [SerializeField] private Junction nextJunction;
    [SerializeField] private Bin endBin;

    public float Speed => speed;
    public int PointCount => points.Length;
    public ConveyorBelt NextBelt => nextBelt;
    public Junction NextJunction => nextJunction;
    public Bin EndBin => endBin;

    public Transform GetPoint(int index)
    {
        return points[index];
    }
}
