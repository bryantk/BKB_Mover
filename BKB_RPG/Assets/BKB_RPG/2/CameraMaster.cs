using UnityEngine;

public class CameraMaster : MonoBehaviour {

    public Transform target;

    public Shaker shaker;
    public TintFader tintFader;
    // shaker script
    // tint
    // some weather

    public void SetRotationScale(Vector3 scale) {
        shaker.RotationScale = scale;
    }

    public void SetShakeScale(Vector3 scale) {
        shaker.PositionScale = scale;
    }

    public void Shake(int power, float duration, Callback callback=null) {
        shaker.Shake(power, duration, callback);
    }

    public void ReParent(Transform target) {
        float z = transform.position.z;
        transform.SetParent(target, true);
        Vector3 pos = Vector3.zero;
        pos.z = z;
        transform.localPosition = pos;
    }
}
