using UnityEngine;

public class SyncBuffer {
    private SyncState[] buffer;
    private int capacity;
    private int writeIndex = 0;

    public SyncBuffer(int capacity = 512) {
        this.capacity = Mathf.Max(8, capacity);
        buffer = new SyncState[this.capacity];
    }

    public void Push(SyncState state) {
        buffer[writeIndex] = state;
        writeIndex = (writeIndex + 1) % capacity;
    }

    // Finds two states that bracket targetTime and returns interpolation t [0..1]
    public bool TryGetInterpolated(float targetTime, out SyncState a, out SyncState b, out float t) {
        a = b = default;
        t = 0f;

        // Simple linear scan backwards from newest; fine because buffer size is moderate
        int idx = (writeIndex - 1 + capacity) % capacity;
        SyncState newest = buffer[idx];

        // if newest.time is zero then buffer is empty (not initialized)
        if (newest.time <= 0f)
            return false;

        // If target is newer than newest, clamp to newest
        if (targetTime >= newest.time) {
            a = b = newest;
            t = 0f;
            return true;
        }

        // Walk backwards to find the older entry <= targetTime
        for (int i = 0; i < capacity; i++) {
            SyncState cur = buffer[idx];
            // skip empty entries
            if (cur.time <= 0f) {
                idx = (idx - 1 + capacity) % capacity;
                continue;
            }

            if (cur.time <= targetTime) {
                a = cur;
                int nextIdx = (idx + 1) % capacity;
                b = buffer[nextIdx];
                // If b is empty or b.time <= a.time, return exact
                if (b.time <= a.time) {
                    t = 0f;
                    return true;
                }
                t = Mathf.InverseLerp(a.time, b.time, targetTime);
                return true;
            }

            idx = (idx - 1 + capacity) % capacity;
        }

        // fallback: return newest
        a = newest;
        b = newest;
        t = 0f;
        return true;
    }
}
