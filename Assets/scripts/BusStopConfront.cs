using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// BUS-STOP version. Put this on the bus-stop PLAYER.
// The confront ray is OFF until the player steps on the trigger plane (see RayZoneTrigger).
// Aim at an NPC and press C to accuse. Stepping OFF the plane shows the Stage popup.
public class BusStopConfront : MonoBehaviour
{
    [Header("Where the ray shoots FROM (drag your Camera here)")]
    public Transform rayOrigin;

    [Header("How far the ray reaches (meters)")]
    public float rayLength = 4f;

    [Tooltip("Degrees to tilt the ray DOWN from the camera's forward.")]
    public float aimDownAngle = 0f;

    [Header("Popups (drag your UI panel GameObjects here)")]
    public GameObject caughtPopup;
    public GameObject wrongPopup;
    public GameObject stagePopup;      // shown when the player steps OFF the plane

    [Header("Scoring (drag your GameUIManager here)")]
    public GameUIManager ui;
    public int winPoints = 15;         // correct
    public int penaltyPoints = 5;      // wrong

    [Header("Objective shown after the correct catch")]
    [TextArea] public string nextObjective = "Head to the next stop.";

    [Tooltip("If ON, accusing the thief before he has stolen counts as WRONG.")]
    public bool requireStolen = true;

    [Header("Key to reload the scene after a popup")]
    public KeyCode reloadKey = KeyCode.R;

    [Header("Key to close the stage popup")]
    public KeyCode closeStageKey = KeyCode.G;

    [Header("Ray line")]
    public bool showLine = true;
    public float lineWidth = 0.03f;

    [Header("Debug logs")]
    public bool debugLogs = true;

    private bool popupOpen;
    private bool wasCorrect;           // true if the open popup is the "right person" one
    private bool rayActive;            // only true while standing on the plane
    private Transform aimedNpc;
    private LineRenderer line;

    void Start()
    {
        if (caughtPopup != null) caughtPopup.SetActive(false);
        if (wrongPopup  != null) wrongPopup.SetActive(false);
        if (stagePopup  != null) stagePopup.SetActive(false);

        if (rayOrigin == null && Camera.main != null)
            rayOrigin = Camera.main.transform;

        if (showLine)
            CreateLine();

        rayActive = false;             // stays off until player steps on the plane
    }

    void CreateLine()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.useWorldSpace = true;
        line.enabled = false;
    }

    // ---------- Called by RayZoneTrigger on the plane ----------

    // Turns the ray on and keeps it on for the rest of the game.
    public void ActivateRay()
    {
        rayActive = true;
        if (line != null) line.enabled = showLine;
        if (debugLogs) Debug.Log("[Confront] Ray activated (stays on until game ends).");
    }

    public void ShowStagePopup()
    {
        // Stepping on the plane completes the previous objective - hide it
        if (ui != null && ui.objectiveText != null)
            ui.objectiveText.gameObject.SetActive(false);

        if (stagePopup != null)
        {
            stagePopup.SetActive(true);
            if (debugLogs) Debug.Log("[BusStop] Stage popup shown.");
        }
        else
        {
            Debug.LogWarning("[BusStop] Stage Popup slot is EMPTY on BusStopConfront - drag your instructions panel in!");
        }
    }

    // ---------- Update ----------

    void Update()
    {
        // Close the stage popup with its key, whenever it's open
        if (stagePopup != null && stagePopup.activeSelf)
        {
            if (Input.GetKeyDown(closeStageKey))
            {
                stagePopup.SetActive(false);
                if (debugLogs) Debug.Log("[Confront] Stage popup closed.");
            }
        }

        if (popupOpen)
        {
            if (Input.GetKeyDown(reloadKey))
            {
                if (wasCorrect)
                {
                    // Right person: R closes the popup, turns the ray off,
                    // and advances the on-screen objective.
                    if (caughtPopup != null) caughtPopup.SetActive(false);
                    popupOpen = false;
                    rayActive = false;
                    if (line != null) line.enabled = false;
                    if (ui != null) ui.ShowObjective(nextObjective);
                    if (debugLogs) Debug.Log("[BusStop] Correct - ray off, next objective shown.");
                }
                else
                {
                    // Wrong person: R reloads the scene
                    if (debugLogs) Debug.Log("[BusStop] Reloading scene...");
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
            if (line != null) line.enabled = false;
            return;
        }

        // Ray is off until the player is on the plane
        if (!rayActive)
        {
            if (line != null) line.enabled = false;
            return;
        }

        AimRay();

        if (Input.GetKeyDown(KeyCode.C))
            TryAccuse();
    }

    void AimRay()
    {
        if (rayOrigin == null) return;

        Vector3 start = rayOrigin.position;
        Vector3 dir = Quaternion.AngleAxis(aimDownAngle, rayOrigin.right) * rayOrigin.forward;
        Vector3 end = start + dir * rayLength;

        aimedNpc = null;

        RaycastHit hit;
        if (Physics.Raycast(start, dir, out hit, rayLength))
        {
            end = hit.point;
            aimedNpc = FindConfrontable(hit.collider);
        }

        bool onNpc = aimedNpc != null;
        Debug.DrawLine(start, end, onNpc ? Color.green : Color.red);

        if (line != null)
        {
            line.enabled = showLine;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            Color c = onNpc ? Color.green : Color.red;
            line.startColor = c;
            line.endColor = c;
        }
    }

    // Returns the root transform of any confrontable NPC the collider belongs to:
    // a Suspect or Innocent marker (bus-stop stage), or a ThiefAI/ShopperAI (shop stage).
    Transform FindConfrontable(Collider col)
    {
        Suspect su = col.GetComponentInParent<Suspect>();
        if (su == null) su = col.GetComponentInChildren<Suspect>();
        if (su != null) return su.transform;

        ThiefAI t = col.GetComponentInParent<ThiefAI>();
        if (t == null) t = col.GetComponentInChildren<ThiefAI>();
        if (t != null) return t.transform;

        Innocent inn = col.GetComponentInParent<Innocent>();
        if (inn == null) inn = col.GetComponentInChildren<Innocent>();
        if (inn != null) return inn.transform;

        ShopperAI sh = col.GetComponentInParent<ShopperAI>();
        if (sh == null) sh = col.GetComponentInChildren<ShopperAI>();
        if (sh != null) return sh.transform;

        return null;
    }

    void TryAccuse()
    {
        if (aimedNpc == null)
        {
            if (debugLogs) Debug.Log("[Confront] Pressed C but not aiming at any NPC.");
            return;
        }

        // Correct if it's a Suspect marker, or a thief (respecting requireStolen).
        bool correct;
        if (aimedNpc.GetComponent<Suspect>() != null)
        {
            correct = true;
        }
        else if (aimedNpc.GetComponent<ThiefAI>() != null)
        {
            ThiefAI t = aimedNpc.GetComponent<ThiefAI>();
            correct = !requireStolen || t.HasStolen;
            if (!correct && debugLogs) Debug.Log("[Confront] Thief hasn't stolen yet -> WRONG.");
        }
        else
        {
            correct = false;   // Innocent marker or ShopperAI -> wrong person
        }

        if (debugLogs) Debug.Log($"[Confront] Accused '{aimedNpc.name}'. Correct = {correct}.");

        PauseEveryone();
        FacePlayer(aimedNpc);

        popupOpen = true;
        wasCorrect = correct;
        if (correct)
        {
            if (ui != null) ui.AddPoints(winPoints);
            if (caughtPopup != null) caughtPopup.SetActive(true);
            else Debug.LogWarning("[Confront] Caught Popup slot is empty!");
        }
        else
        {
            if (ui != null) ui.RemovePoints(penaltyPoints);
            if (wrongPopup != null) wrongPopup.SetActive(true);
            else Debug.LogWarning("[Confront] Wrong Popup slot is empty!");
        }
    }

    void PauseEveryone()
    {
        foreach (ThiefAI t in FindObjectsByType<ThiefAI>(FindObjectsSortMode.None))
        {
            t.StopAllCoroutines();
            NavMeshAgent a = t.GetComponent<NavMeshAgent>();
            if (a != null && a.isOnNavMesh) a.isStopped = true;
        }
        foreach (ShopperAI s in FindObjectsByType<ShopperAI>(FindObjectsSortMode.None))
        {
            s.StopAllCoroutines();
            NavMeshAgent a = s.GetComponent<NavMeshAgent>();
            if (a != null && a.isOnNavMesh) a.isStopped = true;
        }
    }

    void FacePlayer(Transform npc)
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