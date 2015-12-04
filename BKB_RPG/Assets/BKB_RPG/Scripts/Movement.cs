using UnityEngine;
using UnityEditor;
using System.Collections;

namespace BKB_RPG {
	public class Movement : MonoBehaviour {
		/*
		 * TO DOs:
		 *  1. remove unneded things
		 *  2. Cache all one-time-compute things
		 *  3. Movement base class
		 */

		public enum results{Nil = 0, Complete = 1, Hit = 2};

		bool slide = false;
		float speed = 1;
		float spread = 0.75f;
		float stop_range = 2;
		float radius = 1;
		int ray_density = 2;
		// Track recursive calls to Move
		private int depth = 0;


		public void Setup(float mySpeed, float myRadius, bool shouldSlide, float mySpread, float stopAt, int rays) {
			speed = mySpeed;
			radius = myRadius;
			slide = shouldSlide;
			spread = mySpread;
			stop_range = stopAt;
			ray_density = rays;
		}

		public virtual int Move(Vector3 target) {
			depth++;
			Vector2 dir = target - transform.position;
			// within destination?
			if (dir.magnitude <= speed) {
				transform.position = target;
				depth = 0;
				return (int)results.Complete;
			}
			// ray cast
			RaycastHit2D hit = Utils.Raycast(transform.position, dir.normalized, ray_density,
			                                 radius*spread, radius+speed*stop_range, this.transform);
			if (hit) {
				// DEBUG only
				if (EditorApplication.isPlaying) {
					if (hit.transform == this.transform)
						Debug.LogWarning(this.name + " colliding with self.");
				}
				if (slide && depth < 2) {
					Vector3 t = Vector3.Cross(dir.normalized, hit.normal);
					t = Vector3.Cross(hit.normal, t);
					t.z = 0;
					return Move (transform.position + (Vector3)t.normalized);
				}
				depth = 0;
				return (int)results.Hit;
			}
			// move some
			transform.position += (Vector3)dir.normalized * speed;
			depth = 0;
			return (int)results.Nil;
		}

	}
	
}