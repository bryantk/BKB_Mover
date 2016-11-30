using UnityEngine;

namespace BKB_RPG {
	[ExecuteInEditMode]
	public class PixelLock : MonoBehaviour
	{

	    public Transform parent;
	    public Transform target;

	    public Vector3 offset;

		public int pixelsPerUnit = 16;
        public float resolution;
        // accumulate non-pixel movement
		Vector3 carry;

		void OnEnable() {
			carry = Vector3.zero;
		    pixelsPerUnit = Mathf.Max(1, pixelsPerUnit);
			resolution = 1f / pixelsPerUnit;
		    parent = parent ?? transform;
		    target = target ?? transform;

		}

		// Update is called once per frame
		void LateUpdate () {
		    if (parent == target)
		    {
                Vector3 original = transform.position;
                Vector3 goal = SnapTo(transform.position + carry, resolution);
                transform.position = goal;
                carry += (original - goal);
		        return;
		    }
		    target.position = SnapTo(parent.position + offset, resolution);
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
