namespace BKB_RPG {
    [System.Serializable]
    public class MovementCommand_Wait : MovementCommand {

        public float time;

        public MovementCommand_Wait() : base() {
            command_type = CommandTypes.Wait;
            time = 1;
        }
    }
}
