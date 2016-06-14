#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BKB_RPG;

[CustomEditor(typeof(LevelMaster))]
public class Drawer_LevelMaster : Editor {
    LevelMaster myScript;
    bool showLabels = false;
    Vector2 LScroll = Vector2.zero;

    void OnEnable() {
        myScript = target as LevelMaster;
        EditorApplication.hierarchyWindowChanged += ManualUpdate;
        LScroll = Vector2.zero;
    }

    void OnDestroy() {
        EditorApplication.hierarchyWindowChanged -= ManualUpdate;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"),
            true, new GUILayoutOption[0]);
        GUILayout.Space(6);
        showLabels = EditorGUILayout.Foldout(showLabels, "Labels in Scene: " + myScript.labelDict.Count);
        if (showLabels)
        {
            LScroll = EditorGUILayout.BeginScrollView(LScroll, GUILayout.Height(Mathf.Min(200, myScript.labelDict.Count * 22)));
            foreach (string s in myScript.labelDict.Keys)
            {
                if (GUILayout.Button(s, GUILayout.Width(200)))
                    Selection.activeGameObject = myScript.gameObject.transform.FindChild("Labels").FindChild(s).gameObject;
            }
            EditorGUILayout.EndScrollView();
        }
        GUILayout.Space(8);
        if (GUILayout.Button("Manual Update", GUILayout.Width(150)))
        {
            Debug.Log("getting info");
            ManualUpdate();
        }
    }


    void ManualUpdate() {
        string scene = EditorApplication.currentScene;
        scene = scene.Substring(scene.LastIndexOf('/')+1);
        scene = scene.Substring(0, scene.Length-6);
        myScript.gameObject.name = "Level_" + scene;
        myScript.GetInfo();
        // save all scene LABELs in JSON for human correction/help. TP to X - dear user, X does not exist in the scene...
    }

}
#endif