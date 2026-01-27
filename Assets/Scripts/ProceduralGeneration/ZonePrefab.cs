using UnityEngine;

public class ZonePrefab : MonoBehaviour
{
    public ConnectorPoint[] connectors;

    private void OnValidate()
    {
        if (connectors == null || connectors.Length == 0)
        {
            connectors = GetComponentsInChildren<ConnectorPoint>();
        }
    }
}
