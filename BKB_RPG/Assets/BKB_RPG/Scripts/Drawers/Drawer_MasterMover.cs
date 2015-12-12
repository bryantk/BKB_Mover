using UnityEngine;
using System.Collections;
using UnityEditor;
using BKB_RPG;

[CustomEditor(typeof(MasterMover))]
public class Drawer_MasterMover : Editor
{
	MasterMover myScript;
	
	void OnEnable() {
		myScript = target as MasterMover;
	}
	
	public override void OnInspectorGUI() {
		DrawDefaultInspector ();
		if(myScript.unitDistance <= 0)
			myScript.unitDistance = 1;

	}
}