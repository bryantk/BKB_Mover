using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BKB_RPG {
    [System.Serializable]
    public class GameEvent : MonoBehaviour, IPauseable, ISetup {

        [SerializeField]
        public List<GameEventCommand> commands;
        public bool runGlobaly = false;

        Callback callback;
        // NOTE - Probably not needed
        private Coroutine coroutine = null;

        // readonly
        public bool _paused;

        public Entity parent;

        public void iSetup(object o) {
            parent = (Entity)o;
        }

        public void iPause() {
            _paused = true;
        }

        public void iResume() {
            _paused = false;
        }

        public void Run() {
            RunWithCallback();
        }

        public void RunWithCallback(Callback onComplete=null) {
            if (_paused)
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

            EntityMaster.AddCoroutine(parent, _Run(), runGlobaly);
        }

        void OnComplete()
        {
            EntityMaster.HaltCoroutine(parent);
            coroutine = null;
            parent.SetEvaluateConditions(true);
            if (callback != null)
                callback();
        }

        public IEnumerator _Run() {
            parent.SetEvaluateConditions(false);
            GameEvent gameEventObject = runGlobaly ? null : this;
            yield return null;
            for (int i = 0; i < commands.Count; i++)
            {
                while (_paused)
                    yield return null;
                // Run commands. Add case for editor/control related commands
                var command = commands[i];
                switch(command.CommandID) {
                case GameEventCommand.CommandTypes.GoTo:
                    i = command.int_1 == -1 ? commands.Count : command.int_1 - 1;
                    break;
                case GameEventCommand.CommandTypes.If:
                    if (!IsValidCondition(command.string_1))
                        Skip_If(ref i);
                    break;
                case GameEventCommand.CommandTypes.EndIf:
                    break;
                case GameEventCommand.CommandTypes.Else:
                    Skip_Else(ref i);
                    break;
                case GameEventCommand.CommandTypes.Debug:
                    Debug.Log(name + ": " + command.string_1 + "\n@" + Time.time);
                    break;
                case GameEventCommand.CommandTypes.Globals:
                    command.RunGlobals(command.string_2);
                    break;
                case GameEventCommand.CommandTypes.Local:
                    // TODO - pass entity to this class and use UID in this key?
                    command.RunGlobals(name + "_" + command.string_2);
                    break;
                case GameEventCommand.CommandTypes.Pause:
                    command.RunPause();
                    break;
                case GameEventCommand.CommandTypes.UnPause:
                    command.RunUnPause();
                    if (command.entity == null || command.entity == parent)
                        yield break;
                    break;
                case GameEventCommand.CommandTypes.EntityEvent:
                    EntityEventCommand(command);
                    break;
                default:
                    yield return command.Run(this);
                    break; 
                }
            }
            OnComplete();
        }

        public bool IsValidCondition(string condition, bool defaultReturn = false) {
            if (string.IsNullOrEmpty(condition))
                return defaultReturn;
            return GameMaster._instance.stringParser.EvaluateBool(condition);
        }

        private void EntityEventCommand(GameEventCommand command) {
            Entity target = command.entity != null ? command.entity : parent;
            switch (command.executionType)
            {
            case 0:
                var behaviour = (Entity.TriggerBehaviour)command.int_2;
                if (command.int_1 == -2)
                {
                    foreach (var ep in target.eventPages)
                    {
                        ep.trigger = behaviour;
                    }
                    return;
                }
                int eventPage = command.int_1;
                if (command.int_1 == -1)
                    eventPage = target.activePage;
                target.eventPages[eventPage].trigger = behaviour;
                break;
            case 1:
                target.iDestory();
                break;
            }
        }

        private void Skip_If(ref int i) {
            int elseDepth = 0;
            for (int j = i + 1; j < commands.Count; j++)
            {
                if (commands[j].CommandID == GameEventCommand.CommandTypes.If)
                    elseDepth++;
                else if (elseDepth == 0 && commands[j].CommandID == GameEventCommand.CommandTypes.Else)
                {
                    i = j;
                    return;
                }
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
            Debug.LogError(string.Format("No Closing 'EndIf' or Else found for 'If' command beginning at {0} on obj {1}.", i, gameObject.name));
            i = commands.Count - 1;
            return;
        }

        private void Skip_Else(ref int i) {
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