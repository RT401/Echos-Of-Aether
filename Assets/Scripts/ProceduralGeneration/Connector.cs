using System;
using System.Numerics;
using System.Threading.Tasks.Dataflow;
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

    private void TrySpawnNext()
    {
        if (occupied || s_remainingToSpawn <= 0) return;
        if (s_zonePrefabs == null || s_zonePrefabs.Length == 0)
        {
            Debug.LogError("No zone prefabs assigned for generation.");
            return;
        }
        
        occupied = true; // Mark this connector as occupied

        // pick next zone prefab
        zonePrefabs prefab = s_zonePrefabs[GetNextZoneIndex()];

        // spawn it somewhere appropriate
        zonePrefabs nextZone = Instantiate(prefab, Vector3.one * 9999f, Quaternion.identity);

        if (nextZone.connectors == null || nextZone.connectors.Length == 0)
        {
            Debug.LogError("Spawned zone has no connectors.");
            return;
        }

        // pick a random connector from the new zone
        ConnectorPoint attachB = nextZone.connectors[UnityEngine.Random.Range(0, nextZone.connectors.Length)];

        // Snap so connectors overlap and face each other
        SnapConnectors(transform, attachB.transform, nextZone.transform);
    }

    private static void SnapConnectors(Transform connectorA, Transform connectorB, Transform rootB)
    {
        // Rotate rootB so that connectorB faces connectorA
        Quaternion deltaRot = Quaternion.FromToRotation(connectorB.forward, -connectorA.forward);
        rootB.rotation = deltaRot * rootB.rotation;

        // Move rootB so connector positions match
        Vector3 deltaPos = connectorA.position - connectorB.position;
        rootB.position += deltaPos;
    }

    private float GetNextZoneIndex()
    {
        float trueRand = UnityEngine.Random.Range(0, Zones.Length);
        trueRand += timeGenerated;
        trueRand = trueRand % Zones.Length;
        return trueRand;
    }
}
