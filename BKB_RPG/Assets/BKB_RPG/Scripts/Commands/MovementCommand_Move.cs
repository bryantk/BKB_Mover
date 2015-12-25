using UnityEngine;
using BKB_RPG;

public class MovementCommand_Move : MovementCommand {

    public enum MoverTypes { Relative, Absolute, To_transform, obj_name };
    public MoverTypes move_type;
    public Transform transformTarget;
    public Vector2 target;
    public string targetName;
    public bool recalculate; //readucst target

    public MovementCommand_Move() : base() {
        command_type = CommandTypes.Move;
        move_type = MoverTypes.Relative;

    }

}
