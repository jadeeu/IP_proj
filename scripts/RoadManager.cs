using UnityEngine;

public class RoadManager : MonoBehaviour
{
    public static RoadManager Instance;
    public bool CarsMoving = true;

    void Awake()
    {
        Instance = this;
    }
}
