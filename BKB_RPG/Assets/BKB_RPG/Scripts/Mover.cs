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
		public bool reverse = false;
        // When true, current move command will exit upon reaching an impossible to move scenario. 
		public bool ignore_impossible = false;
        #endregion

        // Facing will not be modified unless false.
        public bool lockFacing = false;


        // TODO
        public bool steppingAnimation = false;
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
        public bool showSettings;
        public bool showOptions;
        public bool showQuickCommands = false;

        bool paused = false;
        public bool Pause {
            get { return paused; }
            set {
                _Pause(value);
            }
        }
		Animator anim;
        Renderer render;


		void Start() {
			Setup();
		}


		// Update is called once per frame
		void Update () {
			Tick();
		}


		void Setup() {
			if (reverse && repeat == RepeatBehavior.None)
				Pause = true;
			startPosition = transform.position;
			if (radius == 0)
				radius = GetComponent<CircleCollider2D>().radius;
			anim = GetComponent<Animator>();
            render = GetComponent<Renderer>();
            // pixelLock may impose a larger step than move speed.
            PixelLock pixel = GetComponent<PixelLock>();
            if (pixel != null)
                nearestPixel = pixel.resolution;

            currentNode = -1;
            NextNode();
        }


		void NextNode() {
            waitTime = 0;
            targetSet = false;
            currentNode += (reverse ? -1 : 1);
			if (currentNode >= commands.Count) {
				switch (repeat) {
				case RepeatBehavior.PingPong:
					reverse = true;
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
					Pause = true;
					break;
				}
			} else if (currentNode <= -1) {
				if (!reverse)
					return;
				switch (repeat) {
				case RepeatBehavior.PingPong:
					reverse = false;
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
					Pause = true;
					break;
				}
			}
		}

        void SetFacing(float value) {
            if (lockFacing)
                return;
            facing = value;
            SetAnimation("facing", facing);
        }


		void SetAnimation(string name, float value) {
			if (anim == null)
				return;
			anim.SetFloat(name, value);
		}

        void _Pause(bool on) {
            if (paused == on)
                return;
            paused = on;
            print("set paused: " + on);
        }

        void _MoveCommands() {
            MovementCommand_Move command = (MovementCommand_Move)commands[currentNode];
            if (!targetSet)
            {
                switch (command.move_type) {
                    case MovementCommand_Move.MoverTypes.Absolute:
                        target = command.target;
                        break;
                    case MovementCommand_Move.MoverTypes.Relative:
                        target = transform.position;
                        target += (Vector3)command.target;
                        break;
                    case MovementCommand_Move.MoverTypes.To_transform:
                        target = command.transformTarget.position;
                        break;
                    case MovementCommand_Move.MoverTypes.ObjName:
                        target = command.transformTarget.position;
                        break;
                }
                target.z = transform.position.z;
                targetSet = true;
            }
            else if (command.recalculate)
                target = (Vector3)command.transformTarget.position;
            if (command.instant)
            {
                transform.position = target;
                NextNode();
                return;
            }


            SetFacing( Utils.AngleBetween(transform.position, target) );
            SetAnimation("speed", speed * 100);

            int result = Move(target, command.withinDistance);
            if (result == 1 || (result == 2 && ignore_impossible)) {
                NextNode();
            } // else if (result == 2 && giveup)
        }

        void _BoolCommands() {
            MovementCommand_Bool command = (MovementCommand_Bool)commands[currentNode];
            switch (command.flag)
            {
            case MovementCommand_Bool.FlagType.AlwaysAnimate:

                break;
            case MovementCommand_Bool.FlagType.NeverAnimate:

                break;
            case MovementCommand_Bool.FlagType.Clip:

                break;
            case MovementCommand_Bool.FlagType.ClipAll:

                break;
            case MovementCommand_Bool.FlagType.IgnoreImpossible:
                ignore_impossible = command.Bool;
                break;
            case MovementCommand_Bool.FlagType.Invisible:
                if (render != null)
                    render.enabled = !command.Bool;
                break;
            case MovementCommand_Bool.FlagType.LockFacing:
                lockFacing = command.Bool;
                break;
            case MovementCommand_Bool.FlagType.Reverse:
                reverse = command.Bool;
                break;
            case MovementCommand_Bool.FlagType.Pause:
                Pause = command.Bool;
                break;
            }
        }

        void Tick() {
			if (Pause || commands.Count == 0)
				return;
			SetAnimation("speed", 0);
            var command_type = commands[currentNode].GetType();
            if (command_type == typeof(MovementCommand_Wait))
            {
                waitTime += Time.deltaTime;
                if (waitTime >= ((MovementCommand_Wait)commands[currentNode]).time)
                    NextNode();
            }
            else if (command_type == typeof(MovementCommand_Move))
            {
                _MoveCommands();
            }
            else if (command_type == typeof(MovementCommand_Bool))
            {
                _BoolCommands();
                // This command is a NoOp
                NextNode();
                Tick();
            }
            else if (command_type == typeof(MovementCommand_GOTO))
            {
                currentNode = ((MovementCommand_GOTO)commands[currentNode]).gotoId;

            }
            else if (command_type == typeof(MovementCommand_Script))
            {
                ((MovementCommand_Script)commands[currentNode]).events.Invoke();
                // This command is a NoOp
                NextNode();
                Tick();
            }
            else
            {
                Debug.LogWarning("Unknown command type: " + command_type.ToString());
            }

            //case MovementCommand.CommandTypes.Teleport:
            //	transform.position = (Vector3)commands[currentNode].myVector2;
            //	NextNode();
            //	break;
        }

        // private?
        public virtual int Move(Vector3 target, float stopWithin = 0f) {
			depth++;
			Vector2 dir = target - transform.position;
			// within destination?
			if (dir.magnitude <= Mathf.Max(speed, Mathf.Max(nearestPixel, stopWithin))) {
                if (stopWithin == 0)
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
					return Move (transform.position + (Vector3)t.normalized, stopWithin);
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
