using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public float speed = 20f;
    public float rightEdgeX = 100f;
    public float leftEdgeX = -80f;

    void Update()
    {
        if (RoadManager.Instance == null || !RoadManager.Instance.CarsMoving) return;

        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        float dir = Mathf.Sign(transform.forward.x);

        if (dir > 0 && transform.position.x > rightEdgeX)
        {
            SnapX(leftEdgeX);
        }
        else if (dir < 0 && transform.position.x < leftEdgeX)
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