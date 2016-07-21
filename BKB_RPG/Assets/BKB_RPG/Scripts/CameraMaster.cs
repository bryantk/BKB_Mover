using UnityEngine;

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

    public void SetRotationScale(Vector3 scale) {
        shaker.RotationScale = scale;
    }

    public void SetShakeScale(Vector3 scale) {
        shaker.PositionScale = scale;
    }

    public void Shake(int power, float duration, Callback callback=null) {
        shaker.Shake(power, duration, callback);
    }

    public void Shake(int power, float duration, Vector3 scale, Callback callback = null) {
        SetShakeScale(scale);
        Shake(power, duration, callback);
    }

    public void Shake(int power, float duration, Vector3 scale, Vector3 rotation_Scale, Callback callback = null) {
        SetShakeScale(scale);
        SetRotationScale(rotation_Scale);
        Shake(power, duration, callback);
    }

    // Tint
    public void TintForced(Color toColor, float time = 0, Callback callback = null) {
        TintForced(toColor, time, callback);
    }

    public void Tint(Color fromColor, Color toColor, float time = 0, Callback callback = null) {
        tintFader.Tint(fromColor, toColor, time, callback);
    }

    public void Tint(Color toColor, float time = 0, Callback callback = null) {
        tintFader.Tint(toColor, time, callback);
    }

    public void FadeOut(float time, Callback callback = null) {
        tintFader.FadeOut(time, callback: callback);
    }

    public void FadeIn(float time, Callback callback = null) {
        tintFader.FadeIn(time, callback: callback);
    }

}
