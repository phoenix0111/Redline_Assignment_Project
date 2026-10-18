using UnityEngine;

public class BottleInput : MonoBehaviour
{
    [SerializeField] private float liftHeight = 1.5f;

    private Vector3 originalPosition;
    private Bottle bottle;

    public Vector3 OriginalPosition => originalPosition;
    public Bottle Bottle => bottle;

    private void Start()
    {
        originalPosition = transform.position;
        bottle = GetComponent<Bottle>();
    }

    private void OnMouseDown()
    {
        BottleManager.Instance.OnBottleClicked(this);
    }

    public void Select()
    {
        transform.position = originalPosition + Vector3.up * liftHeight;
    }

    public void Deselect()
    {
        transform.position = originalPosition;
    }
}
