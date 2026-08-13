using UnityEngine;

public class PlayerHitDetector : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player trigger hit by: " + other.name + " (tag: " + other.tag + ")");
        
        if (other.GetComponent<CarMovement>() != null)
        {
            Debug.Log("It's a car! Calling CarHit()");
            GameManager.Instance.CarHit();
        }
    }
}