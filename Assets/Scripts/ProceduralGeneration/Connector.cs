using System;
using UnityEngine;

[RequireComponent(typeof(ConnectorPoint))]
public class Connector : MonoBehaviour
{
    [Header("Seed Settings (only used on start)")]
    public bool isSeed = false;
    public ZonePrefab[] zonePrefabs;
    public int totalZonesToPlace = 10; // includes the starting zone

    [Header("Debug")]
    [SerializeField] private bool occupied = false;

    // Shared run state (so any connector can continue the same generation run)
    private static bool s_initialised;
    private static int s_remainingToSpawn;
    private static float s_timeGenerated;
    private static ZonePrefab[] s_zonePrefabs;

    void Start()
    {
        // Initialize run once, from the seed connector
        if (isSeed && !s_initialized)
        {
            s_initialized = true;
            s_zonePrefabs = zonePrefabs;
            s_remainingToSpawn = Mathf.Max(0, totalZonesToPlace - 1); // exclude starting zone
            s_timeGenerated = ((float)DateTime.Now.Hour + ((float)DateTime.Now.Minute * 0.01f)) / 24f;
        }

        // Only spawn if a run is initialized and we still have zones to spawn
        if (!s_initialized) return;
        TrySpawnNext();
    }

    private float GetNextZoneIndex()
    {
        float trueRand = UnityEngine.Random.Range(0, Zones.Length);
        trueRand += timeGenerated;
        trueRand = trueRand % Zones.Length;
        return trueRand;
    }
}
