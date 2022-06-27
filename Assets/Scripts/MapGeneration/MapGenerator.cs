using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GridVisualizer gridVisualizer;
    public MapVisualizer mapVisualizer;
    public Direction startEdge;
    public Direction exitEdge;
    public bool randomPlacement;
    public bool visualizeUsingPrefabs = false;
    public bool autoRepair = true;
    public int numberOfPieces;
    public int width;
    public int length;
    private Vector3 startPosition;
    private Vector3 exitPosition;
    private MapGrid grid;
    private CandidateMap map;

    void Start()
    {
        gridVisualizer.VisualizeGrid(width, length);
        GenerateNewMap();
    }

    public void GenerateNewMap()
    {
        mapVisualizer.ClearMap();
        grid = new MapGrid(width, length);
        MapHelper.RandomlyChooseAndSetStartAndExit(grid, ref startPosition, ref exitPosition, randomPlacement, startEdge, exitEdge);

        map = new CandidateMap(grid, numberOfPieces);
        map.CreateMap(startPosition, exitPosition, autoRepair);
        mapVisualizer.VisualizeMap(grid, map.ReturnMapData(), visualizeUsingPrefabs);
    }

    public void TryRepair()
    {
        if (map != null)
        {
            var listOfObstaclesToRemove = map.Repair();
            if (listOfObstaclesToRemove.Count > 0)
            {
                mapVisualizer.ClearMap();
                mapVisualizer.VisualizeMap(grid, map.ReturnMapData(), visualizeUsingPrefabs);
            }
        }
    }
}
