﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using BKB_RPG;

[CustomEditor(typeof(events_t))]
public class draw_events : Editor {
    public override void OnInspectorGUI() {
        // Follow this template
        serializedObject.Update();

        events_t myScript = (events_t)target;

        for (int i = 0; i < myScript.myEvents.Count; i++)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("myEvents").GetArrayElementAtIndex(i), new GUIContent("calls"), GUILayout.Width(275));
        }

        serializedObject.ApplyModifiedProperties();
        SceneView.RepaintAll();
    }
}