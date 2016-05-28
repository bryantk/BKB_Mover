using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace BKB_RPG {
    [System.Serializable]
    public class GameEventCommand {

        public enum CommandTypes { Teleport=1, Pause, UnPause, Script };

        public CommandTypes CommandID;
        public bool Block = false;
        // # of lines inspector should reserve
        public int lines = 1;


        // SCRIPT
        public UnityEvent scriptCalls;

        public int dataInt = 0;



        public GameEventCommand(CommandTypes type) {
            SetEventCommand(type);
        }

        public void SetEventCommand(CommandTypes type) {
            lines = 1;
            switch (type)
            {
            case CommandTypes.Pause:
                PauseCommand();
                break;
            case CommandTypes.UnPause:
                
                break;
            case CommandTypes.Script:
                
                break;
            }
        }

        public GameEventCommand Copy() {
            return (GameEventCommand)this.MemberwiseClone();
        }

        // ----------------------------------------------------------------------
        public void PauseCommand() {
            CommandID = CommandTypes.Pause;
            dataInt = 0;
        }

        public void UnPauseCommand() {
            CommandID = CommandTypes.UnPause;
            dataInt = 0;
        }

        // ----------------------------------------------------------------------

        public virtual IEnumerator Execute() {
            if (Block)
                yield return Run();
            else
                GameMaster._instance.StartCoroutine(Run());
        }

        public virtual IEnumerator Run() {
            yield return null;
        }

        public void ScriptCommand() {
            CommandID = CommandTypes.Script;
            scriptCalls = new UnityEvent();
        }

    }
}
