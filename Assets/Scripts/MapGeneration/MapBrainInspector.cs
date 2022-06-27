using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapBrain))]
public class MapBrainInspector : Editor
{
    MapBrain mapBrain;

    void OnEnable()
    {
        mapBrain = (MapBrain)target;
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
        }
    }
}
