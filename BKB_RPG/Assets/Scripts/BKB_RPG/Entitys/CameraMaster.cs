using UnityEngine;
using System.Collections;

public class CameraMaster : MonoBehaviour {

    public Transform target;

    public Shaker shaker;
    public TintFader tintFader;
    // shaker script
    // tint
    // some weather

    public void ReParent(Transform target)
    {
        this.target = target;
        float z = transform.position.z;
        transform.SetParent(target, true);
        Vector3 pos = Vector3.zero;
        pos.z = z;
        transform.localPosition = pos;
    }

    /// <summary>
    /// Returns transform's (X,Y) relative to screen center.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public Vector2 ScreenLocationVector(Transform t) {
        Vector3 pos = Camera.main.WorldToScreenPoint(t.position);
        Vector2 vec = Vector2.zero;
        vec.x = 2 * pos.x / Camera.main.pixelWidth - 1;
        vec.y = 2 * pos.y / Camera.main.pixelHeight - 1;
        return vec;
    }

}
