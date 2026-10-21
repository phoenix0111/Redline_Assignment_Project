using UnityEngine;

public class Junction : MonoBehaviour
{
    [Header("Switch Settings")]
    [SerializeField] private ConveyorBelt[] outputBelts;
    [SerializeField] private GameObject[] arrows;   // one arrow per output belt, same order
    [SerializeField] private int startingBeltIndex = 0;

    private int activeIndex;

    public ConveyorBelt GetActiveBelt()
    {
        return outputBelts[activeIndex];
    }

    private void Start()
    {
        activeIndex = startingBeltIndex;
        ShowActiveArrow();
    }

    private void OnMouseDown()
    {
        activeIndex++;

        if (activeIndex >= outputBelts.Length)
            activeIndex = 0;

        ShowActiveArrow();
    }

    private void ShowActiveArrow()
    {
        for (int i = 0; i < arrows.Length; i++)
        {
            arrows[i].SetActive(i == activeIndex);
        }
    }
}
