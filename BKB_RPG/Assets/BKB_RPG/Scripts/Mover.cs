﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SimpleJSON;
using BKB.Collections;

// TODO
// System.Action callback=null

namespace BKB_RPG {
    [RequireComponent(typeof(Entity))]
	[RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [System.Serializable]
	public class Mover : MonoBehaviour, IPauseable, ISavable {
        // enum definitions.
		public enum RepeatBehavior{None, PingPong, Loop, ResetAndLoop}; 
		public enum Speed{Slowest=5, Slower=15, Slow=30, Normal=50, Faster=100, Fastest=200}
        public enum aSpeed { None = 0, Slowest = 5, Slower = 10, Normal = 20, Faster = 30, Fastest = 40 }
        public enum movementDirections { four=4, eight=8, free=360}
        // movement pathing results
        public enum results{Nil = 0, Complete = 1, Hit = 2};

        // Is this Entity moving
        public bool moving;
        // Angle of attempted movement
        public float movementAngle;
        // Angle Sprite is facing (locked to facing options setting)
        public float facing;

        [SerializeField]
        public List<MovementCommand> commands;
        // Command repeat behavior
		public RepeatBehavior repeat = RepeatBehavior.None;
        // Pretty Selector for speed to 'move' at 
		public Speed move_speed = Speed.Normal;
        // Real movment speed (units/tick)
		public float speed { get {return (int)move_speed/1000f;} }
        // Pretty selector for Animation speed multiplier
        public aSpeed animation_speed = aSpeed.Normal;
        public float animation_rate { get { return (int)animation_speed / 20f; } }
        // How many directions can the sprite face?  TODO - use to constrain random/all movement
        public movementDirections directions = movementDirections.four;
        //currentCommand
        public int currentCommandIndex;
        // Facing will not be modified unless false.
        public bool lockFacing = false;
        // Entity has completed move path without a repeat behavior
        public bool compelete = false;
        // Entity is paused
        bool pause = false;

        // TODO
        public bool alwaysAnimate = false;
        public bool clipEnties = false;
        public bool clipAll = false;

        public float speedModifier = 1;

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
        // Do 'Slope' areas affect this mover?
        public bool affectedBySlope = false;
        #endregion

        #region Inspector Drawer Options
        public Vector3 startPosition = Vector3.zero;
        public Vector2 scrollPos;
        public bool showSettings;
        public bool showOptions;
        public bool showQuickCommands = false;
        public bool advanceDebugDraw = true;
        #endregion

        #region mover helpers
        // Wait Command helpers
        float waitTime = 0;
        // Movement Command helpers
        [SerializeField]
        bool targetSet = false;
        [SerializeField]
        Vector3 target;
        // If Entity has 'Pixel' snap script, Mover will stop when within nearest pixel
        float nearestPixel = 0;
        // Used to save last location of mover
        public Vector3 savePosition;
        // Keys to dis-include from JSON
        readonly string[] dropKeys = { "spread", "stop_range", "radius", "ray_density", "slide",
                "reverse", "ignore_impossible", "startPosition", "scrollPos", "showSettings",
                "showOptions", "showQuickCommands", "advanceDebugDraw" };
        #endregion

        #region References for speedy lookups
        Entity entity;
        Animator anim;
        Renderer render;
        #endregion


        #region Interfaces
        public void iPause() {
            pause = true;
            SetAnimation("speed", 0);
        }

        public void iResume() {
            pause = false;
            SetAnimation("speed", animation_rate);
        }

        public string iSave() {
            savePosition = transform.position;
            string json = JsonUtility.ToJson(this);
            JSONNode N = JSON.Parse(json);
            foreach (string str in dropKeys)
            {
                N.Remove(str);
            }
            return N.ToString();
        }

        public void iLoad(string json) {
            JsonUtility.FromJsonOverwrite(json, this);
            transform.position = savePosition;
        }
        #endregion

        void OnComplete() {
            compelete = true;
            // TODO - Call some complete delegates?
        }

		public void Setup(Entity entity) {
            this.entity = entity;
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

            currentCommandIndex = -1;
            NextNode();
        }

        // TODO - Refacter into less
        void NextNode() {
            waitTime = 0;
            targetSet = false;
            currentCommandIndex += (reverse ? -1 : 1);
			if (currentCommandIndex >= commands.Count)
            {
				switch (repeat) {
				case RepeatBehavior.PingPong:
					reverse = true;
					currentCommandIndex = Mathf.Max(currentCommandIndex-2, 0);
					break;
				case RepeatBehavior.Loop:
					currentCommandIndex = 0;
					break;
				case RepeatBehavior.ResetAndLoop:
					transform.position = startPosition;
					currentCommandIndex = 0;
					break;
				default:
                    OnComplete();
                    break;
				}
			} else if (currentCommandIndex < 0) {
				if (!reverse)
					return;
				switch (repeat) {
				case RepeatBehavior.PingPong:
					reverse = false;
					currentCommandIndex = Mathf.Min(currentCommandIndex+2, commands.Count-1);
					break;
				case RepeatBehavior.Loop:
					currentCommandIndex = commands.Count-1;
					break;
				case RepeatBehavior.ResetAndLoop:
					transform.position = startPosition;
					currentCommandIndex = commands.Count-1;
					break;
				default:
                    OnComplete();
                    break;
				}
			}
		}

        public void SetFacing(float value) {
            movementAngle = value;
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


        Vector3 _GetTarget(MovementCommand command) {
            speedModifier = 1;
            Vector3 result = command.target;
            switch (command.move_type)
            {
            case MovementCommand.MoverTypes.Absolute:
                //result = command.target;
                break;
            case MovementCommand.MoverTypes.Relative:
                result = command.target;
                result += transform.position;
                break;
            case MovementCommand.MoverTypes.To_transform:
            case MovementCommand.MoverTypes.ObjName:
                if (command.transformTarget == null)
                    throw new System.Exception(string.Format("Command {0} target not set on object '{1}'", currentCommandIndex, name));
                result = command.transformTarget.position;
                break;
            case MovementCommand.MoverTypes.Angle:
                float angle = Utils.ClampAngle(command.offsetAngle, (int)directions);
                float magnitude = command.maxStep;
                result = Utils.AngleMagnitudeToVector2(facing + angle, magnitude);
                result += transform.position;
                break;
            }
            if (command.random.magnitude > 0)
            {
                if (command.randomType == MovementCommand.RandomTypes.Linear)
                {
                    Vector3 dir = result - transform.position;
                    dir.z = transform.position.z;
                    result = result + dir.normalized * Random.Range(command.random.x, command.random.y);
                }
                else if (command.randomType == MovementCommand.RandomTypes.Area)
                {
                    //TODO deal with random type (random or weighted)
                    result += (Vector3)Utils.RandomAreaByDirection(0, 360, 360 / (int)directions, Random.Range(command.random.x, command.random.y));
                }
            }
            // Do not excced maxStep distance when moving.
            if (command.maxStep > 0 && command.move_type != MovementCommand.MoverTypes.Angle)
            {
                Vector2 dir = (Vector2)result - (Vector2)transform.position;
                dir = dir.normalized * command.maxStep;
                result = transform.position + (Vector3)dir;
            }
            result.z = transform.position.z;
            return result;
        }


        void _MoveCommands() {
            moving = false;
            if (!alwaysAnimate)
                SetAnimation("speed", 0);
            MovementCommand command = commands[currentCommandIndex];
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
                // if facing command, modify target
                if (command.commandType == MovementCommand.CommandTypes.Face)
                {
                    float random = facing;
                    if (command.randomType == MovementCommand.RandomTypes.Linear)
                    {
                        random += Random.Range(command.random.x, command.random.y);
                    }
                    else if (command.randomType == MovementCommand.RandomTypes.Area)
                    {
                        if (Random.value >= 0.5f)
                            random += Random.Range(command.random.x, command.random.y);
                        else
                            random += Random.Range(command.random2.x, command.random2.y);
                    } else
                        random += command.offsetAngle;
                    random = Utils.ClampAngle(random, (int)directions);
                    target = transform.position + (Vector3)Utils.AngleMagnitudeToVector2(random, 1);
                    SetFacing(Utils.AngleBetween(transform.position, target));
                    NextNode();
                    return;
                }
            }
            else if (command.recalculate)
                target = (Vector3)command.transformTarget.position;
            if ((transform.position-target).magnitude > 0.1f)
                SetFacing(Utils.AngleBetween(transform.position, target));
            if (command.instant)
                transform.position = target;
            int result = StepTowards(target, command.withinDistance);
            if (result == 1 || (result == 2 && ignore_impossible)) {
                NextNode();
                return;
            } // else if (result == 2 && giveup)
            SetAnimation("speed", animation_rate);
            moving = true;
        }

        void _BoolCommands() {
            MovementCommand command = commands[currentCommandIndex];
            switch (command.flag)
            {
            case MovementCommand.FlagType.Clip:
            case MovementCommand.FlagType.ClipAll:
                // TODO - 
                // Disable collider
                // turn on bool / set value for Move() to ignore collisions
                // Clip = dont hit entities, but do hit World
                // ClipAll = clip all
                break;
            case MovementCommand.FlagType.IgnoreImpossible:
                ignore_impossible = command.Bool;
                break;
            case MovementCommand.FlagType.AlwaysAnimate:
                alwaysAnimate = command.Bool;
                if (alwaysAnimate)
                    SetAnimation("speed", animation_rate);
                else
                    SetAnimation("speed", 0);
                break;
            case MovementCommand.FlagType.Invisible:
                if (render != null)
                    render.enabled = !command.Bool;
                break;
            case MovementCommand.FlagType.LockFacing:
                lockFacing = command.Bool;
                break;
            case MovementCommand.FlagType.Reverse:
                reverse = command.Bool;
                break;
            // TODO - call IPauseable... also, this command is destructive... (no way for self to recover)
            case MovementCommand.FlagType.Pause:
                compelete = command.Bool;
                break;
            }
        }


        public void Tick() {
			if (compelete || pause|| commands.Count == 0)
				return;

            MovementCommand.CommandTypes command_type = commands[currentCommandIndex].commandType;
            if (command_type == MovementCommand.CommandTypes.Wait)
            {
                waitTime += Time.deltaTime;
                if (waitTime >= commands[currentCommandIndex].time)
                    NextNode();
            }
            else if (command_type == MovementCommand.CommandTypes.Move || command_type == MovementCommand.CommandTypes.Face)
            {
                _MoveCommands();
            }
            else if (command_type == MovementCommand.CommandTypes.Boolean)
            {
                _BoolCommands();
                // This command is a NoOp
                NextNode();
                Tick();
            }
            else if (command_type == MovementCommand.CommandTypes.GoTo)
            {
                currentCommandIndex = commands[currentCommandIndex].gotoId;
                // This command is a NoOp
                Tick();
            }
            else if (command_type == MovementCommand.CommandTypes.Script)
            {
                commands[currentCommandIndex].scriptCalls.Invoke();
                // This command is a NoOp
                NextNode();
                Tick();
            }
            else if (command_type == MovementCommand.CommandTypes.Remove)
            {
                int start = Mathf.Max(currentCommandIndex - commands[currentCommandIndex].gotoId, 0);
                int remove = commands[currentCommandIndex].Bool ? 1 : 0;
                int range = Mathf.Min(commands[currentCommandIndex].gotoId + remove, commands.Count);
                commands.RemoveRange(start, range);
                currentCommandIndex = start - remove;
                // This command is a NoOp
                NextNode();
                Tick();
            }
            else if (command_type == MovementCommand.CommandTypes.Set)
            {
                if (commands[currentCommandIndex].setType == MovementCommand.SetTypes.Speed)
                    move_speed = (Speed)commands[currentCommandIndex].gotoId;
                else
                    animation_speed = (aSpeed)commands[currentCommandIndex].gotoId;
                // This command is a NoOp
                NextNode();
                Tick();
            }
            // ---------------------------------------------
            // DEFINE COMMAND LOGIC HERE
            // ---------------------------------------------
            else
            {
                Debug.LogWarning("Unknown command type: " + command_type.ToString());
            }
        }


        public int StepTowards(Vector3 target, float stopWithin = 0f, bool trySlide=true) {
            Vector2 dir = target - transform.position;
            float mySpeed = speed * speedModifier;
            // within destination?
			if (dir.magnitude <= Mathf.Max(mySpeed, Mathf.Max(nearestPixel, stopWithin))) {
                if (stopWithin == 0)
    				transform.position = target;
				return (int)results.Complete;
			}
			// ray cast
			RaycastHit2D hit = Utils.Raycast(transform.position, dir.normalized, ray_density,
			                                 radius*spread, radius + mySpeed * stop_range, this.transform);
			if (hit && !hit.collider.isTrigger) {
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
                entity.OnCollision(hit.transform);
				return (int)results.Hit;
			}
			// move some
			transform.position += (Vector3)dir.normalized * mySpeed;
			return (int)results.Nil;
		}
    //end class
	}
}
