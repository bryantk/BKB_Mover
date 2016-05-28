using UnityEngine;
using System.Collections;


public class TintFader : MonoBehaviour {

    // TODO - set quad to camera size.

    MeshRenderer mesh;
    Texture2D tintTexture;

    Callback callback;
    Coroutine coroutine = null;
    Color currentColor = Color.clear;
    Color[] buffer;
    const int width = 4;
    const int hieght = 3;

	// Use this for initialization
	void Start () {
        tintTexture = new Texture2D(width, hieght, TextureFormat.ARGB32, false);
        tintTexture.wrapMode = TextureWrapMode.Clamp;
        buffer = new Color[width * hieght];

        mesh = GetComponent<MeshRenderer>();
        Renderer r = GetComponent<Renderer>();
        r.material.mainTexture = tintTexture;
        
    }

    public void Tint(Color toColor, float time = 0, Callback callback=null) {
        this.callback = callback;
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(_Tint(_2Tone(toColor), time));
    }

    public void Tint(Color fromColor, Color toColor, float time = 0, Callback callback = null) {
        this.callback = callback;
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(_Tint(_2Tone(fromColor, toColor), time));
    }

    public void TintFromCurrent(Color toColor, float time = 0, Callback callback = null) {
        this.callback = callback;
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(_Tint(_2Tone(currentColor, toColor), time));
    }

    // the real guts
    public IEnumerator _Tint(Gradient gradient, float time) {
        mesh.enabled = gradient.Evaluate(1).a != 0;
        float at = 0;
        float step = 0;
        if (time <= 0)
            at = 1;
        else
        {
            mesh.enabled = true;
            step = 1 / (time * (1 / Time.fixedDeltaTime));
        }
        do
        {
            at = Mathf.Min(at + step, 1);
            currentColor = gradient.Evaluate(at);
            _FloodFill(currentColor);
            yield return new WaitForFixedUpdate();
        } while (at < 1);
        
        mesh.enabled = currentColor.a != 0;
        if (callback != null)
            callback();
    }

    // Helpers
    void _FloodFill(Color color) {
        for (int x = 0; x < buffer.Length; x++)
        {
            buffer[x] = color;
        }
        // Right to Left, Bottom to Top
        tintTexture.SetPixels(buffer);
        tintTexture.Apply();
    }

    Gradient _2Tone(Color toColor) {
        Color fromColor = toColor;
        fromColor.a = 0;
        return _2Tone(fromColor, toColor);
    }

    Gradient _2Tone(Color fromColor, Color toColor) {
        Gradient g = new Gradient();
        GradientColorKey[] gck = new GradientColorKey[2];
        GradientAlphaKey[] gak = new GradientAlphaKey[2];
        gck[0].color = fromColor;
        gck[0].time = 0.0F;
        gck[1].color = toColor;
        gck[1].time = 1.0F;
        // Alpha
        gak[0].alpha = fromColor.a;
        gak[0].time = 0.0F;
        gak[1].alpha = toColor.a;
        gak[1].time = 1.0F;
        g.SetKeys(gck, gak);
        return g;
    }
}
