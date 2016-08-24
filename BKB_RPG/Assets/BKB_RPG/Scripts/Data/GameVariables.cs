using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

public class GameVariables {

    static Dictionary<string, float> floatDict;
    static Dictionary<string, string> stringDict;

	public static void iSetup(object parent) {
        floatDict = new Dictionary<string, float>();
        stringDict = new Dictionary<string, string>();
    }

    public static string iSave() {
        JSONNode N = JSON.Parse("{}");
        foreach (KeyValuePair<string, float> kv in floatDict)
            N["floats"][kv.Key] = kv.Value.ToString();
        foreach (KeyValuePair<string, string> kv in stringDict)
            N["strings"][kv.Key] = kv.Value;
        return N.ToString();
    }

    public static void iLoad(string json) {
        iSetup(null);
        JSONNode N = JSON.Parse(json);
        foreach (string key in N["floats"].Keys)
            floatDict.Add(key, N["floats"][key].AsFloat);
        foreach (string key in N["strings"].Keys)
            stringDict.Add(key, N["strings"][key]);
    }


    public static void SetFloat(string key, float value) {
        if (floatDict.ContainsKey(key))
            floatDict[key] = value;
        else
            floatDict.Add(key, value);
    }

    public static void SetBool(string key, bool value) {
        SetFloat(key, value ? 1 : 0);
    }

    public static float GetFloat(string key) {
        return floatDict.GetOrDefault(key, 0f);
    }

    public static bool GetBool(string key) {
        return GetFloat(key) == 0 ? false : true;
    }

    public static void SetString(string key, string value) {
        if (stringDict.ContainsKey(key))
            stringDict[key] = value;
        else
            stringDict.Add(key, value);
    }

    public static void SetColor(string key, Color value) {
        string color = value.r + "," + value.g + "," + value.b + "," + value.a;
        SetString(key, color);
    }

    public static string GetStringt(string key) {
        return stringDict.GetOrDefault(key, "");
    }

    public static Color GetColor(string key) {
        string colorString = GetStringt(key);
        if (colorString == "")
            return Color.white;
        Color color = Color.white;
        string[] rgba = colorString.Split(',');
        color.r = float.Parse(rgba[0]);
        color.g = float.Parse(rgba[1]);
        color.b = float.Parse(rgba[2]);
        color.a = float.Parse(rgba[3]);
        return color;
    }

}
