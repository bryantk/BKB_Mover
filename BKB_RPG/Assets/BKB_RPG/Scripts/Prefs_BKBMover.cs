using UnityEngine;
using UnityEditor;
using System.IO;

public class Prefs_BKBMover
{
	// Have we loaded the prefs yet
	private static bool prefsLoaded = false;
	private static BKBMover_Data data;

	// The Preferences
	private static bool boolPreference = false;
	public static int directions = 0;
	public static float unitLength = 1;
	public static int pixelsToUnit = 32;

	[PreferenceItem("BKB_RPG Mover")]
	private static void CustomPreferencesGUI()
	{
		if (!prefsLoaded)
		{
			boolPreference = EditorPrefs.GetBool("BoolPreferenceKey", false);
			LoadData();
			prefsLoaded = true;
		}
		
		EditorGUILayout.LabelField("Version: 0.1");
		EditorGUILayout.Space();
		directions = EditorGUILayout.IntPopup("Directions: ", directions, new string[]{"Free", "4", "8"}, new int[]{0, 1, 2});
		unitLength = EditorGUILayout.FloatField("Unit lenght: ", unitLength);
		pixelsToUnit = EditorGUILayout.IntField("Pixels per Unit: ", pixelsToUnit);

		
		if (GUI.changed)
		{
			EditorPrefs.SetBool("BoolPreferenceKey", boolPreference);
			EditorApplication.delayCall += UpdateData;
		}
	}

	private static void LoadData() {
		data = Resources.Load("Assets/" + Application.productName + "_data.asset") as BKBMover_Data;
		if (data == null)
			return;
		directions = data.directions/4;
		unitLength = data.unitLength;
		pixelsToUnit = data.pixelsToUnit;
	}

	private static void UpdateData() {
		if (data == null)
			CreateData();
		data.directions = directions*4;
		data.unitLength = unitLength;
		data.pixelsToUnit = pixelsToUnit;
	}

	private static void CreateData() {
		data = ScriptableObject.CreateInstance<BKBMover_Data> ();	
		string assetPathAndName = "Assets/" + Application.productName + "_data.asset";	
		AssetDatabase.CreateAsset (data, assetPathAndName);
		AssetDatabase.SaveAssets ();
		AssetDatabase.Refresh();
	}

}