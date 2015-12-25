using UnityEngine;
using System.Collections;

namespace BKB_RPG {
	[System.Serializable]
	public class MovementCommand {

		public enum CommandTypes {Move, Wait, Teleport, GoTo, Script};
		public enum MoverTypes {Relative, Absolute, To_transform, obj_name};

		public CommandTypes command_type;
		public MoverTypes move_type;
		public Transform transformTarget;
		public Vector2 myVector2;
		public string myString;
		public float myFloat1;
		public float myFloat2;
		public int myInt;
		public bool myBool;
        //public object[] objects;
        public UnityEngine.Events.UnityEvent myScriptCalls;

        public bool expandedInspector;

        public MovementCommand(CommandTypes commandtype=CommandTypes.Wait) {
			Init(commandtype);
		}

		private void Init(CommandTypes commandtype) {
			command_type = commandtype;
			move_type = MoverTypes.Relative;
			transformTarget = null;
			myVector2 = Vector2.zero;
			myString = "";
			myFloat1 = 0;
			myFloat2 = 0;
			myInt = 0;
			myBool = false;
            expandedInspector = true;
        }
	}
}