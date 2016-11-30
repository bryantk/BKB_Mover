#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using BKB_RPG;

[CustomEditor(typeof(Depth))]
public class Drawer_Depth : Editor
{
	Depth myScript;
	
	void OnEnable() {
		myScript = target as Depth;
	}
	
	void OnSceneGUI() {
        if (!myScript.enabled)
            return;
        // TODO - set static / dynamic?
		Vector3 location = myScript.transform.position;
		location.y += myScript.yOffset;
		location = Handles.FreeMoveHandle(location, Quaternion.identity, 0.5f,
		                                         Vector3.one, Handles.SphereCap);
		Handles.DrawLine(location - Vector3.right * 2, location + Vector3.right * 2);
		myScript.yOffset = location.y - myScript.transform.position.y;
        myScript.DrawDepth();
	}
}
#endif