using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public enum Direction { Right, Left }

    public Direction direction = Direction.Right;
    public float speed = 20f;
    public float rightEdgeX = 100f;
    public float leftEdgeX = -80f;

    void Update()
    {
        if (RoadManager.Instance == null || !RoadManager.Instance.CarsMoving) return;

        float move = (direction == Direction.Right ? 1f : -1f) * speed * Time.deltaTime;
        transform.position += new Vector3(move, 0f, 0f);

        if (direction == Direction.Right && transform.position.x > rightEdgeX)
        {
            SnapX(leftEdgeX);
        }
        else if (direction == Direction.Left && transform.position.x < leftEdgeX)
        {
            SnapX(rightEdgeX);
        }
    }

    void SnapX(float x)
    {
        Vector3 pos = transform.position;
        pos.x = x;
        transform.position = pos;
    }
}