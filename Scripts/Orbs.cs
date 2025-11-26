using UnityEngine;

public class Orbs : MonoBehaviour {
    [SerializeField] private int m_Point;
    [SerializeField] private float m_MovementSpeed = 5f;
    [SerializeField] private float m_LineChangeInterval = 2f;

    private float m_LineChangeTimer = 0f;
    private Vector3 m_TargetPosition;

    public int Point => m_Point;

    void Start() {
        // IMPORTANT: Keeps the obstacle at its spawn lane
        m_TargetPosition = transform.position;
    }

    void Update() {
        // Move forward in -Z
        transform.position += Vector3.back * m_MovementSpeed * Time.deltaTime;

        // If you change lanes, update m_TargetPosition instead of going to zero
        // Example:
        if (Time.time >= m_LineChangeTimer) {
            m_LineChangeTimer = Time.time + m_LineChangeInterval;

            // Move left/right only, keep Z
            float newX = transform.position.x + Random.Range(-3f, 3f);
            m_TargetPosition = new Vector3(newX, transform.position.y, transform.position.z);
        }

        // Smooth lane move
        transform.position = Vector3.MoveTowards(
            transform.position,
            m_TargetPosition,
            3f * Time.deltaTime
        );
    }
}

