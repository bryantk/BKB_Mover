using UnityEngine;
using System.Collections.Generic;
using BKB_RPG;

public class LevelMaster : MonoBehaviour {

    // Simple data chache for Master use
    public Dictionary<string, Transform> labelDict = new Dictionary<string, Transform>();
    public bool loaded = false;

    public void GetInfo() {
        Transform label = gameObject.transform.FindChild("Labels");
        if (label != null)
        {
            labelDict.Clear();
            foreach (Transform child in label)
            {
                labelDict.Add(child.name, child);
            }
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif
        print("set up entities");
        EntityMaster em = GameMaster._instance.entityMaster;
        em.entities = new List<Entity>(GetComponentsInChildren<Entity>());
        em.OnSceneReady();

    }

    public void SetupLevel() {
        GetInfo();
        gameObject.SetActive(true);
        // Get auto-run events?
        loaded = true;
    }

    public Vector3 GetLabel(string name) {
        if (!labelDict.ContainsKey(name))
        {
            Debug.LogWarning(string.Format("Label of name '{}' not found in scene.", name));
            return Vector3.zero;
        }
        return labelDict[name].position;
    }

}
