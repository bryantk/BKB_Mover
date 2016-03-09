using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InvertColliderBox))]
public class InvertColliderDrawer : Editor {

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Invert Collider"))
            ((InvertColliderBox)target).InvertCollider();
    }
}
