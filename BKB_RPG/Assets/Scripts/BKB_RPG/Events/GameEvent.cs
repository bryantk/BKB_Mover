using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BKB_TEXT;

namespace BKB_RPG {
    [System.Serializable]
    public class GameEvent : MonoBehaviour, ISetup {

        [SerializeField]
        public List<GameEventCommand> commands;
        public bool runGlobaly = false;

        Callback callback;
        // NOTE - Probably not needed
        private Coroutine coroutine = null;

        public Entity parent;

        public void iSetup(object o) {
            parent = (Entity)o;
        }

        public bool IsRunning()
        {
            return EntityMaster.EntityCoroutineRunning(parent, runGlobaly);
        }

        public void Run() {
            RunWithCallback();
        }

        public void RunWithCallback(Callback onComplete=null) {
            if (IsRunning())
                return;
            if (parent.Paused)
            {
                Debug.LogWarning(name + "'s GameEvent invoked, but it is PAUSED.");
                return;
            }
            coroutine = EntityMaster.AddCoroutine(parent, _Run(), runGlobaly);
            if (coroutine != null)
                callback = onComplete;
        }

        void OnComplete()
        {
            EntityMaster.HaltCoroutine(parent);
            coroutine = null;
            parent.ShouldEvaluateConditions(true);
            if (callback != null)
                callback();
        }

        public IEnumerator _Run() {
            parent.ShouldEvaluateConditions(false);
            yield return null;
            for (int i = 0; i < commands.Count; i++)
            {
                while (parent.Paused)
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
                    var key = "_" + command.string_2;
                    if (parent.myGUID == 0)
                    {
                        Debug.LogError(string.Format("'{0}' attempted to save local key without a uGUI component.", name));
                        command.RunGlobals(name + key);
                        break;
                    }
                    command.RunGlobals(parent.myGUID + key);
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
                    if (command.entity == null)
                        Debug.LogWarning(string.Format("No entity target set for command {0} of '{1}'. Target set to self as default. Is this intended?", i, name));
                    yield return command.Run(this);
                    break;
                case GameEventCommand.CommandTypes.ClearDialouge:
                    DialougeDisplay.CloseMessages();
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