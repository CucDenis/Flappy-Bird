using System.Collections;
using UnityEngine;

public sealed class BackgroundCloudSpawner : MonoBehaviour
{
    [Header("Cloud Prefabs")]
    [SerializeField]
    private BackgroundCloudMover[] cloudPrefabs;

    [Header("Horizontal Area")]
    [SerializeField] private float spawnX = 9f;
    [SerializeField] private float destructionX = -9f;

    [Header("Vertical Area")]
    [SerializeField] private float minimumY = -4f;
    [SerializeField] private float maximumY = 4f;

    [Header("Spawn Timing")]
    [SerializeField] private float minimumSpawnDelay = 2.5f;
    [SerializeField] private float maximumSpawnDelay = 5.5f;
    [SerializeField] private int initialCloudCount = 4;

    [Header("Movement")]
    [SerializeField] private float minimumSpeed = 0.25f;
    [SerializeField] private float maximumSpeed = 0.8f;

    [Header("Scale")]
    [SerializeField] private float minimumScale = 0.7f;
    [SerializeField] private float maximumScale = 1.4f;

    private bool cloudsEnabled = true;

    private Coroutine spawnCoroutine;

    private void Start()
    {
        SpawnInitialClouds();

        spawnCoroutine = StartCoroutine(
            SpawnLoop()
        );
    }

    public void SetCloudsEnabled(bool enabled)
    {
        cloudsEnabled = enabled;
    }

    private void SpawnInitialClouds()
    {
        if (!cloudsEnabled)
        {
            return;
        }

        for (
            int index = 0;
            index < initialCloudCount;
            index++
        )
        {
            float randomX = Random.Range(
                destructionX,
                spawnX
            );

            SpawnCloud(randomX);
        }
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

            if (!cloudsEnabled)
            {
                continue;
            }

            SpawnCloud(spawnX);
        }
    }

    private void SpawnCloud(float x)
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
                Random.Range(
                    0,
                    cloudPrefabs.Length
                )
            ];

        if (prefab == null)
        {
            return;
        }

        float y = Random.Range(
            minimumY,
            maximumY
        );

        BackgroundCloudMover cloud =
            Instantiate(
                prefab,
                new Vector3(x, y, 1f),
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