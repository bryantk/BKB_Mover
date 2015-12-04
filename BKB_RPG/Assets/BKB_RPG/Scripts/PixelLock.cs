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
			transform.position = (transform.position + carry).SnapTo(resolution);
			carry += (goal - transform.position);
		}
		
	}
}
