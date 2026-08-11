using UnityEngine;
using UnityEngine.AI;

public class ProceduralLegs : MonoBehaviour
{
    [Header("Leg Bone References")]
    public Transform leftLeg;
    public Transform rightLeg;

    [Header("Procedural Settings")]
    public float swingSpeed = 10f;  // How fast the legs swing
    public float swingAngle = 25f;  // How far forward/back the legs swing

    private NavMeshAgent agent;
    private Quaternion leftLegInitialRot;
    private Quaternion rightLegInitialRot;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Store the resting pose of the legs
        if (leftLeg != null) leftLegInitialRot = leftLeg.localRotation;
        if (rightLeg != null) rightLegInitialRot = rightLeg.localRotation;
    }

    void LateUpdate()
    {
        // Only swing legs if the character is actually moving
        if (agent != null && agent.velocity.magnitude > 0.1f)
        {
            // Calculate back-and-forth angle using Sine wave
            float angle = Mathf.Sin(Time.time * swingSpeed) * swingAngle;

            // Rotate left leg forward/back, and right leg in the opposite direction
            if (leftLeg != null)
                leftLeg.localRotation = leftLegInitialRot * Quaternion.Euler(angle, 0, 0);

            if (rightLeg != null)
                rightLeg.localRotation = rightLegInitialRot * Quaternion.Euler(-angle, 0, 0);
        }
        else
        {
            // Reset legs to natural position when stopped
            if (leftLeg != null)
                leftLeg.localRotation = Quaternion.Slerp(leftLeg.localRotation, leftLegInitialRot, Time.deltaTime * 5f);
            
            if (rightLeg != null)
                rightLeg.localRotation = Quaternion.Slerp(rightLeg.localRotation, rightLegInitialRot, Time.deltaTime * 5f);
        }
    }
}