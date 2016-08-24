using UnityEngine;
using System.Collections.Generic;
using BKB_RPG;

[RequireComponent(typeof(Collider2D))]
public class BKBForces : MonoBehaviour, ITick {

    // TODO - 2nd script that locks player out of input while in
    [Tooltip("Direction force is applied.")]
    public int Angle = 180;
    [Tooltip("When moving against Angle (within 45 degrees) rate movement speed is multiplied by." +
        "0 = unimpeded flat, 1 = one way cliff. Less jarring than apllying a small constant force.")]
    [Range(0, 1)]
    public float slopeInclination = 0;
    [Tooltip("Strength of force. 50 = normal move speed.")]
    public float Force = 0;
    [Tooltip("Always apply force. Use for Wind, Conveyor Belts, etc.")]
    public bool ConstantForce = false;

    HashSet<Mover> movers = new HashSet<Mover>();
    Collider2D myCollider;

    void Start() {
        myCollider = GetComponent<Collider2D>();
    }

    void Update() {
        iTick();
    }

    public void Setup() {
        
    }

    public void iTick() {
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
                float angle = Mathf.Abs(mover.movementAngle - Angle);
                // going with gravity
                if (angle <= 45)
                    modifier = 1f + slopeInclination / 2f;
                // going against
                else if (angle >= 135)
                    modifier = 1f - slopeInclination;
                mover.speedModifier = modifier;
            }
            if (Force != 0 && (ConstantForce || !mover.moving))
            {
                Vector2 offset = Utils.AngleMagnitudeToVector2(Angle);
                mover.StepTowards(mover.transform.position + (Vector3)offset, speedModifier: Force/1000);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        Mover m = other.GetComponent<Mover>();
        if (m != null && m.affectedBySlope)
            movers.Add(m);
    }

    void OnTriggerExit2D(Collider2D other) {
        Mover m = other.GetComponent<Mover>();
        if (m != null && movers.Contains(m))
        {
            m.speedModifier = 1f;
            movers.Remove(m);
        }
    }

    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "bkb_wind", true);
    }

}
