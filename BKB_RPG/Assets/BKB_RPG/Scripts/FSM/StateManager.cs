using System.Collections.Generic;

namespace BKB_FSM {

    public enum FSMState { None, OnMap, Vox, TP };

    public static class StateManager {
        private static Stack<FSMBase> stateQueue;
        public static FSMBase currentState = null;
        private static Dictionary<string, FSMBase> states;

        public static void iSetup(object o) {
            stateQueue = new Stack<FSMBase>();
            states = new Dictionary<string, FSMBase>();
            // Add states here
            states.Add("OnMap", new FSM_OnMap());
            states.Add("Vox", new FSM_Vox());
            states.Add("TP", new FSM_TP());

            // Push base state
            Push("OnMap");
        }

        public static void Push(string targetState) {
            if (!states.ContainsKey(targetState))
                return;
            if (currentState != null)
                currentState.OnTransitionOut();
            currentState = states[targetState];
            stateQueue.Push(currentState);
            currentState.OnEnter();
        }

        public static void Pop() {
            if (stateQueue.Count == 1)
                return;
            if (currentState != null)
                currentState.OnExit();
            stateQueue.Pop();
            currentState = stateQueue.Peek();
            currentState.OnTransitionIn();
        }

        public static string GetState {
            get { return currentState.state.ToString(); }
        }
    }
}

