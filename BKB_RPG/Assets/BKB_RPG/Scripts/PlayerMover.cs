using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMover : MonoBehaviour {
	/*
		 * TO DOs:
		 *  1. Tile movement is 50% grid manager + 50% non-continuos user input
		 */
	public float speed = 1;

	private BKB_RPG.Mover myMovement;

	// Use this for initialization
	void Start () {
		myMovement = GetComponent<BKB_RPG.Mover>();
        //myMovement.Setup(speed, 0.5f, true, 0.5f, 2, 2);
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 dir = new Vector2(Input.GetAxisRaw("Horizontal") * speed, Input.GetAxisRaw("Vertical") * speed);
		myMovement.Move((Vector3)dir.normalized + transform.position);
		//myRigidbody.velocity = dir;

	}
}
