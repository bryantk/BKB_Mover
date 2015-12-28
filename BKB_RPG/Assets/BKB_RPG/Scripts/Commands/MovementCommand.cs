using UnityEngine;

namespace BKB_RPG {
	[System.Serializable]
	public class MovementCommand : ScriptableObject {

		public enum CommandTypes {Move, Wait, Teleport, GoTo, Boolean, Script};
		//public enum MoverTypes {Relative, Absolute, To_transform, obj_name};

		public CommandTypes command_type;
        public bool expandedInspector;

        public MovementCommand() {
            expandedInspector = true;
        }
	}
}