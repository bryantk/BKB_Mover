using UnityEngine;

namespace BKB_RPG {
	[System.Serializable]
	public class MovementCommand : ScriptableObject {

		public enum CommandTypes {Move, Wait, GoTo, Boolean, Script};
        // What kind of command is this?
		public CommandTypes command_type;
        // Show detailed information in inespector?
        public bool expandedInspector;

        public MovementCommand() {
            expandedInspector = true;
        }

    }
}