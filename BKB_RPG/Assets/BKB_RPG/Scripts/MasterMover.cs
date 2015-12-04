using UnityEngine;
using System.Collections;

public class MasterMover : MonoBehaviour {

	public enum dirs {Free, Four, Eight};

	public static MasterMover _instance { get; private set; }
	[Range(0f, 1000f)]
	public float unitDistance = 1f;
	public dirs directions = dirs.Free;

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
