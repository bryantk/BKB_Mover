using BKB_RPG;

namespace BKB_FSM {
    public class FSM_Vox : FSMBase {

        public FSM_Vox() : base(FSMState.Vox){
        }
        public override void OnEnter(object o = null) {
            GameMaster.PauseAll();
            // Call Vox?
        }

        public override void OnExit(object o = null) {
            //ensure Vox is dead
            GameMaster.ResumeAll();
        }
    }
}