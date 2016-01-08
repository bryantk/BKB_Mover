using UnityEngine;
using System.Collections.Generic;
using BKB_RPG;

/// <summary>
/// Responisble for managing and running all mover objects in the scene.
/// Call 'OnSceneLoad()' with each new scene to setup Movers.
/// Also handles default Mover preferences.
/// </summary>
public class MasterMover : MonoBehaviour {

    public static MasterMover _instance { get; private set; }
    public enum dirs {Free, Four, Eight};
	public dirs directions = dirs.Free;
	public float unitDistance = 1f;

    private List<Mover> movers;

	public virtual void Awake () {
		if (_instance == null) {
			_instance = this;
			DontDestroyOnLoad (this);
            OnAwake();
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

    void OnAwake() {
        OnSceneLoad();
    }

    public void OnSceneLoad() {
        movers = new List<Mover>(FindObjectsOfType<Mover>());
        foreach (Mover m in movers)
        {
            m.Setup();
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		foreach (Mover m in movers)
        {
           m.Tick();
        }
	}



}
