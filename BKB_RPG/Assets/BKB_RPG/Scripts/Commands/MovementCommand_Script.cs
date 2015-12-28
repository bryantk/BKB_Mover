

namespace BKB_RPG {
    [System.Serializable]
    public class MovementCommand_Script : MovementCommand {
        public UnityEngine.Events.UnityEvent events;

        public MovementCommand_Script() : base() {
            command_type = CommandTypes.Script;
        }
    }
}
