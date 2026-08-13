using UnityEngine;

public class NPC : MonoBehaviour
{
    [Tooltip("Check this in the Inspector if this NPC is the guilty suspect.")]
    public bool isSuspicious = false;

    [HideInInspector]
    public bool hasBeenConfronted = false;
}