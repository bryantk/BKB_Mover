using UnityEngine;
using System.Collections;
using UnityEditor;

[ExecuteInEditMode]
public class testt : MonoBehaviour {

	public float facing;
	public float angle;
	public Vector2 A;
	public Vector2 B;
	public Transform target;

	// Update is called once per frame
	void Update () {
		A = Vector2.zero;
		B = (target.position-transform.position).normalized;
		facing = Vector3.Angle( transform.position, target.position );
		Vector2 r = target.position-transform.position;
		angle = Mathf.Atan2(r.y, r.x)*Mathf.Rad2Deg+90;
		if (angle < 0)
			angle += 360;

	}

	void OnDrawGizmosSelected() {
		BKB_RPG.Utils.DrawArrow(transform.position, target.position);
		BKB_RPG.Utils.DrawArrow(Vector3.zero, (target.position-transform.position).normalized);
	}

}
