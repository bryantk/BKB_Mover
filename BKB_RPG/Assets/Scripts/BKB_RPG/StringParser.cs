using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using BKB_RPG;

public class StringParser : MonoBehaviour, ISetup {

    public Evaluator jsEval;
    public string test;

    const string symbols = "+-*/<>=&|!()";
	
    public void iSetup(object parent) {
        jsEval = GetComponent<Evaluator>();
    }

    public bool EvaluateBool(string str, object o = null) {
        string parsed = ParseString(str, o);
        if (parsed == "0")
            return false;
        if (parsed == "1")
            return true;
        print(parsed);
        return (bool)jsEval.Evaluate(parsed);
    }

    public float EvaluateFloat(string str, object o = null) {
        string parsed = ParseString(str, o);
        return (float)jsEval.Evaluate(parsed);
    }

    public string EvaluateString(string str, object o = null) {
        string parsed = ParseString(str, o);
        return (string)jsEval.Evaluate(parsed);
    }

    public string ParseString(string str, object o = null) {
        StringBuilder sb = new StringBuilder();
        List<string> results = new List<string>();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (c == ' ')
                continue;
            if (symbols.Contains(c))
            {
                if (sb.Length > 0)
                    results.Add(sb.ToString());
                results.Add(c.ToString());
                sb = new StringBuilder();
                continue;
            }
            sb.Append(c);
        }
        results.Add(sb.ToString());
        for (int i = 0; i < results.Count; i++)
        {
            //print(results[i]);
            if (results[i].Length == 1 && symbols.Contains(results[i]))
                continue;
            if (results[i].Contains('.') && !IsFloat(results[i]))
            {
                string[] parts = results[i].Split('.');
                // Send each pice to be evalueated/drill down
                switch (parts[0].ToLower())
                {
                case "global":
                case "g":
                    results[i] = Globals(parts[1], parts.Length > 2 ? parts[2] : null);
                    break;
                case "go":
                    results[i] = GetGameObject(parts[1], parts[2]);
                    break;
                case "entity":
                    results[i] = GetEntity(parts[1], parts[2]);
                    break;
                case "this":
                    results[i] = TransformData((GameObject)o, parts[1]);
                    break;
                }
            }
        }
        str = string.Join("", results.ToArray());
        return str;
    }

    bool IsFloat(string str) {
        float f;
        return float.TryParse(str, out f);
    }

    string Globals(string type, string key) {
        switch (type)
        {
        case "float":
        case "f":
            return GameVariables.GetFloat(key).ToString();
        case "bool":
        case "b":
            return GameVariables.GetBool(key).ToString();
        case "string":
        case "s":
            return '"' + GameVariables.GetString(key) + '"';
        default:
            string result = GameVariables.GetString(type);
            if (!string.IsNullOrEmpty(result))
                return '"' + result + '"';
            return GameVariables.GetFloat(type).ToString();
        }
    }

    string GetGameObject(string name, string type) {
        return TransformData(GameObject.Find(name), type);
    }

    string GetEntity(string name, string type) {
        GameObject g = null;
        foreach (var e in GameMaster._instance.entityMaster.entities)
        {
            if (e.name == name)
            {
                g = e.gameObject;
                break;
            }
        }
        return TransformData(g, type);
    }
    string TransformData(GameObject go, string type) {
        if (go == null)
            return "false";
        switch (type.ToLower())
        {
        case "x":
            return go.transform.position.x.ToString();
        case "y":
            return go.transform.position.y.ToString();
        case "z":
            return go.transform.position.z.ToString();
        }
        return "false";
    }

}
