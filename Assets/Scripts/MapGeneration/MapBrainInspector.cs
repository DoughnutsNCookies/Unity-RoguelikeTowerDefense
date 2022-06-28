using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapBrain))]
public class MapBrainInspector : Editor
{
    MapBrain mapBrain;
    Waypoints waypoints;
    WaveSpawner waveSpawner;

    void OnEnable()
    {
        mapBrain = (MapBrain)target;
        waypoints = FindObjectOfType<Waypoints>();
        waveSpawner = FindObjectOfType<WaveSpawner>();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (Application.isPlaying)
        {
            GUI.enabled = !mapBrain.IsAlgorithmRunning;
            if (GUILayout.Button("Run Genetic Algorithm"))
            {
                mapBrain.RunAlgorithm();
            }
            if (GUILayout.Button("Spawn Enemy"))
            {
                waypoints.SetWaypointCount();
                waveSpawner.spawnPoint = FindObjectOfType<Spawnpoint>().transform;
                waveSpawner.isSpawning = true;
            }
        }
    }
}
