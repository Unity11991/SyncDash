using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    [Header("Containers")]
    [SerializeField] private Transform[] m_MovableContainers;
    [SerializeField] private Transform[] m_SpawnPositions;
    [SerializeField] private Transform[] m_DespawnPositions;

    [Header("Settings")]
    [SerializeField] private SpawnConfigSO m_SpawnConfigSO;
    [SerializeField] private float m_BaseSpeed = 1.0f;

    [Header("Player")]
    [SerializeField] private int m_PlayerLifeCount = 3;
    [SerializeField] private TextMeshProUGUI m_PlayerHealthText;
    [SerializeField] private TextMeshProUGUI m_Score;

    [SerializeField] private SyncManager m_SyncManager;

    private List<Transform[]> m_AllMovables = new();

    // NEW: Dictionary to hold a queue for each different type of Obstacle
    private Dictionary<string, Queue<Obstacle>> m_PoolDictionary = new Dictionary<string, Queue<Obstacle>>();

    // We still keep track of active spawns for the GarbageCollector logic
    private List<Obstacle> m_ActiveSpawns = new();
    private int m_PlayerCurrentLife;
    private int m_Points = 0;

    void Start() {
        // Collect children from each container
        foreach (var container in m_MovableContainers) {
            int count = container.childCount;
            Transform[] laneMovables = new Transform[count];

            for (int i = 0; i < count; i++)
                laneMovables[i] = container.GetChild(i);

            m_AllMovables.Add(laneMovables);
        }

        m_SpawnConfigSO.Init();
        m_PlayerCurrentLife = m_PlayerLifeCount;
        m_PlayerHealthText.text = $"{m_PlayerCurrentLife}/{m_PlayerLifeCount}";
    }

    void Update() {
        HandleMovables();
        HandleSpawning();
        GarbageCollector();
    }

    // --- POOLING LOGIC START ---

    // Gets an object from the pool, or creates one if the pool is empty
    public Obstacle SpawnFromPool(Obstacle prefab, Vector3 position, Quaternion rotation) {
        string key = prefab.name;

        // Create queue if it doesn't exist yet
        if (!m_PoolDictionary.ContainsKey(key)) {
            m_PoolDictionary[key] = new Queue<Obstacle>();
        }

        Obstacle objectToSpawn;

        if (m_PoolDictionary[key].Count > 0) {
            // Reuse old object
            objectToSpawn = m_PoolDictionary[key].Dequeue();
            objectToSpawn.gameObject.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
            // OnEnable in Obstacle.cs handles the state reset
        } else {
            // Pool is empty, instantiate new one
            objectToSpawn = Instantiate(prefab, position, rotation);
            objectToSpawn.PoolKey = key; // Assign key so we know where to return it
        }

        return objectToSpawn;
    }

    // Returns object to pool instead of Destroying
    public void ReturnToPool(Obstacle obj) {
        obj.gameObject.SetActive(false);

        // Ensure the queue exists
        if (!m_PoolDictionary.ContainsKey(obj.PoolKey)) {
            m_PoolDictionary[obj.PoolKey] = new Queue<Obstacle>();
        }

        m_PoolDictionary[obj.PoolKey].Enqueue(obj);
    }

    // --- POOLING LOGIC END ---

    private void HandleMovables() {
        for (int lane = 0; lane < m_AllMovables.Count; lane++) {
            var movables = m_AllMovables[lane];

            foreach (var movable in movables) {
                movable.position -= new Vector3(0, 0, Time.deltaTime * m_BaseSpeed);

                if (movable.position.z <= m_DespawnPositions[lane].position.z) {
                    movable.position = new Vector3(
                        movable.position.x,
                        movable.position.y,
                        m_SpawnPositions[lane].position.z
                    );
                }
            }
        }
    }

    private void HandleSpawning() {
        for (int lane = 0; lane < m_SpawnPositions.Length; lane++) {
            foreach (var spawnable in m_SpawnConfigSO.Spawnables) {
                // Modified: Spawnable checks time, returns Prefab. Manager handles Pooling.
                Obstacle prefabToSpawn = spawnable.TryGetPrefabToSpawn();

                if (prefabToSpawn != null) {
                    Obstacle newObj = SpawnFromPool(prefabToSpawn, ChooseSpawnPosition(lane), Quaternion.identity);
                    m_ActiveSpawns.Add(newObj);
                }
            }
        }
    }

    private Vector3 ChooseSpawnPosition(int lane) {
        var xPositions = new float[] { 21f, 18f, 15f, -26f, 23f, 29f };
        var yPositions = new float[] { 0.8f, 3f };

        float x = xPositions[Random.Range(0, xPositions.Length)];
        float y = yPositions[Random.Range(0, yPositions.Length)];
        float z = m_SpawnPositions[lane].position.z;

        return new Vector3(x, y, z);
    }

    private void GarbageCollector() {
        // Iterate backwards when removing from list to avoid index errors
        for (int i = m_ActiveSpawns.Count - 1; i >= 0; i--) {
            var spawn = m_ActiveSpawns[i];

            // Safety check in case it was destroyed elsewhere
            if (spawn == null) {
                m_ActiveSpawns.RemoveAt(i);
                continue;
            }

            foreach (var despawn in m_DespawnPositions) {
                if (spawn.transform.position.z <= despawn.position.z) {
                    m_ActiveSpawns.RemoveAt(i);
                    ReturnToPool(spawn); // UPGRADED: Return, don't Destroy
                    break;
                }
            }
        }
    }

    public void OnPlayerGotHit(Obstacle source, string value) {
        if (value == "Obstacle") {
            m_PlayerCurrentLife -= source.Damage;
            m_PlayerHealthText.text = $"{m_PlayerCurrentLife}/{m_PlayerLifeCount}";

            if (m_SyncManager != null)
                m_SyncManager.RecordCrash(source.transform);

            m_ActiveSpawns.Remove(source);
            ReturnToPool(source); // UPGRADED

            if (m_PlayerCurrentLife <= 0)
                SceneManager.LoadScene("GameOverScene");
        } else {
            m_Points += source.Point;
            m_Score.text = "Score " + m_Points;
            m_ActiveSpawns.Remove(source);
            ReturnToPool(source); // UPGRADED
        }
    }
}

