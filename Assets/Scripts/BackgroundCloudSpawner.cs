using System.Collections;
using UnityEngine;

public sealed class BackgroundCloudSpawner : MonoBehaviour
{
    [Header("Clouds")]
    [SerializeField]
    private BackgroundCloudMover[] cloudPrefabs;

    [Header("Spawn Area")]
    [SerializeField] private float spawnX = 9f;
    [SerializeField] private float destructionX = -9f;
    [SerializeField] private float minimumY = -4f;
    [SerializeField] private float maximumY = 4f;

    [Header("Timing")]
    [SerializeField] private float minimumSpawnDelay = 3f;
    [SerializeField] private float maximumSpawnDelay = 7f;

    [Header("Movement")]
    [SerializeField] private float minimumSpeed = 0.2f;
    [SerializeField] private float maximumSpeed = 0.8f;

    [Header("Scale")]
    [SerializeField] private float minimumScale = 0.6f;
    [SerializeField] private float maximumScale = 1.5f;

    private Coroutine spawnCoroutine;

    private void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float delay = Random.Range(
                minimumSpawnDelay,
                maximumSpawnDelay
            );

            yield return new WaitForSeconds(delay);

            SpawnCloud();
        }
    }

    private void SpawnCloud()
    {
        if (
            cloudPrefabs == null ||
            cloudPrefabs.Length == 0
        )
        {
            return;
        }

        BackgroundCloudMover prefab =
            cloudPrefabs[
                Random.Range(0, cloudPrefabs.Length)
            ];

        if (prefab == null)
        {
            Debug.LogError(
                "BackgroundCloudSpawner contains a missing cloud prefab reference.",
                this
            );

            return;
        }

        float spawnY = Random.Range(
            minimumY,
            maximumY
        );

        BackgroundCloudMover cloud = Instantiate(
            prefab,
            new Vector3(spawnX, spawnY, 1f),
            Quaternion.identity
        );

        float scale = Random.Range(
            minimumScale,
            maximumScale
        );

        cloud.transform.localScale =
            Vector3.one * scale;

        float speed = Random.Range(
            minimumSpeed,
            maximumSpeed
        );

        cloud.Initialize(
            speed,
            destructionX
        );
    }
}