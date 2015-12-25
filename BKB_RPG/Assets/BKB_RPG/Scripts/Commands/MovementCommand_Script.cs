using UnityEngine;
using BKB_RPG;

public class MovementCommand_Script : MovementCommand {

    public UnityEngine.Events.UnityEvent myScriptCalls;

    public MovementCommand_Script() : base() {
        command_type = CommandTypes.Script;
    }

}
