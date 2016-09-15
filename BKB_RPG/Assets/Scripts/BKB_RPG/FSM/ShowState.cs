using UnityEngine;
using System.Collections;

public class ShowState : MonoBehaviour {

    public BKB_FSM.FSMState state;

	// Update is called once per frame
	void Update () {
        state = BKB_FSM.StateManager.currentState.state;

    }
}
