using UnityEngine;

public class PlayerSyncAdapter : MonoBehaviour {
    public SyncManager syncManager;
    public Transform player;

    public float sampleRate = 0.02f;

    private float nextSample = 0f;
    private float groundY;

    private float[] playerLanes;
    private int currentLaneIndex;

    void Start() {
        if (player == null)
            player = transform;

        groundY = player.position.y;

        // Read lanes from your PlayerController (using moveStep)
        PlayerController pc = GetComponent<PlayerController>();
        float mid = player.position.x;

        playerLanes = new float[]
        {
            mid - pc.m_MoveStep,
            mid,
            mid + pc.m_MoveStep
        };
    }

    void Update() {
        if (Time.time >= nextSample) {
            nextSample = Time.time + sampleRate;
            Record();
        }
    }

    void DetectLane() {
        float px = player.position.x;

        float best = float.MaxValue;
        int bestIndex = 1;

        for (int i = 0; i < playerLanes.Length; i++) {
            float diff = Mathf.Abs(px - playerLanes[i]);
            if (diff < best) {
                best = diff;
                bestIndex = i;
            }
        }

        currentLaneIndex = bestIndex;
    }

    void Record() {
        if (syncManager == null)
            return;

        DetectLane();

        bool grounded = Mathf.Abs(player.position.y - groundY) < 0.001f;

        SyncState s = new SyncState(
            Time.time,
            player.position,
            player.rotation,
            currentLaneIndex,   // NEW: lane index
            grounded,
            false               // crash synced from GameManager
        );

        syncManager.RecordState(s);
    }
}
