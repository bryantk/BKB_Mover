using UnityEngine;

namespace BKB_RPG {
	[ExecuteInEditMode]
	public class PixelLock : MonoBehaviour {

		public int pixelsPerUnit = 16;
        public float resolution;
        // accumulate non-pixel movement
		Vector3 carry;

		void OnEnable() {
			carry = Vector3.zero;
			pixelsPerUnit = pixelsPerUnit > 0 ? pixelsPerUnit : 1;
			resolution = 1f / pixelsPerUnit;
		}

		// Update is called once per frame
		void LateUpdate () {
			Vector3 original = transform.position;
            Vector3 goal = SnapTo(transform.position + carry, resolution);
            transform.position = goal;
            carry += (original - goal);
		}

		float SnapTo(float source, float resolution) {
			return Mathf.RoundToInt(source/resolution) * resolution;
		}
		
		public Vector3 SnapTo(Vector3 v3, float resolution) {
			v3.x = SnapTo(v3.x, resolution);
			v3.y = SnapTo(v3.y, resolution);
			return v3;
		}

	}
}
