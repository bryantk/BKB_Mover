using UnityEngine;
using System.Collections;

public class CameraMaster : MonoBehaviour {

    public Transform target;

    public Shaker shaker;
    public TintFader tintFader;
    // shaker script
    // tint
    // some weather

    public void ReParent(Transform target) {
        float z = transform.position.z;
        transform.SetParent(target, true);
        Vector3 pos = Vector3.zero;
        pos.z = z;
        transform.localPosition = pos;
    }

}
