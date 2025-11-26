using UnityEngine;

public class Obstacle : MonoBehaviour {
    [SerializeField] private int m_Damage;
    [SerializeField] private int m_points;
    [SerializeField] private float m_MovementSpeed = 5f;
    [SerializeField] private float m_LineChangeInterval = 2f;

    private float m_LineChangeTimer = 0f;
    private Vector3 m_TargetPosition;

    // NEW: Used to identify which pool this object belongs to
    public string PoolKey { get; set; }

    public int Damage => m_Damage;
    public int Point => m_points;

    // We use OnEnable because it runs every time the object is reused from the pool
    void OnEnable() {
        ResetState();
    }

    // Reset all values to default so the old "life" doesn't affect the new one
    public void ResetState() {
        m_TargetPosition = transform.position;
        m_LineChangeTimer = Time.time + m_LineChangeInterval;
        // Ensure rotation is reset if your game ever rotates them
        transform.rotation = Quaternion.identity;
    }

    void Update() {
        // Move forward in -Z
        transform.position += Vector3.back * m_MovementSpeed * Time.deltaTime;

        // Line changing logic
        if (Time.time >= m_LineChangeTimer) {
            m_LineChangeTimer = Time.time + m_LineChangeInterval;

            float newX = transform.position.x + Random.Range(-3f, 3f);
            m_TargetPosition = new Vector3(newX, transform.position.y, transform.position.z);
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            m_TargetPosition,
            3f * Time.deltaTime
        );
    }
}

//using UnityEngine;

//public class Obstacle : MonoBehaviour {
//    [SerializeField] private int m_Damage;
//    [SerializeField] private int m_points;
//    [SerializeField] private float m_MovementSpeed = 5f;
//    [SerializeField] private float m_LineChangeInterval = 2f;

//    private float m_LineChangeTimer = 0f;
//    private Vector3 m_TargetPosition;

//    public int Damage => m_Damage;
//    public int Point => m_points;
//    void Start() {
//        // IMPORTANT: Keeps the obstacle at its spawn lane
//        m_TargetPosition = transform.position;
//    }

//    void Update() {
//        // Move forward in -Z
//        transform.position += Vector3.back * m_MovementSpeed * Time.deltaTime;

//        // If you change lanes, update m_TargetPosition instead of going to zero
//        // Example:
//        if (Time.time >= m_LineChangeTimer) {
//            m_LineChangeTimer = Time.time + m_LineChangeInterval;

//            // Move left/right only, keep Z
//            float newX = transform.position.x + Random.Range(-3f, 3f);
//            m_TargetPosition = new Vector3(newX, transform.position.y, transform.position.z);
//        }

//        // Smooth lane move
//        transform.position = Vector3.MoveTowards(
//            transform.position,
//            m_TargetPosition,
//            3f * Time.deltaTime
//        );
//    }
//}

