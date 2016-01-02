namespace BKB_RPG {
    [System.Serializable]
    public class MovementCommand_Bool : MovementCommand {

        public enum FlagType { LockFacing, Clip, ClipAll, Invisible, IgnoreImpossible, Reverse, Pause};
        // What kind of bool command.
        public FlagType flag;
        public bool Bool;

        public MovementCommand_Bool() : base() {
            command_type = CommandTypes.Boolean;
            flag = FlagType.LockFacing;
            Bool = false;
        }
    }
}
