using UnityEngine;
using UnityEditor;
using System.Reflection;

using self = HandlesExtensions;

// Original author: MadLittleMods

public class HandlesExtensions {

    static PropertyInfo handlesHandleWireMaterial_PropertyInfo;
    static MethodInfo handleUtilityApplyWireMaterial_MethodInfo;

    static HandlesExtensions() {
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        self.handleUtilityApplyWireMaterial_MethodInfo = typeof(HandleUtility).GetMethod("ApplyWireMaterial", bindingFlags);

        self.handlesHandleWireMaterial_PropertyInfo = typeof(HandleUtility).GetProperty("handleWireMaterial", bindingFlags);
    }

    public static Gradient makeGradient(Color[] colors) {
        Gradient g = new Gradient();
        if (colors.Length == 0)
            colors = new Color[] {Color.white, Color.white};
        if (colors.Length == 1)
            colors = new Color[] { colors[0], colors[0] };
        float step = 1f / (colors.Length-1);
        GradientColorKey[] gck = new GradientColorKey[colors.Length];
        GradientAlphaKey[] gak = new GradientAlphaKey[colors.Length];
        for (int i=0; i < colors.Length; i++) {
            float time = (float)i * step;
            gck[i].color = colors[i];
            gck[i].time = time;
            gak[i].alpha = colors[i].a;
            gak[i].time = time;
        }
        g.SetKeys(gck, gak);
        return g;
    }

    public static void DrawSolidHollowDisc(
        Vector3 position,
        Vector3 normal,
        Vector3 alongPlane,
        ref float innerRadius,
        ref float outerRadius,
        Gradient colors = null) {
        DrawSolidHollowDiscArc(position, normal, alongPlane, 360f, ref innerRadius, ref outerRadius, colors);
    }

    public static void DrawSolidHollowDiscArc(
        Vector3 position,
        Vector3 normal,
        Vector3 alongPlane,
        float angle,
        ref float innerRadius,
        ref float outerRadius,
        Gradient colors = null
    ) {
        if (Event.current.type != EventType.Repaint) {
            return;
        }

        Shader.SetGlobalColor("_HandleColor", Handles.color * new Color(1f, 1f, 1f, 0.5f));
        Shader.SetGlobalFloat("_HandleSize", 1f);
        if (self.handleUtilityApplyWireMaterial_MethodInfo != null) {
            self.handleUtilityApplyWireMaterial_MethodInfo.Invoke(null, null);
        }
        else if (handlesHandleWireMaterial_PropertyInfo != null) {
            ((Material)handlesHandleWireMaterial_PropertyInfo.GetValue(null, null)).SetPass(0);
        }
        GL.PushMatrix();
        GL.MultMatrix(Handles.matrix);
        GL.Begin(GL.TRIANGLES);
        alongPlane.Normalize();
        if (colors == null)
            colors = makeGradient(new Color[0]);
        int segments = colors.colorKeys.Length;
        segments = segments==1? 1 : (segments * 5);
        DrawArc(position, normal, alongPlane, innerRadius, outerRadius, colors, segments, angle);
        GL.End();
        GL.PopMatrix();
    }


    static void DrawArc(
        Vector3 position,
        Vector3 normal,
        Vector3 alongPlane,
        float innerRadius,
        float outerRadius,
        Gradient colors,
        int segments,
        float angle = 360
    ) {
        int numSamples = (int)(angle / 5);
        float step = (outerRadius - innerRadius) / segments;
        for (int i = 0; i < segments; i++) {
            Draw(position, normal, alongPlane, innerRadius + i * step, innerRadius + (i + 1) * step,
                colors.Evaluate((float)i / segments), colors.Evaluate((float)(i + 1) / segments), numSamples, angle);
        }
    }


    static void Draw(
        Vector3 position,
        Vector3 normal,
        Vector3 alongPlane,
        float innerRadius,
        float outerRadius,
        Color fromColor,
        Color toColor,
        int numSamples,
        float angle
    ) {
        Vector3 outerArcEdge = alongPlane * outerRadius;
        Vector3 innerArcEdge = alongPlane * innerRadius;
        for (int i = 0; i <= numSamples; i++) {
            Quaternion rotation = Quaternion.AngleAxis((((float)i) / numSamples) * angle, normal);
            Vector3 outerVertice = position + rotation * outerArcEdge;
            Vector3 innerVertice = position + rotation * innerArcEdge;

            Quaternion rotationNext = Quaternion.AngleAxis((Mathf.Clamp((float)i + 1, 0f, numSamples) / numSamples) * angle, normal);
            Vector3 outerVerticeNext = position + rotationNext * outerArcEdge;
            Vector3 innerVerticeNext = position + rotationNext * innerArcEdge;

            GL.Color(toColor * new Color(1, 1, 1, 0.5f));
            // 1____2
            //  |  /
            //  | /
            // 3|/
            GL.Vertex(outerVertice);
            GL.Vertex(outerVerticeNext);
            GL.Vertex(innerVertice);
            GL.Color(fromColor * new Color(1, 1, 1, 0.5f));
            //    /|2
            //   / |
            //  /  |
            // 1‾‾‾‾3
            GL.Vertex(innerVertice);
            GL.Vertex(outerVerticeNext);
            GL.Vertex(innerVerticeNext);
        }
    }

}