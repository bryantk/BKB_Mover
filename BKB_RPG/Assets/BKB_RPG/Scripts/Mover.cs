using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace BKB_RPG {
	[RequireComponent(typeof(CircleCollider2D))]
	public class Mover : MonoBehaviour {
		public enum RepeatBehavior{None, PingPong, Loop, ResetAndLoop}; 
		public enum Speed{Slowest=5, Slower=15, Slow=30, Normal=50, Faster=100, Fastest=200}
		// movement pathing results
		public enum results{Nil = 0, Complete = 1, Hit = 2};

		public List<MovementCommand> commands;

		public RepeatBehavior Repeat = RepeatBehavior.None;
		public Speed move_speed = Speed.Normal;
		public float speed {
			get {return (int)move_speed/1000f;}
		}

		public float spread = 0.75f;
		public float stop_range = 2;
		public float radius = 0;
		public int ray_density = 2;
		public bool slide = false;
		public bool move_forward = true;
		public int currentNode = 0;
		public bool ignore_impossible = false;

		// not in inspector
		public Vector3 startPosition;
		float waitTime = 0;
		bool targetSet = false;
		Vector3 target;
		bool stopped = false;

		//move related
		int depth = 0;

		void Start() {
			Setup();
		}

		// Update is called once per frame
		void Update () {
			Tick();
		}


		void Setup() {
			if (!move_forward && Repeat == RepeatBehavior.None)
				stopped = true;
			startPosition = transform.position;
			if (radius == 0)
				radius = GetComponent<CircleCollider2D>().radius;
		}

		void NextNode() {
			currentNode += (move_forward ? 1 : -1);
			if (currentNode == commands.Count) {
				switch (Repeat) {
				case RepeatBehavior.PingPong:
					move_forward = false;
					currentNode = Mathf.Max(currentNode-2, 0);
					break;
				case RepeatBehavior.Loop:
					currentNode = 0;
					break;
				case RepeatBehavior.ResetAndLoop:
					transform.position = startPosition;
					currentNode = 0;
					break;
				default:
					stopped = true;
					break;
				}
			} else if (currentNode == -1) {
				if (move_forward)
					return;
				switch (Repeat) {
				case RepeatBehavior.PingPong:
					move_forward = true;
					currentNode = Mathf.Min(currentNode+2, commands.Count-1);
					break;
				case RepeatBehavior.Loop:
					currentNode = commands.Count-1;
					break;
				case RepeatBehavior.ResetAndLoop:
					transform.position = startPosition;
					currentNode = commands.Count-1;
					break;
				default:
					stopped = true;
					break;
				}
			}
		}

		void Tick() {
			if (stopped)
				return;

			switch(commands[currentNode].command_type) {
			case (MovementCommand.CommandTypes.Wait):
				waitTime += Time.deltaTime;
				if (waitTime >= commands[currentNode].myFloat1) {
					waitTime = 0;
					NextNode();
					return;
				}
				break;
			case (MovementCommand.CommandTypes.Move):
				if (!targetSet) {
					switch(commands[currentNode].move_type) {
					case MovementCommand.MoverTypes.Absolute:
						target = commands[currentNode].myVector2;
						break;
					case MovementCommand.MoverTypes.Relative:
						target = transform.position;
						target += (Vector3)commands[currentNode].myVector2;
						break;
					case MovementCommand.MoverTypes.To_transform:
						target = commands[currentNode].transformTarget.position;
						break;
					case MovementCommand.MoverTypes.obj_name:
						target = commands[currentNode].transformTarget.position;
						break;
					}
					target.z = this.transform.position.z;
					targetSet = true;
				}
				if (commands[currentNode].myBool)
					target = (Vector3)commands[currentNode].transformTarget.position;
				int result = Move( target );
				if (result == 1 || (result == 2 && ignore_impossible)) {
					targetSet = false;
					NextNode();
					return;
				} // else if (result == 2 && giveup)
				break;
			case (MovementCommand.CommandTypes.GoTo):
				currentNode = commands[currentNode].myInt;
				break;
			case (MovementCommand.CommandTypes.Teleport):
				transform.position = (Vector3)commands[currentNode].myVector2;
				NextNode();
				break;
			default:
				Debug.LogWarning("Unknown command type: " + commands[currentNode].command_type.ToString());
				break;
			}
		}

		// private?
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



		void OnDrawGizmosSelected() {
			Vector3 lastPos = startPosition;
			for (int x = 0; x < commands.Count; x++) {
				Gizmos.color = Color.blue;
				if (commands[x].command_type == MovementCommand.CommandTypes.Teleport) {
					Gizmos.color = Color.yellow;
					BKB_RPG.DebugHelpers.DrawArrow(lastPos, (Vector3)commands[x].myVector2);
					lastPos = (Vector3)commands[x].myVector2;
					continue;
				}
				if (commands[x].command_type != MovementCommand.CommandTypes.Move)
					continue;
				Vector3 target = lastPos;
				switch (commands[x].move_type) {
				case MovementCommand.MoverTypes.Relative:
					target += (Vector3)commands[x].myVector2;
					break;
				case MovementCommand.MoverTypes.Absolute:
					Gizmos.color = Color.red;
					target = (Vector3)commands[x].myVector2;
					break;
				case MovementCommand.MoverTypes.To_transform:
				case MovementCommand.MoverTypes.obj_name:
					Gizmos.color = Color.green;
					if (commands[x].transformTarget != null)
						target = commands[x].transformTarget.position;
					break;
				default:
					continue;
				}
				BKB_RPG.DebugHelpers.DrawArrow(lastPos, target);
				lastPos = target;
			}
			
		}

	}
}
