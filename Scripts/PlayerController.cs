
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField]
    private float m_RotationMultiplier = 1.0f;
    [SerializeField]
    private float m_MoveSpeed = 1.0f;
    [SerializeField]
    public float m_MoveStep;
    [SerializeField]
    private float m_MoveCooldown = 0.1f;
    [Header("Jumping")]
    [SerializeField]
    private float m_Gravity = -9.81f;
    [SerializeField]
    private float m_JumpForce = 10.0f;
    private float[] m_Lanes;
    private float m_TargetPositionX;
    private float m_NextAllowedMovement = 0f;
    private float m_VerticalVelocity;
    private Vector3 m_InitialPosition;
    private bool m_IsGrounded = true;
    private bool m_IsMovingHorizontaly = false;
    private float m_TargetRotationX;
    private float m_TargetRotationY;
    private GameManager m_GameManager;
    [SerializeField] private GameObject orbHitVFXPrefab;
    private Vector2 touchStartPos;
    private float minSwipeDistance = 50f;
    public AudioSource audioSrc;


    void Start() {
        m_GameManager = FindObjectOfType<GameManager>();
        float middleX = m_TargetPositionX = transform.position.x;
        m_Lanes = new float[] { middleX - m_MoveStep, middleX, middleX + m_MoveStep };
        m_InitialPosition = transform.position;
    }

    void Update() {
        HandleRotation();
        HandleMovement();
        HandleJumping();
        HandleSwipeControls();

        if (Mathf.Approximately(transform.position.x, m_TargetPositionX)) {
            m_IsMovingHorizontaly = false;
        }

        var newPosition = new Vector3(m_TargetPositionX, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, newPosition, m_MoveSpeed * Time.deltaTime);

    }

    void HandleSwipeControls() {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began) {
            touchStartPos = touch.position;
        } else if (touch.phase == TouchPhase.Ended) {
            Vector2 swipeDelta = touch.position - touchStartPos;

            if (swipeDelta.magnitude < minSwipeDistance)
                return;

            float x = Mathf.Abs(swipeDelta.x);
            float y = Mathf.Abs(swipeDelta.y);

            if (x > y) {
                // HORIZONTAL SWIPE
                if (swipeDelta.x > 0)
                    MoveToLane(+1);  // RIGHT
                else
                    MoveToLane(-1);  // LEFT
            } else {
                // VERTICAL SWIPE
                if (swipeDelta.y > 0) {
                    // JUMP
                    if (m_IsGrounded) {
                        m_VerticalVelocity = m_JumpForce;
                        m_IsGrounded = false;
                    }
                } else {
                    // SLIDE (Down swipe)
                    if (!m_IsGrounded) {
                        m_VerticalVelocity = m_Gravity / 3f;
                    }
                }
            }
        }
    }


    void HandleRotation() {
        float rotationAmount = m_RotationMultiplier * Time.deltaTime;

        if (m_IsMovingHorizontaly) {
            m_TargetRotationX = 0f;
            m_TargetRotationY += rotationAmount;
        } else {
            m_TargetRotationY = 0f;
            m_TargetRotationX += rotationAmount;
        }

        Quaternion targetRotation = Quaternion.Euler(m_TargetRotationX, m_TargetRotationY, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationAmount);
    }

    void HandleMovement() {
        if (Time.time >= m_NextAllowedMovement) {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
                MoveToLane(-1);
            } else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
                MoveToLane(+1);
            }
        }
    }

    void HandleJumping() {
        if (m_IsGrounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))) {
            m_VerticalVelocity = m_JumpForce;
            m_IsGrounded = false;
        } else if (!m_IsGrounded && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))) {
            m_VerticalVelocity = m_Gravity / 3;
        }

        // Applying Gravity
        m_VerticalVelocity += m_Gravity * Time.deltaTime;
        transform.position += new Vector3(0, m_VerticalVelocity * Time.deltaTime, 0);

        // Checking if we are grounded
        if (transform.position.y <= m_InitialPosition.y) {
            m_IsGrounded = true;
            m_VerticalVelocity = 0;
            transform.position = new Vector3(transform.position.x, m_InitialPosition.y, transform.position.z);
        }
    }

    void MoveToLane(int direction) {
        for (int i = 0; i < m_Lanes.Length; i++) {

            if (Mathf.Approximately(m_TargetPositionX, m_Lanes[i])) {
                int newIndex = Mathf.Clamp(i + direction, 0, m_Lanes.Length - 1);
                m_TargetPositionX = m_Lanes[newIndex];
                break;
            }
        }

        m_NextAllowedMovement = Time.time + m_MoveCooldown;
        m_IsMovingHorizontaly = true;
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
