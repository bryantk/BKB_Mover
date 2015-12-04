using UnityEngine;
using System.Collections;

namespace BKB_RPG {
	public static class DebugHelpers {

		public static void DrawArrow(Vector3 start, Vector3 end) {
			Gizmos.DrawLine(start, end);
			Vector3 dir = end-start;
			dir = dir.normalized;
			Vector3 t = Vector3.Cross(Vector3.forward, dir) * 0.15f;
			dir = dir * 0.25f;
			Gizmos.DrawLine(end, end-t-dir);
			Gizmos.DrawLine(end, end+t-dir);
		}

	}
}
