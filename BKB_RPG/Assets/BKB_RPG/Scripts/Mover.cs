using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace BKB_RPG {
	[RequireComponent(typeof(CircleCollider2D))]
    [System.Serializable]
	public class Mover : MonoBehaviour {
        // enum definitions.
		public enum RepeatBehavior{None, PingPong, Loop, ResetAndLoop}; 
		public enum Speed{Slowest=5, Slower=15, Slow=30, Normal=50, Faster=100, Fastest=200}
        public enum aSpeed { None = 0, Slowest = 5, Slower = 10, Normal = 20, Faster = 30, Fastest = 40 }
        public enum movementDirections { four=4, eight=8, free=360}
        // movement pathing results
        public enum results{Nil = 0, Complete = 1, Hit = 2};

		public List<MovementCommand> commands;
        // Move speed
		public RepeatBehavior repeat = RepeatBehavior.None;
		public Speed move_speed = Speed.Normal;
		public float speed {
			get {return (int)move_speed/1000f;}
		}
        // Animation speed multiplier
        public aSpeed animation_speed = aSpeed.Normal;
        public float animation_rate {
            get { return (int)animation_speed / 20f; }
        }

        public movementDirections directions = movementDirections.four;
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
        public bool alwaysAnimate = false;
        public bool clipEnties = false;
        public bool clipAll = false;
        

        // Wait Command helpers
        float waitTime = 0;

        // Movement Command helpers
        bool targetSet = false;
        Vector3 target;
        float nearestPixel = 0;

        #region Inspector Drawer Options
        public Vector3 startPosition = Vector3.zero;
        public Vector2 scrollPos;
        public bool showSettings;
        public bool showOptions;
        public bool showQuickCommands = false;
        public bool advanceDebugDraw = true;
        #endregion

        // TODO - correct pasue implementation. Use Ipausaable
        bool paused = false;
        public bool Pause {
            get { return paused; }
            set {
                _Pause(value);
            }
        }

        #region References for speedy lookups
        Animator anim;
        Renderer render;
        #endregion
        public int myID = 0;


		public void Setup() {
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

            SetAnimation("8-dir", ((int)directions >= 8));
            if (alwaysAnimate)
                SetAnimation("speed", animation_rate);
            else
                SetAnimation("speed", 0);

            currentNode = -1;
            NextNode();
        }


        // TODO - Refacter into less
        void NextNode() {
            waitTime = 0;
            targetSet = false;
            currentNode += (reverse ? -1 : 1);
			if (currentNode >= commands.Count)
            {
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
			} else if (currentNode < 0) {
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
            facing = Utils.ClampAngle(value, (int)directions);
            Vector2 f = Utils.AngleMagnitudeToVector2(facing, 1);
            SetAnimation("x", f.x);
            SetAnimation("y", f.y);
        }


		void SetAnimation(string name, float value) {
			if (anim == null)
				return;
			anim.SetFloat(name, value);
		}

        void SetAnimation(string name, bool value) {
            if (anim == null)
                return;
            anim.SetBool(name, value);
        }

        // TODO - use IPausable... not this
        void _Pause(bool on) {
            if (paused == on)
                return;
            paused = on;
        }

        Vector3 _GetTarget(MovementCommand_Move command) {
            Vector3 result = command.target;
            switch (command.move_type)
            {
            case MovementCommand_Move.MoverTypes.Absolute:
                //result = command.target;
                break;
            case MovementCommand_Move.MoverTypes.Relative:
                result = command.target;
                result += transform.position;
                break;
            case MovementCommand_Move.MoverTypes.To_transform:
            case MovementCommand_Move.MoverTypes.ObjName:
                if (command.transformTarget == null)
                    throw new System.Exception(string.Format("Command {0} target not set on object '{1}'", currentNode, name));
                result = command.transformTarget.position;
                result = command.transformTarget.position;
                break;
            case MovementCommand_Move.MoverTypes.Angle:
                // TODO - Implement random angle
                //  offset current
                //      lock to 4, 8, free directions
                //  random
                //      lock to 4, 8, free directions
                float magnitude = command.maxStep;
                // add random direction
                result = Utils.AngleMagnitudeToVector2(facing + command.offsetAngle, magnitude);
                result += transform.position;
                break;
            }
            if (command.random.magnitude > 0)
            {
                if (command.randomType == MovementCommand_Move.RandomTypes.Linear)
                {
                    Vector3 dir = result - transform.position;
                    dir.z = transform.position.z;
                    result = result + dir.normalized * Random.Range(command.random.x, command.random.y);
                }
                else if (command.randomType == MovementCommand_Move.RandomTypes.Area)
                {
                    //TODO deal with random type (random or weighted)
                    result += (Vector3)Utils.RandomAreaByDirection(0, 360, 360 / (int)directions, Random.Range(command.random.x, command.random.y));
                }
            }
            // Do not excced maxStep distance when moveing.
            if (command.maxStep > 0 && command.move_type != MovementCommand_Move.MoverTypes.Angle)
            {
                Vector2 dir = (Vector2)result - (Vector2)transform.position;
                dir = dir.normalized * command.maxStep;
                result = transform.position + (Vector3)dir;
            }
            result.z = transform.position.z;
            return result;
            
        }


        void _MoveCommands() {
            if (!alwaysAnimate)
                SetAnimation("speed", 0);
            MovementCommand_Move command = (MovementCommand_Move)commands[currentNode];
            if (!targetSet)
            {
                try
                {
                    target = _GetTarget(command);
                    targetSet = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e, this);
                    return;
                }
            }
            else if (command.recalculate)
                target = (Vector3)command.transformTarget.position;
            SetFacing(Utils.AngleBetween(transform.position, target));
            // If a 'facingCommand' variant, do not poceed to actual movement code.
            if (command.facingCommand)
            {
                NextNode();
                return;
            }
            if (command.instant)
                transform.position = target;
            int result = StepTowards(target, command.withinDistance);
            if (result == 1 || (result == 2 && ignore_impossible)) {
                NextNode();
                return;
            } // else if (result == 2 && giveup)
            SetAnimation("speed", animation_rate);
        }

        void _BoolCommands() {
            MovementCommand_Bool command = (MovementCommand_Bool)commands[currentNode];
            switch (command.flag)
            {
            case MovementCommand_Bool.FlagType.Clip:
            case MovementCommand_Bool.FlagType.ClipAll:
                // TODO - 
                // Disable collider
                // turn on bool / set value for Move() to ignore collisions
                // Clip = dont hit entities, but do hit World
                // ClipAll = clip all
                break;
            case MovementCommand_Bool.FlagType.IgnoreImpossible:
                ignore_impossible = command.Bool;
                break;
            case MovementCommand_Bool.FlagType.AlwaysAnimate:
                alwaysAnimate = command.Bool;
                if (alwaysAnimate)
                    SetAnimation("speed", animation_rate);
                else
                    SetAnimation("speed", 0);
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
            // TODO - call IPauseable... also, this command is destructive... (no way for self to recover)
            case MovementCommand_Bool.FlagType.Pause:
                Pause = command.Bool;
                break;
            }
        }


        public void Tick() {
			if (Pause || commands.Count == 0)
				return;
            
            var command_type = commands[currentNode].GetType();
            if (command_type == typeof(MovementCommand_Wait))
            {
                waitTime += Time.deltaTime;
                if (waitTime >= ((MovementCommand_Wait)commands[currentNode]).time)
                    NextNode();
            }
            else if (command_type == typeof(MovementCommand_Move) || command_type == typeof(MovementCommand_Face))
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
                // This command is a NoOp
                Tick();
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
        }

        // private?
        public int StepTowards(Vector3 target, float stopWithin = 0f, bool trySlide=true) {
			Vector2 dir = target - transform.position;
			// within destination?
			if (dir.magnitude <= Mathf.Max(speed, Mathf.Max(nearestPixel, stopWithin))) {
                if (stopWithin == 0)
    				transform.position = target;
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

				if (slide && trySlide) {
					Vector3 t = Vector3.Cross(dir.normalized, hit.normal);
					t = Vector3.Cross(hit.normal, t);
					t.z = 0;
					return StepTowards (transform.position + (Vector3)t.normalized, stopWithin, false);
				}
				return (int)results.Hit;
			}
			// move some
			transform.position += (Vector3)dir.normalized * speed;
			return (int)results.Nil;
		}


	}
}
