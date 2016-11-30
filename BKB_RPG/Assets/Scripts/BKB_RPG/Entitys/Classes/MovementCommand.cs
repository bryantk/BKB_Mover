using UnityEngine;
using UnityEngine.Events;

namespace BKB_RPG {
	[System.Serializable]
	public class MovementCommand {

		public enum CommandTypes { Move, Face, Wait, GoTo, Boolean, Script, Remove, Set, Note, Sync, WaitSync };
        // What kind of command is this?
		public CommandTypes commandType;
        // Show detailed information in inespector?
        public bool expandedInspector;

        // BOOL
        public enum FlagType { LockFacing, AlwaysAnimate, Clip, ClipAll, Invisible, IgnoreImpossible, Reverse };
        // What kind of bool 
        public FlagType flag;
        public bool Bool;   // and Remove

        // WAIT
        public float time;

        // GOTO / REMOVE / SET
        public int int_1;

        // SET
        public enum SetTypes { Speed, Animation };
        public SetTypes setType;

        // SCRIPT
        public UnityEvent scriptCalls;

        // MOVE
        public enum MoverTypes { Relative, Angle, Absolute, To_transform, ObjName };
        public enum RandomTypes { None = 0, Linear = 1, Area = 2 };
        // Determines what logic to apply.
        public MoverTypes move_type;
        // Angle for movment. Use MaxStep for magnitude
        public float offsetAngle;
        // Transform to move towards.
        public Transform transformTarget;
        // Find object with name and set transform as target. Also used as note text.
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
        // # of lines inspector should reserve
        public int lines;

        public Mover moverTarget;

        public MovementCommand(CommandTypes type=CommandTypes.Move) {
            SetMovementCommand(type);
            return;
        }


        public MovementCommand Copy() {
            return (MovementCommand)this.MemberwiseClone();
        }


        public void SetMovementCommand(CommandTypes type = CommandTypes.Move) {
            expandedInspector = true;
            lines = 1;
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
            case CommandTypes.Script:
                ScriptCommand();
                break;
            case CommandTypes.Remove:
                RemoveCommand();
                break;
            case CommandTypes.Set:
                SetCommand();
                break;
            case CommandTypes.Note:
                NoteCommand();
                break;
            case CommandTypes.Sync:
                SyncCommand();
                break;
            case CommandTypes.Move:
            default:
                MoveCommand();
                break;
            }
        }

        public void BoolCommand() {
            commandType = CommandTypes.Boolean;
            Bool = false;
            flag = FlagType.LockFacing;
        }
        

        public void WaitCommand() {
            commandType = CommandTypes.Wait;
            time = 1;
        }


        public void GoToCommand() {
            commandType = CommandTypes.GoTo;
            int_1 = 0;
        }

        public void RemoveCommand() {
            commandType = CommandTypes.Remove;
            Bool = false;
            int_1 = 1;
        }

        public void ScriptCommand() {
            commandType = CommandTypes.Script;
            scriptCalls = new UnityEvent();
        }

        public void MoveCommand() {
            commandType = CommandTypes.Move;
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
            commandType = CommandTypes.Face;
        }

        public void SetCommand() {
            commandType = CommandTypes.Set;
            int_1 = 0;
            setType = SetTypes.Animation;
        }

        public void NoteCommand() {
            commandType = CommandTypes.Note;
            targetName = "";
        }

        public void SyncCommand() {
            commandType = CommandTypes.Sync;
            int_1 = 0;
            moverTarget = null;
        }

        /// <summary>
        /// Constructs a one line summary of the command
        /// </summary>
        public string BuildSummary() {
            string summary = "";
            switch (commandType)
            {
            case MovementCommand.CommandTypes.Move:
            case MovementCommand.CommandTypes.Face:
                switch (move_type)
                {
                case MovementCommand.MoverTypes.To_transform:
                    if (transformTarget != null)
                        summary = "TO: " + transformTarget.name;
                    else
                        summary = "TO: NULL";
                    break;
                case MovementCommand.MoverTypes.ObjName:
                    summary = "TO: " + targetName;
                    break;
                default:
                    Vector2 norm = target.normalized;
                    if (norm == Vector2.up)
                        summary = "Up " + target.y;
                    else if (norm == Vector2.right)
                        summary = "Right " + target.x;
                    else if (norm == Vector2.down)
                        summary = "Down " + Mathf.Abs(target.y);
                    else if (norm == Vector2.left)
                        summary = "Left " + Mathf.Abs(target.x);
                    else
                        summary = target.ToString() + " " + move_type.ToString();
                    break;
                case MovementCommand.MoverTypes.Angle:
                    if (offsetAngle == 0)
                        summary = "Forward " + maxStep;
                    else if (offsetAngle == 90)
                        summary = "Right " + maxStep;
                    else if (offsetAngle == 180)
                        summary = "Back " + maxStep;
                    else if (offsetAngle == 270)
                        summary = "Left " + maxStep;
                    else
                        summary = "Move " + maxStep + " at " + offsetAngle + " degrees";
                    break;
                }
                if (withinDistance > 0)
                    summary = "*" + summary;
                if (instant)
                    summary = "!" + summary;
                break;
            case CommandTypes.Wait:
                summary = time.ToString() + " seconds";
                break;
            case CommandTypes.Boolean:
                summary = string.Format("{0} : {1}", flag, Bool);
                break;
            case CommandTypes.GoTo:
                summary = "GoTo command " + int_1.ToString();
                break;
            case CommandTypes.Script:
                summary = "Call scripts: " + scriptCalls.GetPersistentEventCount();
                break;
            case CommandTypes.Remove:
                summary = "Remove " + int_1 + " commands";
                break;
            case CommandTypes.Set:
                summary = "Set " + setType.ToString() + " = " + int_1.ToString();
                break;
            case CommandTypes.Note:
                summary = "\"" + targetName + "\"";
                break;
            case CommandTypes.Sync:
                if (int_1 == 0)
                    summary = "Wait Sync";
                else
                    summary = "Send Sync: " + (moverTarget == null ? "self" : moverTarget.name);
                break;
            // ----------------------------------
            // COMMAND QUICK VIEW
            // ----------------------------------
            default:
                summary = commandType.ToString() + " not implemented";
                break;
            }

            return summary;
        }

    }
}