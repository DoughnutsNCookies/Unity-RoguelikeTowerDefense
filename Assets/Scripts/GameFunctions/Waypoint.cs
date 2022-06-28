using UnityEngine;

public class Waypoint : MonoBehaviour
{
    void Awake()
    {
        gameObject.transform.SetParent(FindObjectOfType<Waypoints>().transform);
    }
}
