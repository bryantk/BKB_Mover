using UnityEngine;

namespace BKB_RPG {
	public class Depth : MonoBehaviour {
        // Where is the 'base' of the object relative to its origin
		public float yOffset;
        // Will this object be moving? (and thus need to re-calculate z-depth)
		public bool Moves = false;

        // Divide Y position by this to 'flatten' the z-depth
        const float field_depth = 10f;
        // Number of frames to wait to update a 'mover's z-depth
        const int sleep_frames = 10;

        int _sleep = 0;
		

		void Start () {
			DrawDepth();
		}
		
		// Update is called once per frame
		void Update () {
            if (!Moves)
                return;
            if (_sleep <= 0)
            {
                DrawDepth();
                _sleep = sleep_frames;
            }
            _sleep--;
		}

		
		void DrawDepth() {
			Vector3 temp = transform.position;
			temp.z = (temp.y + yOffset) / field_depth;
			transform.position = temp;
		}
		
	}
}
