using UnityEngine;

public class TriggerTeleport : MonoBehaviour
{
    public Transform destination;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            
            if (cc != null)
            {
                cc.enabled = false;
                other.transform.position = destination.position;
                other.transform.rotation = destination.rotation;
                cc.enabled = true;
            }
            else
            {
                other.transform.position = destination.position;
                other.transform.rotation = destination.rotation;
            }
        }
    }
}