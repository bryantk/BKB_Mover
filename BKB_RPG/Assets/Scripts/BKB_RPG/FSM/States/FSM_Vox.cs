using BKB_RPG;
using BKB_TEXT;

namespace BKB_FSM {
    public class FSM_Vox : FSMBase {

        public FSM_Vox() : base(FSMState.Vox){
        }

        public override void OnTransitionIn(object o = null)
        {
            GameMaster.PauseAll();
            DialougeDisplay.EnterState();
        }

        public override void OnEnter(object o = null) {
            GameMaster.PauseAll();
            DialougeDisplay.EnterState();
        }

        public override void OnTransitionOut(object o = null)
        {
            GameMaster.ResumeAll();
            DialougeDisplay.ExitState();
        }

        public override void OnExit(object o = null) {
            GameMaster.ResumeAll();
            DialougeDisplay.ExitState();
        }
    }
}