using UnityEngine;

public class Hole : MonoBehaviour
{
    public CharacterColor acceptedColor;
    public Vector2Int gridPosition;

    private void OnMouseDown()
    {
        GridManager.Instance.OnHoleTapped(this);
    }
}
