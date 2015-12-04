using UnityEngine;
using System.Collections;

namespace BKB_RPG {
	[ExecuteInEditMode]
	public class PixelLock : MonoBehaviour {

		public int pixelsPerUnit = 16;
		float resolution;
		Vector3 carry;
		
		// Use this for initialization
		void OnEnable () {
			resolution = 1f / pixelsPerUnit;
		}


		// Update is called once per frame
		void LateUpdate () {
			Vector3 goal = transform.position;
			transform.position = SnapTo(transform.position + carry, resolution);
			carry += (goal - transform.position);
		}
		
		float SnapTo(float source, float resolution) {
			return Mathf.RoundToInt(source/resolution) * resolution;
		}
		
		Vector3 SnapTo(Vector3 v3, float resolution) {
			v3.x = SnapTo(v3.x, resolution);
			v3.y = SnapTo(v3.y, resolution);
			return v3;
		}

	}
}
