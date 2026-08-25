using System.Collections;
using UnityEngine;

public sealed class BackgroundCloudSpawner : MonoBehaviour
{
    [Header("Cloud Prefabs")]
    [SerializeField]
    private BackgroundCloudMover[] cloudPrefabs;

    [Header("Horizontal Area")]
    [SerializeField]
    private float spawnX = 9f;

    [SerializeField]
    private float destructionX = -9f;

    [Header("Vertical Area")]
    [SerializeField]
    private float minimumY = -4f;

    [SerializeField]
    private float maximumY = 4f;

    [Header("Spawn Timing")]
    [SerializeField]
    private float minimumSpawnDelay = 2.5f;

    [SerializeField]
    private float maximumSpawnDelay = 5.5f;

    [SerializeField]
    private int initialCloudCount = 4;

    [Header("Movement")]
    [SerializeField]
    private float minimumSpeed = 0.25f;

    [SerializeField]
    private float maximumSpeed = 0.8f;

    [Header("Scale")]
    [SerializeField]
    private float minimumScale = 0.7f;

    [SerializeField]
    private float maximumScale = 1.4f;

    private bool cloudsEnabled = false;

    private Coroutine spawnCoroutine;

    public void BeginSpawning()
    {
        if (cloudsEnabled)
        {
            return;
        }

        cloudsEnabled = true;

        SpawnInitialClouds();

        StartCloudSpawning();
    }

    public void StopSpawning(bool clearExistingClouds)
    {
        cloudsEnabled = false;

        StopCloudSpawning();

        if (clearExistingClouds)
        {
            ClearAllClouds();
        }
    }

    public void SetCloudsEnabled(bool enabled)
    {
        if (cloudsEnabled == enabled)
        {
            return;
        }

        cloudsEnabled = enabled;

        if (cloudsEnabled)
        {
            StartCloudSpawning();
        }
        else
        {
            StopCloudSpawning();
        }
    }

    public void SetVerticalRange(
        float minimum,
        float maximum
    )
    {
        minimumY = minimum;
        maximumY = maximum;
    }

    private void StartCloudSpawning()
    {
        if (
            !cloudsEnabled ||
            spawnCoroutine != null
        )
        {
            return;
        }

        spawnCoroutine =
            StartCoroutine(
                SpawnLoop()
            );
    }

    private void StopCloudSpawning()
    {
        if (spawnCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            spawnCoroutine
        );

        spawnCoroutine = null;
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
            float randomX =
                Random.Range(
                    destructionX,
                    spawnX
                );

            SpawnCloud(randomX);
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (cloudsEnabled)
        {
            float delay =
                Random.Range(
                    minimumSpawnDelay,
                    maximumSpawnDelay
                );

            yield return
                new WaitForSeconds(delay);

            if (!cloudsEnabled)
            {
                break;
            }

            SpawnCloud(spawnX);
        }

        spawnCoroutine = null;
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

        float y =
            Random.Range(
                minimumY,
                maximumY
            );

        BackgroundCloudMover cloud =
            Instantiate(
                prefab,
                new Vector3(
                    x,
                    y,
                    1f
                ),
                Quaternion.identity
            );

        float scale =
            Random.Range(
                minimumScale,
                maximumScale
            );

        cloud.transform.localScale =
            Vector3.one * scale;

        float speed =
            Random.Range(
                minimumSpeed,
                maximumSpeed
            );

        cloud.Initialize(
            speed,
            destructionX
        );
    }

    public void ClearAllClouds()
    {
        BackgroundCloudMover[] clouds =
            FindObjectsByType<BackgroundCloudMover>(
                FindObjectsSortMode.None
            );

        foreach (
            BackgroundCloudMover cloud
            in clouds
        )
        {
            if (cloud != null)
            {
                Destroy(
                    cloud.gameObject
                );
            }
        }
    }
}