using UnityEngine;
using System.Collections.Generic;

public class events_t : MonoBehaviour {

    public List<UnityEngine.Events.UnityEvent> myEvents;

    // Use this for initialization
    void Start () {
        myEvents.Add(new UnityEngine.Events.UnityEvent());
        myEvents.Add(new UnityEngine.Events.UnityEvent());
        print("setup");
        print(myEvents.Count);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}