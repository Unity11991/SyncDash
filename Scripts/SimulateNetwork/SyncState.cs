using UnityEngine;

[System.Serializable]
public struct SyncState {
    public float time;
    public Vector3 position;
    public Quaternion rotation;

    public int laneIndex;
    public bool grounded;
    public bool crashed;

    public SyncState(float time, Vector3 position, Quaternion rotation,
                     int laneIndex, bool grounded, bool crashed) {
        this.time = time;
        this.position = position;
        this.rotation = rotation;
        this.laneIndex = laneIndex;
        this.grounded = grounded;
        this.crashed = crashed;
    }
}

