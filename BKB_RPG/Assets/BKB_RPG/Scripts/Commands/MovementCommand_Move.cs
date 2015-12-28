using UnityEngine;
namespace BKB_RPG {
    [System.Serializable]
    public class MovementCommand_Move : MovementCommand {

        public enum MoverTypes { Relative, Absolute, To_transform, ObjName };
        public MoverTypes move_type;
        public Transform transformTarget;
        public Vector2 target;
        public string targetName;
        public float withinDistance;
        public bool instant;
        public bool recalculate; //readucst target

        public MovementCommand_Move() : base() {
            command_type = CommandTypes.Move;
            move_type = MoverTypes.Relative;
            withinDistance = 0;
            instant = false;
            recalculate = false;
        }
    }
}
