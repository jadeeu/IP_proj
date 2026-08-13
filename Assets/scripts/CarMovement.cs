using UnityEngine;
using System.Collections.Generic;

public class CarMovement : MonoBehaviour
{
    public enum Direction { Right, Left }

    public Direction direction = Direction.Right;
    public float speed = 20f;
    public float rightEdgeX = 100f;
    public float leftEdgeX = -80f;

    [Header("Traffic light")]
    public IntersectionLightController trafficLight;
    public float stopLineX;
    public float carGap = 6f;

    private static List<CarMovement> allCars = new List<CarMovement>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { allCars = new List<CarMovement>(); }

    void OnEnable() { allCars.Add(this); }
    void OnDisable() { allCars.Remove(this); }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Car " + name + " trigger hit by: " + other.name + " (tag: " + other.tag + ")");
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("It's the player! Calling CarHit()");
            GameManager.Instance.CarHit();
        }
    }
    void Update()
    {
        if (RoadManager.Instance == null || !RoadManager.Instance.CarsMoving) return;

        float dir = (direction == Direction.Right ? 1f : -1f);
        float newX = transform.position.x + dir * speed * Time.deltaTime;

        bool carsMustStop = trafficLight != null
            && trafficLight.CurrentState == IntersectionLightController.LightState.B;

        if (carsMustStop)
        {
            float limit = GetStopLimit();
            if (direction == Direction.Right && transform.position.x <= limit)
                newX = Mathf.Min(newX, limit);
            else if (direction == Direction.Left && transform.position.x >= limit)
                newX = Mathf.Max(newX, limit);
        }

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        if (direction == Direction.Right && transform.position.x > rightEdgeX)
            SnapToRespawn(leftEdgeX);
        else if (direction == Direction.Left && transform.position.x < leftEdgeX)
            SnapToRespawn(rightEdgeX);
    }

    float GetStopLimit()
    {
        float limit = stopLineX;

        foreach (CarMovement other in allCars)
        {
            if (other == null || other == this || other.direction != direction) continue;
            if (Mathf.Abs(other.transform.position.z - transform.position.z) > 2f) continue;

            float otherX = other.transform.position.x;

            if (direction == Direction.Right && otherX > transform.position.x && otherX <= stopLineX + 0.1f)
                limit = Mathf.Min(limit, otherX - carGap);
            else if (direction == Direction.Left && otherX < transform.position.x && otherX >= stopLineX - 0.1f)
                limit = Mathf.Max(limit, otherX + carGap);
        }
        return limit;
    }

    void SnapToRespawn(float baseX)
    {
        float respawnX = baseX;

        foreach (CarMovement other in allCars)
        {
            if (other == null || other == this || other.direction != direction) continue;
            if (Mathf.Abs(other.transform.position.z - transform.position.z) > 2f) continue;

            float otherX = other.transform.position.x;

            if (direction == Direction.Right)
            {
                if (otherX < respawnX + carGap)
                    respawnX = Mathf.Min(respawnX, otherX - carGap);
            }
            else
            {
                if (otherX > respawnX - carGap)
                    respawnX = Mathf.Max(respawnX, otherX + carGap);
            }
        }

        Vector3 pos = transform.position;
        pos.x = respawnX;
        transform.position = pos;
    }
}