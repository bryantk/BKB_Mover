using UnityEngine;
using System.Collections;

public static class Utils {

	public static RaycastHit2D Raycast(Vector2 position, Vector2 castDir, int rays=1, float spread=1,
	                                   float lookAhead=0.5f, Transform self=null, int layers=Physics2D.DefaultRaycastLayers) {
		// spread = collider.radius * spread
		// lookAhead = collider.radius + speed * stop_range
		// move compare location slightly forward to prevent hitting own collider.
		position += castDir * 0.025f;
		Debug.DrawLine(position, position + castDir * lookAhead, Color.yellow);
		RaycastHit2D hit = Physics2D.Raycast(position, castDir, lookAhead, layers);
		if ((hit || rays == 1) && hit.transform != self)
			return hit;
		Vector2 offset = Vector3.Cross(castDir.normalized, Vector3.forward);
		float step = 1f/(rays-1);
		for (float x = 1; x > 0; x -= step) {
			float decay = 1 - 0.25f * x * x;
			// right side
			Vector2 test_location = position + offset * x * spread;
			Debug.DrawLine(test_location, test_location + castDir * lookAhead * decay, Color.yellow);
			hit = Physics2D.Raycast(test_location, castDir, lookAhead * decay, layers);
			if (hit && hit.transform != self)
				return hit;
			// left side
			test_location = position - offset * x * spread;
			Debug.DrawLine(test_location, test_location + castDir * lookAhead * decay, Color.yellow);
			hit = Physics2D.Raycast(test_location, castDir, lookAhead * decay, layers);
			if (hit && hit.transform != self)
				return hit;
		}
		if (hit && hit.transform == self)
			hit = new RaycastHit2D();
		return hit;
	}

	public static float SnapTo(float source, float resolution) {
		return Mathf.RoundToInt(source/resolution) * resolution;
	}

	public static Vector2 SnapTo(this Vector2 v2, float resolution) {
		v2.x = SnapTo(v2.x, resolution);
		v2.y = SnapTo(v2.y, resolution);
		return v2;
	}

	public static Vector3 SnapTo(this Vector3 v3, float resolution) {
		v3.x = SnapTo(v3.x, resolution);
		v3.y = SnapTo(v3.y, resolution);
		return v3;
	}

}
