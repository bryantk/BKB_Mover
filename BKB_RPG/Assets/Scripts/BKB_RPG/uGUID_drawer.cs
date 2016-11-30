using UnityEngine;
using UnityEditor;
using BKB_RPG;
using System.IO;
using System.Reflection;

[CustomEditor(typeof(uGUID))]
public class uGUID_drawer : Editor
{

    void OnEnable() {
        uGUID myScript = target as uGUID;

        PropertyInfo inspectorModeInfo =
        typeof(UnityEditor.SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

        UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(myScript.gameObject);

        inspectorModeInfo.SetValue(serializedObject, UnityEditor.InspectorMode.Debug, null);

        UnityEditor.SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");

        //Debug.Log ("found property: " + localIdProp.intValue);

        myScript.persistentID = localIdProp.intValue;
        //Important set the component to dirty so it won't be overriden from a prefab!
        UnityEditor.EditorUtility.SetDirty(this);

    }
}