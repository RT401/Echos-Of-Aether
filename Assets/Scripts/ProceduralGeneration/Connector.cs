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
        if (isSeed && !s_initialised)
        {
            s_initialised = true;
            s_zonePrefabs = zonePrefabs;
            s_remainingToSpawn = Mathf.Max(0, totalZonesToPlace - 1); // exclude starting zone
            s_timeGenerated = ((float)DateTime.Now.Hour + ((float)DateTime.Now.Minute * 0.01f)) / 24f;
        }

        // Only spawn if a run is initialized and we still have zones to spawn
        if (!s_initialised) return;
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
        ZonePrefab prefab = s_zonePrefabs[(int)GetNextZoneIndex()];

        // spawn it somewhere appropriate
        ZonePrefab nextZone = Instantiate(prefab, Vector3.one * 9999f, Quaternion.identity);

        if (nextZone.connectors == null || nextZone.connectors.Length == 0)
        {
            Debug.LogError("Spawned zone has no connectors.");
            return;
        }

        // pick a random connector from the new zone
        ConnectorPoint attachB = nextZone.connectors[UnityEngine.Random.Range(0, nextZone.connectors.Length)];

        // Snap so connectors overlap and face each other
        SnapConnectors(transform, attachB.transform, nextZone.transform);

        // Mark the new connector as occupied
        MarkConnectorOccupied(attachB);

        // Decrement remaining, and choose a different connector to continue spawning from
        s_remainingToSpawn--;

        if (s_remainingToSpawn <= 0) return;

        ConnectorPoint continueFrom = PickDifferentConnector(nextZone, attachB);
        if (continueFrom == null) return;
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

    private static void MarkConnectorOccupied(ConnectorPoint cp)
    {
        var spawner = cp.GetComponent<Connector>();
        if (spawner != null)
        {
            spawner.occupied = true;
        }
    }

    private static ConnectorPoint PickDifferentConnector(ZonePrefab zone, ConnectorPoint exclude)
    {
        if (zone.connectors.Length <= 1) return null;

        // Try a few times to pick a different one
        for (int i = 0; i < 8; i++)
        {
            var c = zone.connectors[UnityEngine.Random.Range(0, zone.connectors.Length)];
            if (c != null && c != exclude) return c;
        }

        // Fallback: just pick the first different one
        foreach (var c in zone.connectors)
            if (c != null && c != exclude)
                return c; 

        return null;
    }

    private float GetNextZoneIndex()
    {
        float trueRand = UnityEngine.Random.Range(0, zonePrefabs.Length);
        trueRand += s_timeGenerated;
        trueRand = trueRand % zonePrefabs.Length;
        return trueRand;
    }
}
