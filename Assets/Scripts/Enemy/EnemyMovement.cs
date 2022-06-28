using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyMovement : MonoBehaviour
{
	private Transform target;
    private Enemy enemy;
    private List<Transform> waypointsCopy;

    void Start()
	{
        waypointsCopy = Waypoints.waypoints;
        enemy = GetComponent<Enemy>();
        GetNextNearestWaypoint();
	}

    void Update()
    {
        Vector3 direction = target.position - transform.position;
        transform.Translate(direction.normalized * enemy.speed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, target.position) <= 0.1f)
            GetNextNearestWaypoint();
        enemy.speed = enemy.startSpeed;
    }

    void GetNextNearestWaypoint()
    {
        float shortestDistance = Mathf.Infinity;
        float distance = 0f;
        int shortestDistanceIndex = 0;

        if (waypointsCopy.Count <= 0)
        {
            EndPath();
            return;
        }
        for (int i = 0; i < waypointsCopy.Count; i++)
        {
            distance = Vector3.Distance(transform.position, waypointsCopy[i].position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                shortestDistanceIndex = i;
            }
        }
        target = waypointsCopy[shortestDistanceIndex];
        waypointsCopy.Remove(waypointsCopy[shortestDistanceIndex]);
    }

    void EndPath()
    {
        // PlayerStats.Lives--;
        // WaveSpawner.enemiesAlive--;
        Destroy(gameObject);
    }
}
