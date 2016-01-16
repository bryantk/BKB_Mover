using System.Collections.Generic;

namespace BKB_RPG {
    [System.Serializable]
    public class MovementCommand_Face : MovementCommand_Move {

        public MovementCommand_Face() : base() {
            command_type = CommandTypes.Face;
            facingCommand = true;
        }

    }
}
