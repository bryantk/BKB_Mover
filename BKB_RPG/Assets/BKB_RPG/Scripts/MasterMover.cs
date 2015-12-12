using UnityEngine;
using System.Collections;
using BKB_RPG;

public class MasterMover : MonoBehaviour {

	public enum dirs {Free, Four, Eight};

	public static MasterMover _instance { get; private set; }

	public dirs directions = dirs.Free;
	public float unitDistance = 1f;

	public virtual void Awake () {
		if (_instance == null) {
			_instance = this;
			DontDestroyOnLoad (this);
		}
		else {
			Debug.LogWarning("MoverMaster already exists, deleting.");
			Destroy (this.gameObject);
			return;
		}
	}
	
	void OnDestroy() {
		if(_instance == this) _instance = null;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}



}
