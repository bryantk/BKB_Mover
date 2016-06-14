using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SimpleBlit : MonoBehaviour
{
    public Material TransitionMaterial;
    Callback callback;
    Coroutine coroutine = null;
    Color currentColor = Color.clear;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (TransitionMaterial != null)
            Graphics.Blit(src, dst, TransitionMaterial);
    }

    IEnumerator Start() {
        bool wait = true;
        Transition(1, true, 0.5f, null, Color.green, () => { wait = false; });
        while (wait)
            yield return null;
        wait = true;
        yield return new WaitForSeconds(0.5f);
        Transition(1, false, 1, null, Color.black, () => { wait = false; });
        while (wait)
            yield return null;
        wait = true;
        TintFromCurrent(Color.clear, 3, () => { wait = false; });
        while (wait)
            yield return null;
        print("Done");
    }

    public void Transition(float time = 1, bool fadeOut=true, float fade=1, Texture t = null, Color? color=null, Callback callback = null) {
        this.callback = callback;
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(_Transition(fadeOut, time, fade, t, color));
    }

    public void Tint(Color toColor, float time = 0, Callback callback = null) {
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
    public IEnumerator _Transition(bool fadeOut, float time, float fade, Texture t=null, Color? color=null) {
        if (color != null)
            TransitionMaterial.SetColor("_Color", color.Value);
        TransitionMaterial.SetFloat("_Fade", fade);
        if (t != null)
            TransitionMaterial.SetTexture("_TransitionTex", t);
        float at = 0;
        float step = 1 / (time * (1 / Time.fixedDeltaTime));
        if (fadeOut)
        {
            if (time <= 0)
                at = 1;
            do
            {
                at = Mathf.Min(at + step, 1);
                TransitionMaterial.SetFloat("_Cutoff", at);
                yield return new WaitForFixedUpdate();
            } while (at < 1);
        }
        else
        {
            at = 1;
            if (time <= 0)
                at = 0;
            do
            {
                at = Mathf.Max(at - step, 0);
                TransitionMaterial.SetFloat("_Cutoff", at);
                yield return new WaitForFixedUpdate();
            } while (at > 0);
        }

        if (callback != null)
            callback();
    }

    // the real guts
    public IEnumerator _Tint(Gradient gradient, float time) {
        TransitionMaterial.SetFloat("_Cutoff", 1);
        float at = 0;
        float step = 0;
        if (time <= 0)
            at = 1;
        else
            step = 1 / (time * (1 / Time.fixedDeltaTime));
        do
        {
            at = Mathf.Min(at + step, 1);
            currentColor = gradient.Evaluate(at);
            _FloodFill(currentColor);
            yield return new WaitForFixedUpdate();
        } while (at < 1);

        if (callback != null)
            callback();
    }

    // Helpers
    void _FloodFill(Color color) {
        TransitionMaterial.SetColor("_Color", color);
        TransitionMaterial.SetFloat("_Fade", color.a);
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
