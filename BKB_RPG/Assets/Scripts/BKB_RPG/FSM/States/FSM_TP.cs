using UnityEngine;
using BKB_RPG;

namespace BKB_FSM {
    /// <summary>
    /// State when player is being Teleported
    /// </summary>
    public class FSM_TP : FSMBase {

        public FSM_TP() : base(FSMState.TP){
        }
        public override void OnEnter(object o = null) {
            GameMaster.PauseAll();
            Debug.LogWarning("Entering TP State");
        }

        public override void OnExit(object o = null) {
            GameMaster.ResumeAll();
            Debug.LogWarning("Exit TP State");
        }
    }
}