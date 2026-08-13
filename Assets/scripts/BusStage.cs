using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// Put this on the PLAYER (the moving object with a Rigidbody/CharacterController).
// FLOW: step on a StartPlane -> instructions popup -> press G -> ray turns on ->
//       aim at a person, press C -> Suspect = right (+15), Innocent = wrong (-5) ->
//       press R to replay.
public class BusStage : MonoBehaviour
{
    [Header("Ray")]
    public Transform rayOrigin;        // drag your Camera; auto-uses Main Camera if empty
    public float rayLength = 4f;
    public float aimDownAngle = 0f;    // tilt ray down if it points too high

    [Header("Popups")]
    public GameObject instructionsPopup;
    public GameObject rightPopup;
    public GameObject wrongPopup;

    [Header("Scoring")]
    public GameUIManager ui;
    public int winPoints = 15;
    public int penaltyPoints = 5;

    [Header("Keys")]
    public KeyCode confrontKey = KeyCode.C;
    public KeyCode closeInstructionsKey = KeyCode.G;
    public KeyCode reloadKey = KeyCode.R;

    [Header("Ray line")]
    public bool showLine = true;
    public float lineWidth = 0.03f;

    [Header("Debug")]
    public bool debugLogs = true;

    private bool stageStarted;
    private bool rayActive;
    private bool resultOpen;
    private Transform aimed;
    private LineRenderer line;

    void Start()
    {
        if (instructionsPopup != null) instructionsPopup.SetActive(false);
        if (rightPopup != null) rightPopup.SetActive(false);
        if (wrongPopup != null) wrongPopup.SetActive(false);

        if (rayOrigin == null && Camera.main != null)
            rayOrigin = Camera.main.transform;

        if (showLine)
        {
            line = gameObject.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.useWorldSpace = true;
            line.enabled = false;
        }
    }

    // The PLAYER detects stepping onto the plane.
    void OnTriggerEnter(Collider other)
    {
        if (stageStarted) return;

        StartPlane plane = other.GetComponentInParent<StartPlane>();
        if (plane == null) plane = other.GetComponentInChildren<StartPlane>();
        if (plane == null) return;   // not a start plane

        stageStarted = true;
        if (instructionsPopup != null) instructionsPopup.SetActive(true);
        if (debugLogs) Debug.Log("[BusStage] Stepped on plane -> instructions shown. Press " + closeInstructionsKey);
    }

    void Update()
    {
        // Result popup up: wait for reload
        if (resultOpen)
        {
            if (Input.GetKeyDown(reloadKey))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            if (line != null) line.enabled = false;
            return;
        }

        // Close instructions -> turn ray on
        if (instructionsPopup != null && instructionsPopup.activeSelf
            && Input.GetKeyDown(closeInstructionsKey))
        {
            instructionsPopup.SetActive(false);
            rayActive = true;
            if (debugLogs) Debug.Log("[BusStage] Instructions closed -> ray ON.");
        }

        if (!rayActive)
        {
            if (line != null) line.enabled = false;
            return;
        }

        AimRay();

        if (Input.GetKeyDown(confrontKey))
            TryConfront();
    }

    void AimRay()
    {
        if (rayOrigin == null) return;

        Vector3 start = rayOrigin.position;
        Vector3 dir = Quaternion.AngleAxis(aimDownAngle, rayOrigin.right) * rayOrigin.forward;
        Vector3 end = start + dir * rayLength;

        aimed = null;
        RaycastHit hit;
        if (Physics.Raycast(start, dir, out hit, rayLength))
        {
            end = hit.point;

            Suspect su = hit.collider.GetComponentInParent<Suspect>();
            if (su == null) su = hit.collider.GetComponentInChildren<Suspect>();
            if (su != null) aimed = su.transform;

            if (aimed == null)
            {
                Innocent inn = hit.collider.GetComponentInParent<Innocent>();
                if (inn == null) inn = hit.collider.GetComponentInChildren<Innocent>();
                if (inn != null) aimed = inn.transform;
            }
        }

        bool onPerson = aimed != null;
        Debug.DrawLine(start, end, onPerson ? Color.green : Color.red);

        if (line != null)
        {
            line.enabled = showLine;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            Color c = onPerson ? Color.green : Color.red;
            line.startColor = c;
            line.endColor = c;
        }
    }

    void TryConfront()
    {
        if (aimed == null)
        {
            if (debugLogs) Debug.Log("[BusStage] Pressed C but not aiming at anyone.");
            return;
        }

        bool correct = aimed.GetComponent<Suspect>() != null;
        if (debugLogs) Debug.Log($"[BusStage] Accused '{aimed.name}'. Correct = {correct}");

        FaceMe(aimed);
        resultOpen = true;

        if (correct)
        {
            if (ui != null) ui.AddPoints(winPoints);
            if (rightPopup != null) rightPopup.SetActive(true);
        }
        else
        {
            if (ui != null) ui.RemovePoints(penaltyPoints);
            if (wrongPopup != null) wrongPopup.SetActive(true);
        }
    }

    void FaceMe(Transform npc)
    {
        NavMeshAgent a = npc.GetComponent<NavMeshAgent>();
        if (a != null) a.updateRotation = false;

        Vector3 from = rayOrigin != null ? rayOrigin.position : transform.position;
        Vector3 dir = from - npc.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            npc.rotation = Quaternion.LookRotation(dir);
    }
}