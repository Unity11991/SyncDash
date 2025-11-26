using UnityEngine;

public class GhostController : MonoBehaviour {
    public SyncManager syncManager;

    public float positionSmooth = 10f;
    public float rotationSmooth = 10f;

    public float[] ghostLanes = { -7f, -4f, -1f }; // set these in inspector

    private Vector3 smoothPos;
    private Quaternion smoothRot;

    private bool initialized = false;
    private GameManager m_GameManager;
    [SerializeField] private GameObject orbHitVFXPrefab;
    public AudioSource audioSrc;
    private void Start() {
        m_GameManager = FindObjectOfType<GameManager>();
    }
    void Update() {
        if (syncManager == null)
            return;

        if (syncManager.TryGetState(out SyncState s)) {

            // map lane to ghost lane X
            float laneX = ghostLanes[s.laneIndex];

            if (!initialized) {
                smoothPos = new Vector3(laneX, s.position.y, s.position.z);
                smoothRot = s.rotation;

                transform.position = smoothPos;
                transform.rotation = smoothRot;

                initialized = true;
            }

            Vector3 targetPos = new Vector3(
                laneX,            // Ghost lane
                s.position.y,     // Jump
                s.position.z      // Forward movement
            );

            smoothPos = Vector3.Lerp(
                smoothPos,
                targetPos,
                1f - Mathf.Exp(-positionSmooth * Time.deltaTime)
            );

            smoothRot = Quaternion.Slerp(
                smoothRot,
                s.rotation,
                1f - Mathf.Exp(-rotationSmooth * Time.deltaTime)
            );

            transform.position = smoothPos;
            transform.rotation = smoothRot;

            if (s.crashed)
                Debug.Log("Ghost crashed");
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Obstacle") {
            if (other.TryGetComponent<Obstacle>(out var obstacle)) {
                m_GameManager.OnPlayerGotHit(obstacle, "Obstacle");
            }
        } else {
            if (other.TryGetComponent<Obstacle>(out var obstacle)) {
                // Spawn VFX at orb position
                SpawnOrbHitVFX(other.transform.position);

                m_GameManager.OnPlayerGotHit(obstacle, "Orbs");
            }
        }
    }

    void SpawnOrbHitVFX(Vector3 position) {
        if (orbHitVFXPrefab != null) {
            GameObject vfx = Instantiate(orbHitVFXPrefab, position, Quaternion.identity);

            // Destroy automatically when particle system finishes
            ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
            audioSrc.Play();
            if (ps != null) {
                Destroy(vfx, ps.main.duration + ps.main.startLifetime.constantMax);
            } else {
                // fallback destroy
                Destroy(vfx, 2f);
            }
        }
    }
}
