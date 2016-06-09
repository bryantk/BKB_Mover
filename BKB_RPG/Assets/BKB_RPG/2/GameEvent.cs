using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BKB_RPG {
    [System.Serializable]
    public class GameEvent : MonoBehaviour, IPauseable {

        [SerializeField]
        public List<GameEventCommand> commands;

        Callback callback;
        Coroutine coroutine = null;

        // readonly
        public bool Paused;


        public void iPause() {
            Paused = true;
        }

        public void iResume() {
            Paused = false;
        }

        public void Run() {
            RunWithCallback(null);
        }

        public void RunWithCallback(Callback onComplete=null) {
            if (Paused)
            {
                Debug.LogWarning(name + "'s GameEvent invoked, but it is PAUSED.");
                return;
            }
            if (coroutine != null)
            {
                Debug.LogWarning(name + "'s GameEvent invoked while already running.");
                return;
            }
            callback = onComplete;
            //coroutine = StartCoroutine(_Run());
            coroutine = GameMaster._instance.StartCoroutine(_Run());
        }

        public IEnumerator _Run() {
            for (int i = 0; i < commands.Count; i++) {
                // Run commands. Add case for editor/control related commands
                switch(commands[i].CommandID) {
                case GameEventCommand.CommandTypes.GoTo:
                    i = commands[i].int_1 - 1;
                    break;
                case GameEventCommand.CommandTypes.If:
                    // TODO
                    // If not true, jump to else+1 or end
                    break;
                case GameEventCommand.CommandTypes.EndIf:
                    break;
                case GameEventCommand.CommandTypes.Else:
                    Run_Else(ref i);
                    break;
                default:
                    yield return commands[i].Execute();
                    break; 
                }
            }
            coroutine = null;
            if (callback != null)
                callback();
        }

        private void Run_Else(ref int i) {
            int elseDepth = 0;
            for (int j = i + 1; j < commands.Count; j++)
            {
                if (commands[j].CommandID == GameEventCommand.CommandTypes.If)
                    elseDepth++;
                else if (commands[j].CommandID == GameEventCommand.CommandTypes.EndIf)
                {
                    if (elseDepth == 0)
                    {
                        i = j;
                        return;
                    }
                    elseDepth--;
                }
            }
            Debug.LogError(string.Format("No Closing 'EndIf' found for 'Else' command beginning at {0} on obj {1}.", i, gameObject.name));
            i =  commands.Count - 1;
            return;
        }

    }
}