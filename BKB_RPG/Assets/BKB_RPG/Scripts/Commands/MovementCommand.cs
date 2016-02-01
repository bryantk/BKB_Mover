using UnityEngine;

namespace BKB_RPG {
	[System.Serializable]
	public class MovementCommand {

		public enum CommandTypes { Move, Face, Wait, GoTo, Boolean, Script};
        // What kind of command is this?
		public CommandTypes command_type;
        // Show detailed information in inespector?
        public bool expandedInspector;

        // BOOL
        public enum FlagType { LockFacing, AlwaysAnimate, Clip, ClipAll, Invisible, IgnoreImpossible, Reverse, Pause, Script };
        // What kind of bool command.
        public FlagType flag;
        public bool Bool;

        // WAIT
        public float time;

        // GOTO
        public int gotoId;

        // MOVE
        public enum MoverTypes { Relative, Angle, Absolute, To_transform, ObjName };
        public enum RandomTypes { None = 0, Linear = 1, Area = 2 };
        // Determines what logic to apply.
        public MoverTypes move_type;
        // Angle for movment. Use MaxStep for magnitude
        public float offsetAngle;
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
        //
        public RandomTypes randomType;
        // Choose random point from 'x' to 'y' of target as new target
        public Vector2 random;
        // Choose random point from 'x' to 'y' of target as new target
        public Vector2 random2;
        // Teleport to target
        public bool instant;
        // Readjust target each frame (follow a target transform)
        public bool recalculate;


        public MovementCommand(CommandTypes type=CommandTypes.Move) {
            expandedInspector = true;
            switch (type)
            {
            case CommandTypes.Boolean:
                BoolCommand();
                break;
            case CommandTypes.Wait:
                WaitCommand();
                break;
            case CommandTypes.GoTo:
                GoToCommand();
                break;
            case CommandTypes.Face:
                FaceCommand();
                break;
            case CommandTypes.Move:
            default:
                MoveCommand();
                break;
            }
        }

        public void SetMovementCommand(CommandTypes type = CommandTypes.Move) {
            expandedInspector = true;
            switch (type)
            {
            case CommandTypes.Boolean:
                BoolCommand();
                break;
            case CommandTypes.Wait:
                WaitCommand();
                break;
            case CommandTypes.GoTo:
                GoToCommand();
                break;
            case CommandTypes.Face:
                FaceCommand();
                break;
            case CommandTypes.Move:
            default:
                MoveCommand();
                break;
            }
        }

        public void BoolCommand() {
            command_type = CommandTypes.Boolean;
            Bool = false;
            flag = FlagType.LockFacing;
        }
        

        public void WaitCommand() {
            command_type = CommandTypes.Wait;
            time = 1;
        }


        public void GoToCommand() {
            command_type = CommandTypes.GoTo;
            gotoId = 0;
        }

        
        public void MoveCommand() {
            command_type = CommandTypes.Move;
            move_type = MoverTypes.Relative;
            randomType = RandomTypes.None;
            random = Vector2.zero;
            random2 = Vector2.zero;
            offsetAngle = 0;
            maxStep = 0;
            withinDistance = 0;
            instant = false;
            recalculate = false;
        }


        public void FaceCommand() {
            MoveCommand();
            command_type = CommandTypes.Face;
        }

    }
}