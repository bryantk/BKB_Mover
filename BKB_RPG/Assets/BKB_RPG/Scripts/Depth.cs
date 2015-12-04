using UnityEngine;
using System.Collections;

namespace BKB_RPG {
	public class Depth : MonoBehaviour {
		public float yOffset;
		public bool Moves = false;
		bool flip = false;
		
		// Use this for initialization
		void Start () {
			DrawDepth();
		}
		
		// Update is called once per frame
		void Update () {
			if (Moves && flip) {
				DrawDepth();
				flip = false;
			} else 
				flip = true;
		}
		
		void DrawDepth() {
			Vector3 temp = transform.position;
			temp.z = (temp.y + yOffset) / 10;
			transform.position = temp;
		}
		
	}
}
