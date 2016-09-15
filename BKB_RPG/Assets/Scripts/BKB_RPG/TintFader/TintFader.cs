using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TintFader : MonoBehaviour
{
    public Material TransitionMaterialTint;
    public Material TransitionMaterial;
    public Material TransitionMaterialLetterBox;

    Callback callbackTint;
    Coroutine coroutineTint = null;
    Callback callbackTransition;
    Coroutine coroutineTransition = null;
    Callback callbackLetterBox;
    Coroutine coroutineLetterBox = null;

    Color currentColor = Color.clear;
    float letterboxCurrentCutoff;

    void OnRenderImage(RenderTexture src, RenderTexture dst) {
        if (TransitionMaterialTint != null && TransitionMaterialTint.GetFloat("_Cutoff") != 0)
        {
            Graphics.Blit(src, dst, TransitionMaterialTint);
            Graphics.Blit(dst, src);
        }
        if (TransitionMaterial != null && TransitionMaterial.GetFloat("_Cutoff") != 0) {
            Graphics.Blit(src, dst, TransitionMaterial);
            Graphics.Blit(dst, src);
        }
        if (TransitionMaterialLetterBox != null && TransitionMaterialLetterBox.GetFloat("_Cutoff") != 0)
            Graphics.Blit(src, dst, TransitionMaterialLetterBox); 
    }

    /// <summary>
    /// Draw black bars at top and bottom of the screen.
    /// </summary>
    /// <param name="enabled">True = turn on, False = turn off</param>
    /// <param name="time">Duration in Seconds to apply the letterbox.</param>
    /// <param name="maxCutoff"></param>
    /// <param name="callback"></param>
    public IEnumerator LetterBox(bool enabled=true, float time=0, Callback callback = null, float maxCutoff = 0.213f) {
        callbackLetterBox = callback;
        if (coroutineLetterBox != null)
            StopCoroutine(coroutineLetterBox);
        coroutineLetterBox = StartCoroutine(_Letterbox(enabled, time, maxCutoff));
        yield return new WaitForSeconds(time);
    }
    

    /// <summary>
    /// Fade out to black.
    /// </summary>
    /// <param name="time">Duration of fade in seconds.</param>
    /// <param name="t">Optional: Texture to use for wipe.</param>
    /// <param name="callback"></param>
    public IEnumerator FadeOut(float time, bool distort = false, Color? color = null, Texture2D t = null, Vector2? offset = null, Callback callback = null) {
        callbackTransition = callback;
        if (coroutineTransition != null)
            StopCoroutine(coroutineTransition);
        Color fadeColor = color == null ? Color.black : color.Value;
        if (t != null)
        {
            // Make sure there is an offset and extra texture to crop
            if (offset != null && offset.Value != Vector2.zero && t.width > 128)
            {
                int x = (int)(128 - 128 * Mathf.Clamp(offset.Value.x, -1f, 1f));
                int y = (int)(64 - 64 * Mathf.Clamp(offset.Value.y, -1f, 1f));
                Color[] pix = t.GetPixels(x, y, 256, 128);
                Texture2D tex = new Texture2D(256, 128);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.SetPixels(pix);
                tex.Apply();
                t = tex;
            }
            
            coroutineTransition = StartCoroutine(_Transition(true, time, distort, 1, t, color));
        }
        else
            coroutineTransition = StartCoroutine(_Tint(_2Tone(Color.clear, fadeColor), time, TransitionMaterial));
        yield return new WaitForSeconds(time);
    }

    /// <summary>
    /// Fade in.
    /// </summary>
    /// <param name="time">Duration of fade in seconds.</param>
    /// <param name="t">Optional: Texture to use for wipe.</param>
    /// <param name="callback"></param>
    public IEnumerator FadeIn(float time, bool distort=false, Texture2D t = null, Vector2? offset = null, Callback callback = null) {
        callbackTransition = callback;
        if (coroutineTransition != null)
            StopCoroutine(coroutineTransition);
        // Make sure there is an offset and extra texture to crop
        if (t != null && offset != null && t.width > 128)
        {
            print("Fade in");
            int x = (int)(128 - 128 * Mathf.Clamp(offset.Value.x, -1f, 1f));
            int y = (int)(64 - 64 * Mathf.Clamp(offset.Value.y, -1f, 1f));
            Color[] pix = t.GetPixels(x, y, 256, 128);
            Texture2D tex = new Texture2D(256, 128);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixels(pix);
            tex.Apply();
            t = tex;
        }
        coroutineTransition = StartCoroutine(_Transition(false, time, distort, t: t));
        yield return new WaitForSeconds(time);
    }

    /// <summary>
    /// Wipe to 'fromColor, then tint to 'toColor' over 'time' seconds.
    /// </summary>
    /// <param name="fromColor"></param>
    /// <param name="toColor">Target color</param>
    /// <param name="time">Transition over 'X' seconds</param>
    /// <param name="callback"></param>
    public IEnumerator Tint(Color fromColor, Color toColor, float time = 0, Callback callback = null) {
        callbackTint = callback;
        if (coroutineTint != null)
            StopCoroutine(coroutineTint);
        coroutineTint = StartCoroutine(_Tint(_2Tone(fromColor, toColor), time));
        yield return new WaitForSeconds(time);
    }

    /// <summary>
    /// Tint screen from current tint to 'toColor' over 'time' seconds.
    /// </summary>
    /// <param name="toColor">Target color</param>
    /// <param name="time">Transition over 'X' seconds</param>
    /// <param name="callback"></param>
    public IEnumerator Tint(Color toColor, float time = 0, Callback callback = null) {
        callbackTint = callback;
        if (coroutineTint != null)
            StopCoroutine(coroutineTint);
        coroutineTint = StartCoroutine(_Tint(_2Tone(currentColor, toColor), time));
        yield return new WaitForSeconds(time);
    }

    /// <summary>
    /// Tint screen according to gradient over 'time' seconds.
    /// </summary>
    /// <param name="g">Gradient for color tint. (Start with alpha 0 usualy)</param>
    /// <param name="time">Transition over 'X' seconds</param>
    /// <param name="callback"></param>
    public IEnumerator Tint(Gradient g, float time = 0, Callback callback = null) {
        callbackTint = callback;
        if (coroutineTint != null)
            StopCoroutine(coroutineTint);
        coroutineTint = StartCoroutine(_Tint(g, time));
        yield return new WaitForSeconds(time);
    }

    // the real guts
    public IEnumerator _Transition(bool fadeOut, float time, bool distort=false,
                                   float fade=1, Texture2D t=null, Color? color=null) {
        if (color == null)
            color = fadeOut ? Color.clear : Color.black;
        TransitionMaterial.SetColor("_Color", color.Value);
        TransitionMaterial.SetFloat("_Fade", fade);
        TransitionMaterial.SetFloat("_Distort", distort ? 1 : 0);
        float at = 0;
        float step = 1 / (time * (1 / Time.fixedDeltaTime));
        if (t != null)
            TransitionMaterial.SetTexture("_TransitionTex", t);
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

        if (callbackTransition != null)
            callbackTransition();
    }

    // Basicaly _Transition, but with the letterbox material and callback
    public IEnumerator _Letterbox(bool enabled, float time, float maxCutoff = 0.25f) {
        maxCutoff = enabled ? maxCutoff : 0;
        float step = Mathf.Abs(letterboxCurrentCutoff - maxCutoff) / (time * (1 / Time.fixedDeltaTime));
        print(maxCutoff+ "    t:  " + time + "     step: " + step);
        if (enabled)
        {
            if (time <= 0)
                letterboxCurrentCutoff = maxCutoff;
            do
            {
                letterboxCurrentCutoff = Mathf.Min(letterboxCurrentCutoff + step, maxCutoff);
                TransitionMaterialLetterBox.SetFloat("_Cutoff", letterboxCurrentCutoff);
                yield return new WaitForFixedUpdate();
            } while (letterboxCurrentCutoff < maxCutoff);
        }
        else
        {
            if (time <= 0)
                letterboxCurrentCutoff = 0;
            do
            {
                letterboxCurrentCutoff = Mathf.Max(letterboxCurrentCutoff - step, 0);
                TransitionMaterialLetterBox.SetFloat("_Cutoff", letterboxCurrentCutoff);
                yield return new WaitForFixedUpdate();
            } while (letterboxCurrentCutoff > 0);
        }
        if (callbackLetterBox != null)
            callbackLetterBox();

    }

    // the real guts
    public IEnumerator _Tint(Gradient gradient, float time, Material mat=null) {
        bool isTint = mat == null;
        if (mat == null)
            mat = TransitionMaterialTint;
        mat.SetFloat("_Cutoff", 1);
        float at = 0;
        float step = 0;
        if (time <= 0)
            at = 1;
        else
            step = 1 / (time * (1 / Time.fixedDeltaTime));
        do
        {
            at = Mathf.Min(at + step, 1);
            Color temp = gradient.Evaluate(at);
            _FloodFill(mat, temp);
            if (isTint)
                currentColor = temp;
            yield return new WaitForFixedUpdate();
        } while (at < 1);

        if (isTint)
        {
            if (callbackTint != null)
                callbackTint();
        }
        else
        {
            if (callbackTransition != null)
                callbackTransition();
        }
        
    }

    // Helpers
    // Fill the given material by color with alpha.
    void _FloodFill(Material mat, Color color) {
        mat.SetColor("_Color", color);
        mat.SetFloat("_Fade", color.a);
    }

    // Create a gradient from a single color. Start alpha at 0 to 1.
    Gradient _2Tone(Color toColor) {
        Color fromColor = toColor;
        fromColor.a = 0;
        return _2Tone(fromColor, toColor);
    }

    // 
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
