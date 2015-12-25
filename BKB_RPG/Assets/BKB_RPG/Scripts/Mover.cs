using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace BKB_RPG {
	[RequireComponent(typeof(CircleCollider2D))]
    [System.Serializable]
	public class Mover : MonoBehaviour {
		public enum RepeatBehavior{None, PingPong, Loop, ResetAndLoop}; 
		public enum Speed{Slowest=5, Slower=15, Slow=30, Normal=50, Faster=100, Fastest=200}
        public enum aSpeed { Frozen = 0, Slowest = 1, Slower = 2, Normal = 3, Faster = 4, Fastest = 5 }
        // movement pathing results
        public enum results{Nil = 0, Complete = 1, Hit = 2};

		public List<MovementCommand> commands;

		public RepeatBehavior repeat = RepeatBehavior.None;
		public Speed move_speed = Speed.Normal;
		public float speed {
			get {return (int)move_speed/1000f;}
		}

        public int currentNode;
        public float facing;

        #region Movement Options
        // % spread of raycasts. 1 = flush with collider radius.
        public float spread = 0.75f;
        // % of speed to look ahead for obstacles and stop. 2 = objects 2x speed ahead will caus collision
		public float stop_range = 2;
        // Radius of 'collider'. May be larger or smaller than acctual collider (for sliping into tight spots and what not).
        // Defaults to circle collider radius.
		public float radius = 0;
        // Number of rays to cast for collisions. Starts with '1' in the center, then adds more to each side.
        // More rays yields more percise checking and is a must for larger colliders
		public int ray_density = 2;
        // When encountering an obstacle, should this mover attempt to 'slide' and keep moving?
		public bool slide = false;
        // Looping through commands ++ or --?
		public bool move_forward = true;
        // When true, current move command will exit upon reaching an impossible to move scenario. 
		public bool ignore_impossible = false;
        #endregion

        // TODO
        public bool alwaysAnimate = false;
        public bool lockFacing = false;
        public bool clipEnties = false;
        public bool clipAll = false;
        public aSpeed animation_speed = aSpeed.Normal;
        public float animation_rate {
            get { return (int)move_speed/5; }
        }

        // command helpers
        float waitTime = 0;
        bool targetSet = false;
        Vector3 target;
        int depth = 0;
        float nearestPixel = 0;

        // drawer things
        public Vector3 startPosition = Vector3.zero;
        public Vector2 scrollPos;
        public bool showOptions;
        public bool showQuickCommands = false;

        bool stopped = false;
		Animator anim;


		void Start() {
			Setup();
		}


		// Update is called once per frame
		void Update () {
			Tick();
		}


		void Setup() {
			if (!move_forward && repeat == RepeatBehavior.None)
				stopped = true;
			startPosition = transform.position;
			if (radius == 0)
				radius = GetComponent<CircleCollider2D>().radius;
			anim = GetComponent<Animator>();
            // pixelLock may impose a larger step than move speed.
            PixelLock pixel = GetComponent<PixelLock>();
            if (pixel != null)
                nearestPixel = pixel.resolution;

            currentNode = -1;
            NextNode();
        }


		void NextNode() {
			currentNode += (move_forward ? 1 : -1);
			if (currentNode >= commands.Count) {
				switch (repeat) {
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
			} else if (currentNode <= -1) {
				if (move_forward)
					return;
				switch (repeat) {
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


		void SetAnimation(string name, float value) {
			if (anim == null)
				return;
			anim.SetFloat(name, value);
		}


        void MoveCommands() {
            if (!targetSet) {
                switch (commands[currentNode].move_type) {
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

            facing = Utils.AngleBetween(transform.position, target);
            SetAnimation("facing", facing);
            SetAnimation("speed", speed * 100);

            int result = Move(target);
            if (result == 1 || (result == 2 && ignore_impossible)) {
                targetSet = false;
                NextNode();
                return;
            } // else if (result == 2 && giveup)
        }

		void Tick() {
			if (stopped || commands.Count == 0)
				return;
			SetAnimation("speed", 0);
			switch(commands[currentNode].command_type) {
			case MovementCommand.CommandTypes.Wait:
				waitTime += Time.deltaTime;
				if (waitTime >= commands[currentNode].myFloat1) {
					waitTime = 0;
					NextNode();
					return;
				}
				break;
			case MovementCommand.CommandTypes.Move:
                MoveCommands();
				break;
			case MovementCommand.CommandTypes.GoTo:
				currentNode = commands[currentNode].myInt;
				break;
			case MovementCommand.CommandTypes.Teleport:
				transform.position = (Vector3)commands[currentNode].myVector2;
				NextNode();
				break;
            case MovementCommand.CommandTypes.Script:
                commands[currentNode].myScriptCalls.Invoke();
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
			if (dir.magnitude <= Mathf.Max(speed, nearestPixel)) {
				transform.position = target;
				depth = 0;
				return (int)results.Complete;
			}
			// ray cast
			RaycastHit2D hit = Utils.Raycast(transform.position, dir.normalized, ray_density,
			                                 radius*spread, radius+speed*stop_range, this.transform);
			if (hit) {
                // DEBUG only
                if (EditorApplication.isPlaying && hit.transform == this.transform) {
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
