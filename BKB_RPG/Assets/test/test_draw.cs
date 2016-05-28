#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(test))]
public class test_draw : Editor {

    void OnSceneGUI () {
        test myScript = target as test;
        myScript.inner = Mathf.Clamp(myScript.inner, 0, myScript.outer);
        myScript.outer = Mathf.Clamp(myScript.outer, myScript.inner+0.1f, 20);
        Handles.color = new Color(1, 0, 0, 0.5f);
        HandlesExtensions.DrawSolidHollowDiscArc(myScript.transform.position, Vector3.forward, Vector3.right,
            myScript.end, ref myScript.inner, ref myScript.outer, HandlesExtensions.makeGradient(new Color[] { Color.red, Color.yellow }));
        Handles.color = Color.white;
        /*
        l = Handles.ScaleValueHandle(2,
                        myScript.transform.position,
                        myScript.transform.rotation,
                        1,
                        Handles.ConeCap,
                        1);
        */
        //v = Handles.Slider(go.transform.position, Vector3.back);
    }

}
#endif