using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapVisualizer : MonoBehaviour
{
    private Transform parent;
    public Color startColor;
    public Color exitColor;
    public GameObject roadStraight;
    public GameObject roadTileCorner;
    public GameObject tileEmpty;
    public GameObject startTile;
    public GameObject exitTile;
    public GameObject[] environmentTiles;

    Dictionary<Vector3, GameObject> dictionaryOfObstacles = new Dictionary<Vector3, GameObject>();

    private void Awake()
    {
        parent = this.transform;
    }

    public void VisualizeMap(MapGrid grid, MapData data, bool visualizeUsingPrefabs)
    {
        if(visualizeUsingPrefabs)
        {
            VisualizeUsingPrefabs(grid, data);
        }
        else
        {
            VisualizeUsingPrimitives(grid, data);
        }
    }

    private void VisualizeUsingPrefabs(MapGrid grid, MapData data)
    {
        for (int i = 0; i < data.path.Count; i++)
        {
            var position = data.path[i];
            if (position != data.exitPosition)
            {
                grid.SetCell(position.x, position.z, CellObjectType.Road);
            }
        }
        Vector3 buffer = new Vector3(0, 0, 0);
        for (int col = 0; col < grid.Width; col++)
        {
            buffer.z = 0;
            for (int row = 0; row < grid.Length; row++)
            {
                var cell = grid.GetCell(col, row);
                var position = new Vector3(cell.X, 0, cell.Z);

                var index = grid.CalculateIndexFromCoordinates(position.x, position.z);
                if (data.obstacleArray[index] && cell.IsTaken == false)
                {
                    cell.ObjectType = CellObjectType.Obstacle;
                }
                Direction previousDirection = Direction.None;
                Direction nextDirection = Direction.None;
                switch (cell.ObjectType)
                {
                    case CellObjectType.Empty:
                        CreateIndicator(position + buffer, tileEmpty);
                        break;
                    case CellObjectType.Road:
                        if (data.path.Count > 0)
                        {
                            previousDirection = GetDirectionOfPreviousCell(position, data);
                            nextDirection = GetDirectionOfNextCell(position, data);
                        }
                        if (previousDirection == Direction.Down && nextDirection == Direction.Left || previousDirection == Direction.Left && nextDirection == Direction.Down)
                            CreateIndicator(position + buffer, roadTileCorner, Quaternion.Euler(0, -90, 0));
                        if (previousDirection == Direction.Up && nextDirection == Direction.Right || previousDirection == Direction.Right && nextDirection == Direction.Up)
                            CreateIndicator(position + buffer, roadTileCorner, Quaternion.Euler(0, 90, 0));
                        if (previousDirection == Direction.Right && nextDirection == Direction.Down || previousDirection == Direction.Down && nextDirection == Direction.Right)
                            CreateIndicator(position + buffer, roadTileCorner, Quaternion.Euler(0, 180, 0));
                        if (previousDirection == Direction.Left && nextDirection == Direction.Up || previousDirection == Direction.Up && nextDirection == Direction.Left)
                            CreateIndicator(position + buffer, roadTileCorner);
                        if (previousDirection == Direction.Right && nextDirection == Direction.Left || previousDirection == Direction.Left && nextDirection == Direction.Right)
                            CreateIndicator(position + buffer, roadStraight, Quaternion.Euler(0, 90, 0));
                        if (previousDirection == Direction.Up && nextDirection == Direction.Down || previousDirection == Direction.Down && nextDirection == Direction.Up)
                            CreateIndicator(position + buffer, roadStraight);
                        break;
                    case CellObjectType.Obstacle:
                        int randomIndex = Random.Range(0, environmentTiles.Length);
                        CreateIndicator(position + buffer, environmentTiles[randomIndex]);
                        break;
                    case CellObjectType.Start:
                        if (data.path.Count > 0)
                            nextDirection = GetDirectionFromVectors(data.path[0], position);
                        if (nextDirection == Direction.Right || nextDirection == Direction.Left)
                            CreateIndicator(position + buffer, startTile, Quaternion.Euler(0, 90, 0));
                        else
                            CreateIndicator(position + buffer, startTile);
                        break;
                    case CellObjectType.Exit:
                        if (data.path.Count > 0)
                            previousDirection = GetDirectionOfPreviousCell(position, data);
                        switch (previousDirection)
                        {
                            case Direction.Right:
                                CreateIndicator(position + buffer, exitTile, Quaternion.Euler(0, 90, 0));
                                break;
                            case Direction.Left:
                                CreateIndicator(position + buffer, exitTile, Quaternion.Euler(0, -90, 0));
                                break;
                            case Direction.Down:
                                CreateIndicator(position + buffer, exitTile, Quaternion.Euler(0, 180, 0));
                                break;
                            default:
                                CreateIndicator(position + buffer, exitTile);
                                break;

                        }
                        break;
                    default:
                        break;
                }
                buffer.z += 3;
            }
            buffer.x += 3;
        }
    }

    private Direction GetDirectionOfNextCell(Vector3 position, MapData data)
    {
        int index = data.path.FindIndex(a => a == position);
        var nextCellPosition = data.path[index + 1];
        return GetDirectionFromVectors(nextCellPosition, position);
    }

    private Direction GetDirectionOfPreviousCell(Vector3 position, MapData data)
    {
        var index = data.path.FindIndex(a => a == position);
        var previousCellPosition = Vector3.zero;
        if (index > 0)
            previousCellPosition = data.path[index - 1];
        else
            previousCellPosition = data.startPosition;
        return GetDirectionFromVectors(previousCellPosition, position);
    }

    private Direction GetDirectionFromVectors(Vector3 positionToGoTo, Vector3 position)
    {
        if (positionToGoTo.x > position.x)
            return Direction.Right;
        else if (positionToGoTo.x < position.x)
            return Direction.Left;
        else if (positionToGoTo.z > position.z)
            return Direction.Up;
        return Direction.Down;
    }

    private void CreateIndicator(Vector3 position, GameObject prefab, Quaternion rotation = new Quaternion())
    {
        var placementPosition = position;
        var element = Instantiate(prefab, placementPosition, rotation);
        element.transform.parent = parent;
        dictionaryOfObstacles.Add(position, element);
    }

    private void VisualizeUsingPrimitives(MapGrid grid, MapData data)
    {
        PlaceStartAndExitPoints(data);
        for (int i = 0; i < data.obstacleArray.Length; i++)
        {
            if (data.obstacleArray[i])
            {
                var positionOnGrid = grid.CalculateCoordinatesFromIndex(i);
                if (positionOnGrid == data.startPosition || positionOnGrid == data.exitPosition)
                    continue;
                grid.SetCell(positionOnGrid.x, positionOnGrid.z, CellObjectType.Obstacle);
                if (PlaceKnightObstacle(data, positionOnGrid))
                    continue;
                if(dictionaryOfObstacles.ContainsKey(positionOnGrid) == false)
                {
                    CreateIndicator(positionOnGrid, Color.white, PrimitiveType.Cube);
                }
            }
        }
    }

    private bool PlaceKnightObstacle(MapData data, Vector3 positionOnGrid)
    {
        foreach (var knight in data.knightPiecesList)
        {
            if (knight.Position == positionOnGrid)
            {
                CreateIndicator(positionOnGrid, Color.red, PrimitiveType.Cube);
                return true;
            }
        }
        return false;
    }

    private void PlaceStartAndExitPoints(MapData data)
    {
        CreateIndicator(data.startPosition, startColor, PrimitiveType.Sphere);
        CreateIndicator(data.exitPosition, exitColor, PrimitiveType.Sphere);
    }

    private void CreateIndicator(Vector3 position, Color color, PrimitiveType sphere)
    {
        var element = GameObject.CreatePrimitive(sphere);
        dictionaryOfObstacles.Add(position, element);
        element.transform.position = position + new Vector3(0.5f, 0.5f, 0.5f);
        element.transform.parent = parent;
        var renderer = element.GetComponent<Renderer>();
        renderer.material.SetColor("_Color", color);
    }

    public void ClearMap()
    {
        foreach ( var obstacle in dictionaryOfObstacles.Values)
        {
            Destroy(obstacle);
        }
        dictionaryOfObstacles.Clear();
    }
}