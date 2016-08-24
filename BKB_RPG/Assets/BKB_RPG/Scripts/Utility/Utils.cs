using UnityEngine;
using System.Collections.Generic;


namespace BKB_RPG {
	public static class Utils {

        /// <summary>
        /// Like Unity RaycastHit2D, but casts 1 to many rays from center to teh width of the collider.
        /// </summary>
        /// <param name="position">Center of Entity</param>
        /// <param name="castDir">Direction to cast</param>
        /// <param name="rays"># of rays to cast</param>
        /// <param name="spread">Distance between rays modifier.</param>
        /// <param name="lookAhead">Extra Distance to cast for colissiosn immediatly ahead.</param>
        /// <param name="self">Optional transform to ignore collisions against</param>
        /// <param name="layers">Layers to raycast against</param>
        /// <param name="minDepth">Only include objects with a Z coordinate (depth) greater than or equal to this value.</param>
        /// <param name="maxDepth">Only include objects with a Z coordinate (depth) less than or equal to this value.</param>
        /// <returns></returns>
		public static RaycastHit2D Raycast(Vector2 position, Vector2 castDir, int rays=1, float spread=1,
		                                   float lookAhead=0.5f, Transform self=null, int layers=Physics2D.DefaultRaycastLayers,
                                           float minDepth=-Mathf.Infinity, float maxDepth=Mathf.Infinity) {
            // spread = collider.radius * spread
            // lookAhead = collider.radius + speed * stop_range
            // move compare location slightly forward to prevent hitting own collider.
            if (layers == 0)
                return new RaycastHit2D();
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
				hit = Physics2D.Raycast(test_location, castDir, lookAhead * decay, layers, minDepth, maxDepth);
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

        /// <summary>
        /// Convert given angle to Vector2 (N = 0 degrees, follows clockwise)
        /// </summary>
        /// <param name="angle">Input angle</param>
        /// <param name="magnitude">Optional magnitude of resulting Vector2</param>
        /// <returns></returns>
        public static Vector2 AngleMagnitudeToVector2(float angle, float magnitude=1) {
            Quaternion rotation = Quaternion.AngleAxis(450 - angle, Vector3.forward);
            return rotation * (Vector3.right * magnitude);
        }

        /// <summary>
        /// Given an arc, choose a random angle;
        /// </summary>
        /// <param name="min">Degree start of arc</param>
        /// <param name="max">Degree end of acr</param>
        /// <param name="directions">Acceptable 'steps' of directions (4, 8, 360)</param>
        /// <param name="magnitude"></param>
        /// <returns></returns>
        public static int RandomAngleWithinArc(int min, int max, int directions = 360) {
            min = min / directions;
            max = max / directions;
            return Random.Range(min, max) * directions;
        }

        /// <summary>
        /// Clamp given angle based on permited mover vectors.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="dir">4 = 90, 8 = 45, 360 = 360</param>
        /// <returns></returns>
        public static int ClampAngle(float angle, int dir) {
            int step = 360 / dir;
            return (Mathf.RoundToInt(angle / step) * step) % 360;
        }

        /// <summary>
        /// Covert given Vector to angle.
        /// </summary>
        /// <param name="v">Vector to convert.</param>
        /// <returns></returns>
        public static float Vector2toAngle(Vector2 v) {
            if (v == Vector2.zero)
                return 0;
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg - 90f;
            if (angle < 0)
                angle += 360;
            return Mathf.Abs(angle - 360) % 360;
        }

        /// <summary>
        /// Determine angle between two given vectors.
        /// </summary>
        /// <param name="position">Source</param>
        /// <param name="target">Destination</param>
        /// <returns></returns>
        public static float AngleBetween(Vector3 position, Vector3 target) {
            Vector2 r = target - position;
            return Vector2toAngle(r);
        }

        public static Vector2 RotateVector2byAngle(Vector2 v, float angle) {
            if (angle == 0)
                return v;
            float magnitude = v.magnitude;
            float myAngle = (Vector2toAngle(v) + angle) % 360;
            return AngleMagnitudeToVector2(myAngle, magnitude);
        }

        // Debug / Handles things

        public static void DrawArrow(Vector3 start, Vector3 end) {
			Gizmos.DrawLine(start, end);
			Vector3 dir = end-start;
			dir = dir.normalized;
			Vector3 t = Vector3.Cross(Vector3.forward, dir) * 0.15f;
			dir = dir * 0.25f;
			Gizmos.DrawLine(end, end-t-dir);
			Gizmos.DrawLine(end, end+t-dir);
		}


        public static float TouchedAt(float myFacing, float targetFacing) {
            float diff = targetFacing - myFacing;
            if (diff < 0)
                diff = 360 + diff;
            // 0 = Back
            // 90 = Right Side
            // 180 = Front
            // 270 = Left side
            return diff % 360;
        }


#if UNITY_EDITOR
        public static void DrawArrow(Vector3 start, Vector3 end, Color? color=null) {
            Color old = UnityEditor.Handles.color;
            if (color != null)
                UnityEditor.Handles.color = (Color)color;
            UnityEditor.Handles.DrawLine(start, end);
            Vector3 dir = end - start;
            dir = dir.normalized;
            Vector3 t = Vector3.Cross(Vector3.forward, dir) * 0.15f;
            dir = dir * 0.25f;
            UnityEditor.Handles.DrawLine(end, end - t - dir);
            UnityEditor.Handles.DrawLine(end, end + t - dir);
            if (color != null) {
                UnityEditor.Handles.color = old;
            }
        }

        public static void DrawDottedArrow(Vector3 start, Vector3 end, Color? color = null) {
            Color old = UnityEditor.Handles.color;
            if (color != null)
                UnityEditor.Handles.color = (Color)color;
            UnityEditor.Handles.DrawDottedLine(start, end, 5);
            Vector3 dir = end - start;
            dir = dir.normalized;
            Vector3 t = Vector3.Cross(Vector3.forward, dir) * 0.15f;
            dir = dir * 0.25f;
            UnityEditor.Handles.DrawLine(end, end - t - dir);
            UnityEditor.Handles.DrawLine(end, end + t - dir);
            if (color != null)
            {
                UnityEditor.Handles.color = old;
            }
        }


        public static LayerMask LayerMaskField(string label, LayerMask layerMask) {
            //by FlyingOstriche
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = UnityEditor.EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }
#endif
    }
}
