using UnityEngine;
using System.Collections;
using BKB_RPG;

public class tempTEst : MonoBehaviour {

	public void Go() {
        GameMaster.Shake(40, 0.15f, new Vector3(1, 0.5f, 0), callers);
        print("hit at " + Time.time);
    }

    public void callers() {
        print("end at " + Time.time);
    }

}
