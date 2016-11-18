using UnityEngine;

namespace BKB_RPG {
	public class Depth : MonoBehaviour {
        // Divide Y position by this to 'flatten' the z-depth
        const float FIELD_DEPTH = 10f;

        // Where is the 'base' of the object relative to its origin
        public float yOffset;
        // Will this object be moving? (and thus need to re-calculate z-depth)
		public bool Moves = true;
        // For dynamic things (walking over bridge, pick up item, fly, etc)
        public float DynamicOffset = 0;

        private TickCounter frameCounter = new TickCounter(30);

        void OnEnable () {
			DrawDepth();
            enabled = Moves;
        }
		
		// TODO - Should this tie into Enties?
		void Update ()
		{
		    if (!enabled)
		        return;
            if (frameCounter.Tick)
            {
                DrawDepth();
            }
		}
		
		public void DrawDepth() {
			Vector3 temp = transform.position;
			temp.z = (temp.y + yOffset + DynamicOffset) / FIELD_DEPTH;
			transform.position = temp;
		}
		
	}
}
