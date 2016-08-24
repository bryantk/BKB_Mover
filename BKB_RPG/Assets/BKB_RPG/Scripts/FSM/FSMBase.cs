namespace BKB_FSM {
    public abstract class FSMBase {

        protected FSMState state;

        public FSMBase(FSMState state) {
            this.state = state;
        }
        public virtual void OnEnter(object o = null) { }
        public virtual void OnExit(object o = null) { }
    }
}