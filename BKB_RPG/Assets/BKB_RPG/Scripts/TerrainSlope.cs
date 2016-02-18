using UnityEngine;
using System.Collections.Generic;
using BKB_RPG;

[RequireComponent(typeof(Collider2D))]
public class TerrainSlope : MonoBehaviour {

    // TODO - 2nd script that locks player out of input while in

    public int gravityAngle = 180;
    [Range(0, 1)]
    public float speedModifier = 1;
    public float gravity = 0;

    HashSet<Mover> movers = new HashSet<Mover>();
    Collider2D myCollider;
    int myUID;
    float cachedSpeedModifier = 0;

    void Start() {
        myCollider = GetComponent<Collider2D>();
    }

    void Update() {
        Tick();
    }

    public void Setup() {
        myUID = this.GetInstanceID();
    }

    public void Tick() {
        // TODO - do less than once per frame?
        foreach (Mover mover in movers)
        {
            float modifier = 1f;
            // Track 'depth' entities via feet location
            Depth d = mover.GetComponent<Depth>();
            if (d != null)
            {
                Vector3 pos = mover.transform.position;
                pos.y += d.yOffset;
                pos.z = transform.position.z;
                if (!myCollider.bounds.Contains(pos))
                {
                    modifier = 1;
                    continue;
                }
            }
            if (mover.moving)
            {
                float angle = Mathf.Abs(mover.movementAngle - gravityAngle);
                // going with gravity
                if (angle <= 45)
                    modifier = 1f + speedModifier / 2f;
                // going against
                else if (angle >= 135)
                    modifier = 1f - speedModifier;
            }
            else if (gravity != 0)
            {
                Vector2 offset = Utils.AngleMagnitudeToVector2(gravityAngle, 1);
                modifier = gravity;
                mover.StepTowards(mover.transform.position + (Vector3)offset);
            }
            mover.speedModifier = modifier;
            print(mover.name);
        }
    }


    // TODO - handle depth at entity's feet
    void OnTriggerEnter2D(Collider2D other) {
        Mover m = other.GetComponent<Mover>();
        if (m != null && m.affectedBySlope)
        {
            movers.Add(m);
        }

    }

    void OnTriggerExit2D(Collider2D other) {
        Mover m = other.GetComponent<Mover>();
        if (m != null && movers.Contains(m))
        {
            m.speedModifier = 1;
            movers.Remove(m);
        }
    }

}
