using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class MapBrain : MonoBehaviour
{
    //Genetic algorithm parameters
    [SerializeField, Range(20, 100)] private int populationSize = 20; // Increasing this parameter will increase time needed to run the algorithm
    [SerializeField, Range(0, 100)] private int crossoverRate = 100; // 100 candidate maps from population to crossover with the best parent found from the population
    [SerializeField, Range(0, 100)] private int mutationRate = 0; // To achieve better variations among candidate maps
    [SerializeField, Range(1, 100)] private int generationLimit = 10; // A way to stop our algorithm
    
    private double crossoverRatePercent;
    private double mutationRatePercent;

    //Algorithm Variables
    private List<CandidateMap> currentGeneration;
    private int totalFitnessThisGeneration;
    private int bestFitnessScoreAllTime = 0;
    private CandidateMap bestMap = null;
    private int bestMapGenerationNumber = 0;
    private int generationNumber = 1;

    //Fitness Parameters
    [SerializeField] private int fitnessCornerMin = 6;
    [SerializeField] private int fitnessCornerMax = 12;
    [SerializeField, Range(1, 3)] private int fitnessCornerWeight = 1;
    [SerializeField, Range(1, 3)] private int fitnessNearcornerWeight = 1;
    [SerializeField, Range(0.3f, 1f)] private float fitnessObstacleWeight = 1;
    [SerializeField, Range(1, 5)] private int fitnessPathWeight = 1;

    //Map Start Parameters
    [SerializeField, Range(3, 20)] private int widthOfMap = 11;
    [SerializeField, Range(3, 20)] private int lengthOfMap = 11;
    [SerializeField, Range(4, 100)] private int numberOfKnightPieces = 7;
    [SerializeField] private Direction startPositionEdge = Direction.Left;
    [SerializeField] private Direction exitPositionEdge = Direction.Right;
    [SerializeField] private bool randomStartAndEnd = false;
    private Vector3 startPosition;
    private Vector3 exitPosition;
    private MapGrid grid;

    //Visualize Grid
    public MapVisualizer mapVisualizer;
    DateTime startDate, endDate;
    private bool isAlgorithmRunning = false;

    public bool IsAlgorithmRunning { get => isAlgorithmRunning; }

    void Start()
    {
        mutationRatePercent = mutationRate / 100D;
        crossoverRatePercent = crossoverRate / 100D;
    }

    public void RunAlgorithm()
    {
        ResetAlgorithmVariables();
        mapVisualizer.ClearMap();
        grid = new MapGrid(widthOfMap, lengthOfMap);
        MapHelper.RandomlyChooseAndSetStartAndExit(grid, ref startPosition, ref exitPosition, randomStartAndEnd, startPositionEdge, exitPositionEdge);

        isAlgorithmRunning = true;
        startDate = DateTime.Now;
        FindOptimalSolution(grid);
    }

    private void ResetAlgorithmVariables()
    {
        totalFitnessThisGeneration = 0;
        bestFitnessScoreAllTime = 0;
        bestMap = null;
        bestMapGenerationNumber = 0;
        generationNumber = 0;
    }

    private void FindOptimalSolution(MapGrid grid)
    {
        currentGeneration = new List<CandidateMap>(populationSize);
        for (int i = 0; i < populationSize; i++)
        {
            var CandidateMap = new CandidateMap(grid, numberOfKnightPieces);
            CandidateMap.CreateMap(startPosition, exitPosition, true);
            currentGeneration.Add(CandidateMap);
        }

        StartCoroutine(GeneticAlgorithm());
    }

    private IEnumerator GeneticAlgorithm()
    {
        totalFitnessThisGeneration = 0;
        int bestFitnessScoreThisGeneration = 0;
        CandidateMap bestMapThisGeneration = null;
        foreach (var candidate in currentGeneration)
        {
            candidate.FindPath();
            candidate.Repair();
            var fitness = CalculateFitness(candidate.ReturnMapData());

            totalFitnessThisGeneration += fitness;
            if (fitness > bestFitnessScoreThisGeneration)
            {
                bestFitnessScoreThisGeneration = fitness;
                bestMapThisGeneration = candidate;
            }
        }
        if (bestFitnessScoreThisGeneration > bestFitnessScoreAllTime)
        {
            bestFitnessScoreAllTime = bestFitnessScoreThisGeneration;
            bestMap = bestMapThisGeneration.DeepClone();
            bestMapGenerationNumber = generationNumber;
        }
        generationNumber++;
        yield return new WaitForEndOfFrame();
        if (generationNumber < generationLimit)
        {
            List<CandidateMap> nextGeneration = new List<CandidateMap>();
            while (nextGeneration.Count < populationSize)
            {
                var parent1 = currentGeneration[RouletteWheelSelection()];
                var parent2 = currentGeneration[RouletteWheelSelection()];
                CandidateMap child1;
                CandidateMap child2;

                CrossoverParents(parent1, parent2, out child1, out child2);
                child1.AddMutation(mutationRatePercent);
                child2.AddMutation(mutationRatePercent);
                nextGeneration.Add(child1);
                nextGeneration.Add(child2);
            }
            currentGeneration = nextGeneration;
            StartCoroutine(GeneticAlgorithm());
        }
        else
            ShowResults();
    }

    private void ShowResults()
    {
        isAlgorithmRunning = false;
        // Debug.Log("Best solution at generation " + bestMapGenerationNumber + " with score: " + bestFitnessScoreAllTime);

        var data = bestMap.ReturnMapData();
        mapVisualizer.VisualizeMap(bestMap.grid, data, true);
        // Debug.Log("Path length: " + data.path);
        // Debug.Log("Corners Count: " + data.cornersList.Count);

        endDate = DateTime.Now;
        long elapsedTicks = endDate.Ticks - startDate.Ticks;
        TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
        // Debug.Log("Time needed to run this genetic optimisation: " + elapsedSpan.TotalSeconds);
    }

    private void CrossoverParents(CandidateMap parent1, CandidateMap parent2, out CandidateMap child1, out CandidateMap child2)
    {
        child1 = parent1.DeepClone();
        child2 = parent2.DeepClone();

        if (Random.value < crossoverRatePercent)
        {
            int numBits = parent1.ObstaclesArray.Length;
            int crossoverIndex = Random.Range(0, numBits);

            for (int i = crossoverIndex; i < numBits; i++)
            {
                child1.PlaceObstacle(i, parent2.IsObstacleAt(i));
                child2.PlaceObstacle(i, parent1.IsObstacleAt(i));
            }
        }
    }

    private int RouletteWheelSelection()
    {
        int randomValue = Random.Range(0, totalFitnessThisGeneration);
        for (int i = 0; i < populationSize; i++)
        {
            randomValue -= CalculateFitness(currentGeneration[i].ReturnMapData());
            if (randomValue <= 0)
            {
                return i;
            }
        }
        return populationSize - 1;
    }

    private int CalculateFitness(MapData mapData)
    {
        int numberOfObstacles = mapData.obstacleArray.Where(isObstacle => isObstacle).Count();
        int score = mapData.path.Count * fitnessPathWeight + (int)(numberOfObstacles * fitnessObstacleWeight);
        int cornersCount = mapData.cornersList.Count;
        if (cornersCount >= fitnessCornerMin && cornersCount <= fitnessCornerMax)
            score += cornersCount * fitnessCornerWeight;
        else if (cornersCount > fitnessCornerMax)
            score -= fitnessCornerWeight * (cornersCount - fitnessCornerMax);
        else if (cornersCount < fitnessCornerMin)
            score -= fitnessCornerWeight * fitnessCornerMin;
        score -= mapData.cornersNearEachOther * fitnessNearcornerWeight;
        return score;
    }
}
