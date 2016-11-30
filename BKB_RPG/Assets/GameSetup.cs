using UnityEngine;
using System.Collections;

public class GameSetup : MonoBehaviour {

    public string StartLabel;

	// Use this for initialization
	void Start () {
        BKB_RPG.GameMaster.Teleport(StartLabel);
	}

}
