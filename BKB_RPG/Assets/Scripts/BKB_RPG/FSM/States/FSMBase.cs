namespace BKB_FSM {
    public abstract class FSMBase {

        public FSMState state;

        public FSMBase(FSMState state) {
            this.state = state;
        }
        public virtual void OnEnter(object o = null) { }
        public virtual void OnTransitionOut(object o = null) { }
        public virtual void OnExit(object o = null) { }
        public virtual void OnTransitionIn(object o = null) { }
    }
}