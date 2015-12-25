using UnityEngine;
using BKB_RPG;

public class MovementCommand_Move : MovementCommand {

    public MoverTypes move_type;
    public Transform transformTarget;
    public Vector2 myVector2;
    public string myString;

    public MovementCommand_Move() : base() {
        command_type = CommandTypes.Wait;
        move_type = MoverTypes.Relative;

    }

}
