using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public enum Direction { Right, Left }

    public Direction direction = Direction.Right;
    public float speed = 20f;
    public float rightEdgeX = 100f;
    public float leftEdgeX = -80f;

    [Header("Traffic light")]
    public IntersectionLightController trafficLight;
    public float stopLineX;   // where the FIRST car in this lane stops
    public float carGap = 6f; // space kept behind the car in front

    void Update()
    {
        if (RoadManager.Instance == null || !RoadManager.Instance.CarsMoving) return;

        float dir = (direction == Direction.Right ? 1f : -1f);
        float newX = transform.position.x + dir * speed * Time.deltaTime;

        bool lightIsRed = trafficLight != null
            && trafficLight.CurrentState == IntersectionLightController.LightState.B;

        if (lightIsRed)
        {
            float limit = GetStopLimit();
            if (direction == Direction.Right && transform.position.x <= limit)
                newX = Mathf.Min(newX, limit);
            else if (direction == Direction.Left && transform.position.x >= limit)
                newX = Mathf.Max(newX, limit);
        }

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        if (direction == Direction.Right && transform.position.x > rightEdgeX)
            SnapX(leftEdgeX);
        else if (direction == Direction.Left && transform.position.x < leftEdgeX)
            SnapX(rightEdgeX);
    }

    // Stop at the line, OR behind the closest car ahead of me in my lane — whichever comes first
    float GetStopLimit()
    {
        float limit = stopLineX;

        foreach (CarMovement other in FindObjectsOfType<CarMovement>())
        {
            if (other == this || other.direction != direction) continue;
            if (Mathf.Abs(other.transform.position.z - transform.position.z) > 2f) continue; // not my lane

            float otherX = other.transform.position.x;

            if (direction == Direction.Right && otherX > transform.position.x && otherX <= stopLineX + 0.1f)
                limit = Mathf.Min(limit, otherX - carGap);
            else if (direction == Direction.Left && otherX < transform.position.x && otherX >= stopLineX - 0.1f)
                limit = Mathf.Max(limit, otherX + carGap);
        }
        return limit;
    }

    void SnapX(float x)
    {
        Vector3 pos = transform.position;
        pos.x = x;
        transform.position = pos;
    }

    void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        GameManager.Instance.CarHit();
    }
}
}