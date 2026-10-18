
using System.Collections.Generic;
using UnityEngine;

public class Bottle : MonoBehaviour
{
    [Header("Bottle Settings")]
    [SerializeField] private int capacity = 4;
    [SerializeField] private Transform[] slots;
    [SerializeField] private GameObject liquidSegmentPrefab;

    [Header("Starting Liquid")]
    [SerializeField]
    private List<LiquidColor> liquids =
        new List<LiquidColor>();

    private readonly List<GameObject> segmentObjects =
        new List<GameObject>();

    public int Capacity => capacity;
    public int LiquidCount => liquids.Count;
    public bool IsEmpty => liquids.Count == 0;
    public bool IsFull => liquids.Count >= capacity;

    public LiquidColor GetTopColor()
    {
        return liquids[liquids.Count - 1];
    }

    public bool CanReceive(LiquidColor color)
    {
        if (IsFull)
            return false;

        if (IsEmpty)
            return true;

        return GetTopColor() == color;
    }

    public int GetTopGroupCount()
    {
        if (IsEmpty)
            return 0;

        LiquidColor topColor = GetTopColor();
        int count = 0;

        for (int i = liquids.Count - 1; i >= 0; i--)
        {
            if (liquids[i] != topColor)
                break;

            count++;
        }

        return count;
    }

    public void RemoveTopLiquid()
    {
        liquids.RemoveAt(liquids.Count - 1);
    }

    public void AddLiquid(LiquidColor color)
    {
        liquids.Add(color);
    }

    private void Start()
    {
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        foreach (GameObject segment in segmentObjects)
        {
            if (segment != null)
                Destroy(segment);
        }

        segmentObjects.Clear();

        for (int i = 0; i < liquids.Count; i++)
        {
            GameObject segment = Instantiate(liquidSegmentPrefab, slots[i].position, slots[i].rotation, transform);

            SpriteRenderer spriteRenderer = segment.GetComponent<SpriteRenderer>();

            spriteRenderer.color = GetUnityColor(liquids[i]);

            segmentObjects.Add(segment);
        }
    }

    private Color GetUnityColor(LiquidColor liquidColor)
    {
        switch (liquidColor)
        {
            case LiquidColor.Red:
                return Color.red;

            case LiquidColor.Blue:
                return Color.blue;

            case LiquidColor.Green:
                return Color.green;

            case LiquidColor.Yellow:
                return Color.yellow;

            default:
                return Color.white;
        }
    }
}
