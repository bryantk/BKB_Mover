using UnityEngine;
namespace BKB_RPG {
    [System.Serializable]
    public class MovementCommand_Move : MovementCommand {

        public enum MoverTypes { Relative, Angle, Absolute, To_transform, ObjName };
        // Determines what logic to apply.
        public MoverTypes move_type;
        // Angle for movment. Use MaxStep for magnitude
        public float angle;
        // Transform to move towards.
        public Transform transformTarget;
        // Find object with name and set transform as target
        public string targetName;
        // Vector to move towards. Either relative to self or absolute.
        public Vector2 target;
        // Walk 'X' units towards target, then stop.
        public float maxStep;
        // Stop when within 'X' units of target.
        public float withinDistance;
        // Choose random point from 'x' to 'y' of target as new target
        public Vector2 random;
        // Teleport to target
        public bool instant;
        // Readjust target each frame (follow a target transform)
        public bool recalculate;

        // Facing command?
        public bool facingCommand;

        public MovementCommand_Move() : base() {
            command_type = CommandTypes.Move;
            move_type = MoverTypes.Relative;
            angle = 0;
            maxStep = 0;
            withinDistance = 0;
            instant = false;
            recalculate = false;
            facingCommand = false;
        }
    }
}
