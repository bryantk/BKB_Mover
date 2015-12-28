
namespace BKB_RPG {
    [System.Serializable]
    public class MovementCommand_GOTO : MovementCommand {

        public int gotoId;

        public MovementCommand_GOTO() : base() {
            command_type = CommandTypes.GoTo;
            gotoId = 0;
        }

    }
}
