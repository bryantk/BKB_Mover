#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BKB_RPG;

[CustomEditor(typeof(Player))]
public class Drawer_Player : Editor
{
    Player myScript;
	
	void OnEnable() {
		myScript = target as Player;
	}

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"),
            true, new GUILayoutOption[0]);
        GUILayout.Label("Paused: " + myScript.Paused);
    }
}
#endif