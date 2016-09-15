using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections.Generic;

public class StringParser : MonoBehaviour, ISetup {

    public Evaluator jsEval;
    public string test;

    const string symbols = "+-*/<>=&|!";
	
    public void iSetup(object parent) {
        jsEval = GetComponent<Evaluator>();
    }

    public bool EvaluateBool(string str) {
        //return true;
        // TODO
        return (bool)jsEval.Evaluate(ParseString(str));
    }

    public float EvaluateFloat(string str) {
        return 1;
        // TODO
        return (float)jsEval.Evaluate(ParseString(str));
    }

    public string ParseString(string str) {
        str = str.ToLower();
        StringBuilder sb = new StringBuilder();
        List<string> results = new List<string>();
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            
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
                results[i] = "123";
            }
        }
        return (string.Join("", results.ToArray()));
    }

    bool IsFloat(string str) {
        float f;
        return float.TryParse(str, out f);
    }

	// Update is called once per frame
	void Update () {
	
	}
}
