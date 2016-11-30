using UnityEngine;
using BKB_RPG;

[RequireComponent(typeof(Collider2D))]
public class Teleporter : MonoBehaviour {

    public string label;
    public Vector3 position;

    
    public string scene;


    void OnTriggerEnter2D(Collider2D other) {
        Player p = other.GetComponent<Player>();
        if (p != null)
            return;
       Teleport();
    }

    public void Teleport() {
        scene = scene != "" ? scene : null;
        if (label.Contains("."))
        {
            GameMaster.Teleport(label);
            return;
        }
        if (label != "")
            GameMaster.Teleport(label, scene);
        else
            GameMaster.Teleport(position, scene);
    }

}
