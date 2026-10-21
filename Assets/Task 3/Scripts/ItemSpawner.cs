using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [SerializeField] private ConveyorItem[] itemPrefabs;   // one prefab per color
    [SerializeField] private ConveyorBelt startBelt;
    [SerializeField] private float spawnInterval = 1.5f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnItem();
        }
    }

    private void SpawnItem()
    {
        ConveyorItem randomPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

        ConveyorItem item = Instantiate(randomPrefab);
        item.PlaceOnBelt(startBelt);
    }
}
