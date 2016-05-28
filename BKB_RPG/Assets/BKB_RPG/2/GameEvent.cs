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

        void OnEnable() {
            if (commands == null)
                commands = new List<GameEventCommand>();
            hideFlags = HideFlags.HideAndDontSave;
        }


        void Start() {
            foreach (GameEventCommand c in commands)
            {
                print(c.GetType());
            }
        }


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
            coroutine = StartCoroutine(_Run());
        }

        public IEnumerator _Run() {
            foreach (GameEventCommand command in commands)
            {
                yield return command.Execute();
            }
            coroutine = null;
            if (callback != null)
                callback();
        }

    }
}