using BKB_RPG;

namespace BKB_FSM {
    public class FSM_OnMap : FSMBase {

        public FSM_OnMap() : base(FSMState.OnMap){ }

        public override void OnTransitionIn(object o = null) {
            GameMaster._instance.playerData.playerEntity.EnterState();
        }

        public override void OnEnter(object o = null)
        {
            GameMaster._instance.playerData.playerEntity.EnterState();
        }

        public override void OnTransitionOut(object o = null)
        {
            GameMaster._instance.playerData.playerEntity.ExitState();
        }

        public override void OnExit(object o = null)
        {
            GameMaster._instance.playerData.playerEntity.ExitState();
        }

        
    }
}