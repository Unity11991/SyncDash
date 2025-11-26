using UnityEngine;

public class SyncManager : MonoBehaviour {
    public float simulatedLag = 0.15f;
    public int bufferSize = 256;

    private SyncBuffer buffer;

    void Awake() {
        buffer = new SyncBuffer(bufferSize);
    }

    public void RecordState(SyncState s) {
        buffer.Push(s);
    }

    public bool TryGetState(out SyncState s) {
        float target = Time.time - simulatedLag;

        if (buffer.TryGetInterpolated(target, out SyncState a, out SyncState b, out float t)) {
            Vector3 pos = Vector3.Lerp(a.position, b.position, t);
            Quaternion rot = Quaternion.Slerp(a.rotation, b.rotation, t);

            // because player snaps lanes, we don't interpolate laneIndex
            int laneIndex = a.laneIndex;

            bool grounded = a.grounded || b.grounded;
            bool crashed = a.crashed || b.crashed;

            s = new SyncState(
                target,
                pos,
                rot,
                laneIndex,
                grounded,
                crashed
            );

            return true;
        }

        s = default;
        return false;
    }

    public void RecordCrash(Transform player) {
        // NOTE: laneIndex will be overwritten by next PlayerSyncAdapter sample,
        // so here we just use 0 temporarily.
        int laneIndex = 0;

        SyncState s = new SyncState(
            Time.time,
            player.position,
            player.rotation,
            laneIndex,
            true,
            true
        );

        RecordState(s);
    }
}
