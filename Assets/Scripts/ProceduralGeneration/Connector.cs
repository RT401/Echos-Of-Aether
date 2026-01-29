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

            UnityEngine.Random.InitState(13245);
            s_timeGenerated = ((float)DateTime.Now.Hour + ((float)DateTime.Now.Minute * 0.01f)) / 24f;
        }

        // Only spawn if a run is initialized and we still have zones to spawn
        if (!s_initialised) return;
        TrySpawnNext();
    }

    private void TrySpawnNext()
    {
        Debug.Log(s_remainingToSpawn);

        if (occupied || s_remainingToSpawn <= 0) return;
        if (zonePrefabs == null || zonePrefabs.Length == 0)
        {
            Debug.LogError("No zone prefabs assigned for generation.");
            return;
        }

        // pick next zone prefab
        ZonePrefab prefab = zonePrefabs[(int)GetNextZoneIndex()];

        // spawn it somewhere appropriate
        ZonePrefab nextZone = Instantiate(prefab, Vector3.one * 9999f, Quaternion.identity);

        if (nextZone.connectors == null || nextZone.connectors.Length == 0)
        {
            Debug.LogError("Spawned zone has no connectors.");
            Destroy(nextZone.gameObject);
            return;
        }

        //Try arrach using ANY connector in new room
        ConnectorPoint attachB = null;
        if (TryAttachingUsingAnyConnector(nextZone, this.transform, out attachB))
        {
            // success: mark both ends occupied
            MarkConnectorOccupied(attachB);
            this.occupied = true;

            // decrement Only on success
            s_remainingToSpawn--;
            if (s_remainingToSpawn <= 0) return;

            // continue from a different connector on hte placed zone
            ConnectorPoint continueFrom = PickDifferentConnector(nextZone, attachB);
            if (continueFrom == null) return;

            var nextSpawner = continueFrom.GetComponent<Connector>();
            if (nextSpawner != null)
                nextSpawner.TrySpawnNext();
        }
        else
        {
            // failed: destroy room, close this connector, DO NOT decrement
            Destroy(nextZone.gameObject);
            this.occupied = true; // "closed"
            Debug.Log("No valid connector on spawned room fit here. Closing this point.");   
        }
        
        // // pick a random connector from the new zone
        // ConnectorPoint attachB = nextZone.connectors[UnityEngine.Random.Range(0, nextZone.connectors.Length)];

        // // Snap so connectors overlap and face each other
        // SnapConnectors(transform, attachB.transform, nextZone.transform);

        // // Mark the new connector as occupied
        // MarkConnectorOccupied(attachB);

        // // Decrement remaining, and choose a different connector to continue spawning from
        // s_remainingToSpawn--;

        // if (s_remainingToSpawn <= 0) return;

        // ConnectorPoint continueFrom = PickDifferentConnector(nextZone, attachB);
        // if (continueFrom == null) return;
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

    private bool TryAttachingUsingAnyConnector(ZonePrefab newZone, Transform existingConnector, out ConnectorPoint chosen)
    {
        chosen = null;

        // shuffeling for randomness (not always tried in same order)
        var conns = (ConnectorPoint[])newZone.connectors.Clone();
        for (int i = 0; i < conns.Length; i++)
        {
            int j = UnityEngine.Random.Range(i, conns.Length);
            (conns[i], conns[j]) = (conns[j], conns[i]);
        }

        foreach (var attachB in conns)
        {
            if (attachB == null) continue;

            // Snap so connectors overlap and face each other
            SnapConnectors(existingConnector, attachB.transform, newZone.transform);

            // If no clash we done
            if (!DoesZoneClash(newZone))
            {
                chosen = attachB;
                return true;
            }
        }

        return false;
    }

    [SerializeField] private LayerMask roomCollisionMask = ~0;
    private bool DoesZoneClash(ZonePrefab zone)
    {
        Debug.Log(zone.name);
        Collider[] myCols = zone.GetComponentsInChildren<Collider>();
        if (myCols == null || myCols.Length ==  0) return false;

        foreach (var col in myCols)
        {
            if (col == null) continue;

            // Ignore triggers (connectors usually)
            if (col.isTrigger) continue;

            // Ignore connector cubes by name (or change this to a layer/tag check)
            if (col.gameObject.name.Contains("Connector")) continue;

            // Find any overlaps against other rooms (mask!)
            Collider[] hits = Physics.OverlapBox(
                col.bounds.center,
                col.bounds.extents,
                col.transform.rotation,   // rotation-aware
                roomCollisionMask,
                QueryTriggerInteraction.Ignore
            );

            foreach (var hit in hits)
            {
                if (hit == null) continue;

                // Ignore our own colliders
                if (hit.transform.IsChildOf(zone.transform)) continue;

                // confirm it's a real penetration (not just broadphase overlap)
                Vector3 dir;
                float dist;
                bool overlapped = Physics.ComputePenetration(
                    col, col.transform.position, col.transform.rotation,
                    hit, hit.transform.position, hit.transform.rotation,
                    out dir, out dist
                );

                if (overlapped && dist > 0.001f)
                    return true;
            }
        }

        return false;
    }

    private float GetNextZoneIndex()
    {
        float trueRand = UnityEngine.Random.Range(0, zonePrefabs.Length);
        trueRand += s_timeGenerated;
        trueRand = trueRand % zonePrefabs.Length;

        if  ( 
                ( 
                    (int)trueRand == Array.FindIndex(zonePrefabs, z => z.gameObject.name == "SectionPrefab_End") 
                ) 
                && 
                ( 
                    s_remainingToSpawn > (totalZonesToPlace/2) 
                )
            )
        {
            Debug.Log("Refinding rand" + trueRand);
            trueRand = GetNextZoneIndex();
        }

        return trueRand;
    }
}