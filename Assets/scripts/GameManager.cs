using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void CarHit()
    {
        Debug.Log("Player was hit by a car!");
    }
}