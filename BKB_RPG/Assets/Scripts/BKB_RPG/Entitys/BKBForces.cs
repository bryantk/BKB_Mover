using UnityEngine;
using System.Collections.Generic;
using BKB_RPG;
using JetBrains.Annotations;

[RequireComponent(typeof(Collider2D))]
public class BKBForces : MonoBehaviour {

    // TODO - 2nd script that locks player out of input while in
    [Tooltip("Direction force is applied.")]
    public Vector2 Direction = Vector2.down;
    [Tooltip("Strength of force. 50 = normal move speed.")]
    public float Force = 0;
    [Tooltip("Always apply force. Use for Wind, Conveyor Belts, etc.")]
    public bool ConstantForce = false;

    public bool LockOutPlayerInput = false;

    HashSet<Mover> movers = new HashSet<Mover>();

    void Start() {
        Direction = Direction.normalized * Force;
        ConstantForce = ConstantForce || LockOutPlayerInput;
    }
 
    void OnTriggerEnter2D(Collider2D other) {
        Mover m = other.GetComponent<Mover>();
        if (m != null && m.affectedBySlope && !movers.Contains(m))
        {
            movers.Add(m);
            if (ConstantForce)
                m.constantForces += Direction;
            else
                m.forces += Direction;
            if (LockOutPlayerInput && other.tag == "Player")
            {
                InputMaster.DisablePlayerInput();
                m.SetFacing(Utils.Vector2toAngle(Direction));
                m.Stop();
            }
                
        }
        
            
    }

    void OnTriggerExit2D(Collider2D other) {
        Mover m = other.GetComponent<Mover>();
        if (m != null && movers.Contains(m))
        {
            if (ConstantForce)
                m.constantForces -= Direction;
            else
                m.forces -= Direction;
            movers.Remove(m);
            if (LockOutPlayerInput && other.tag == "Player")
                InputMaster.EnablePlayerInput();
        }
    }

    void OnDrawGizmos() {
        Gizmos.DrawIcon(transform.position, "bkb_wind", true);
    }

}
