using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using UnityEditor.Animations;

// TODO
// System.Action callback=null

namespace BKB_RPG {
    [RequireComponent(typeof(Entity))]
	[RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [System.Serializable]
	public class Mover : MonoBehaviour, IPauseable, ISavable, ITick {
        // enum definitions.
		public enum RepeatBehavior{None, PingPong, Loop, ResetAndLoop}; 
		public enum Speed{Slowest=5, Slower=15, Slow=30, Normal=50, Faster=100, Fastest=200}
        public enum aSpeed { None = 0, Slowest = 5, Slower = 10, Normal = 20, Faster = 30, Fastest = 40 }
        public enum movementDirections { four=4, eight=8, free=360}
        // movement pathing results
        public enum results{Nil = 0, Complete = 1, Hit = 2};

        // Is this Entity moving
        public bool moving;
        // Angle Sprite is facing (locked to facing options setting)
        public float facing;

        [SerializeField]
        public List<MovementCommand> commands;
        // Command repeat behavior
		public RepeatBehavior repeat = RepeatBehavior.None;
        // Pretty Selector for speed to 'move' at 
		public Speed move_speed = Speed.Normal;
        // Real movment speed (units/tick)
		public float speed { get {return (int)move_speed/10;} }
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
        // Stored speed modifier - used for moving on slopes
        public Vector2 forces = Vector2.zero;
        public Vector2 constantForces = Vector2.zero;
        //
        public int collisionLayerMask = Physics2D.DefaultRaycastLayers;
        // Sync command - do not advance until message from elsewhere
        public bool awaitSync = false;

        // TODO
        public bool alwaysAnimate = false;
        public bool clipEnties = false;
        public bool clipAll = false;

        public string moverName;

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

        // Looping through commands ++ or --?
        public bool reverse = false;
        // When true, current move command will exit upon reaching an impossible to move scenario. 
        public bool ignore_impossible = true;
        // Do 'Slope' areas affect this mover?
        public bool affectedBySlope = true;
        #endregion

        #region Inspector Drawer Options
        public Vector3 startPosition = Vector3.zero;
        
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
        readonly string[] dropKeys = { "spread", "stop_range", "radius", "ray_density",
                "reverse", "ignore_impossible", "startPosition" };
        #endregion

        #region References for speedy lookups
        // If not set in inspector, object will assign to animator on itself

        Animator _anim;
        Entity entity;
        private Rigidbody2D rigidBody;
        Renderer render;
        #endregion
        Callback _callback;

        #region Interfaces
        public void iPause() {
            pause = true;
            SetAnimation("speed", 0);
            Stop();
            rigidBody.velocity = Vector2.zero;
        }

        public void iResume() {
            pause = false;
            SetAnimation("speed", animation_rate);
            rigidBody.velocity = constantForces;
        }

        public string iSave()
        {
            return Save();
        }

        public string Save(params string[] keys)
        {
            savePosition = transform.position;
            string json = JsonUtility.ToJson(this);
            JSONNode N = JSON.Parse(json);
            foreach (string str in dropKeys)
            {
                N.Remove(str);
            }
            foreach (string str in keys)
            {
                N.Remove(str);
            }
            savePosition = default(Vector3);
            return N.ToString();
        }

        public void iLoad(string json) {
            JsonUtility.FromJsonOverwrite(json, this);
            if (savePosition != default(Vector3))
                transform.position = savePosition;
        }
        #endregion

        void OnComplete() {
            compelete = true;
            if (_callback != null)
                _callback();
            // TODO - Call some complete delegates?
        }

        public void SetCallback(Callback callback)
        {
            _callback = callback;
        }

        void OnCycleComplete() {
            // TODO - Call onCycleComplete delegate/call back
        }

		public void iSetup(object entity) {
            this.entity = entity as Entity;
		    _anim = this.entity._animator;
            SetAnimation("8-dir", ((int)directions >= 8));
            if (alwaysAnimate)
                SetAnimation("speed", animation_rate);
            else
                SetAnimation("speed", 0);
            currentCommandIndex = -1;
            NextNode();
            // break if already setup
            if (rigidBody != null)
                return;
            // TODO - move this up? changes use case
            startPosition = transform.position;
            // Resolve 'magic strings'
            foreach (MovementCommand command in commands)
            {
                if (command.targetName.ToUpper() == "PLAYER")
                    command.transformTarget = GameMaster.GetPlayerTransform();
            }
		    if (radius == 0)
		    {
                BoxCollider2D box = GetComponent<BoxCollider2D>();
                if (GetComponent<CircleCollider2D>() != null)
                    radius = GetComponent<CircleCollider2D>().radius;
                else
    		        radius = box != null ? Mathf.Max(box.size.x, box.size.y) / 2 : 0.5f;
            }
            render = GetComponent<Renderer>();
		    rigidBody = GetComponent<Rigidbody2D>();
		    rigidBody.isKinematic = false;
		    rigidBody.freezeRotation = true;
            // pixelLock may impose a larger step than move speed.
            PixelLock pixel = GetComponent<PixelLock>();
		    nearestPixel = pixel != null ? pixel.resolution : 0.03f;
        }

        void NextNode() {
            waitTime = 0;
            targetSet = false;
            currentCommandIndex += (reverse ? -1 : 1);
			if (currentCommandIndex >= commands.Count)
            {
                OnCycleComplete();
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
                OnCycleComplete();
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
            if (lockFacing)
                return;
            facing = Utils.ClampAngle(value, (int)directions);
            Vector2 f = Utils.AngleMagnitudeToVector2(facing);
            SetAnimation("x", f.x);
            SetAnimation("y", f.y);
        }

        public void SetAnimation(string name, float value) {
			if (_anim == null)
				return;
			_anim.SetFloat(name, value);
		}

        void SetAnimation(string name, bool value) {
            if (_anim == null)
                return;
            _anim.SetBool(name, value);
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
                    float random = Utils.AngleBetween(transform.position, target);
                    // Apply randoms
                    if (command.randomType == MovementCommand.RandomTypes.Linear)
                        random += Random.Range(command.random.x, command.random.y);
                    else if (command.randomType == MovementCommand.RandomTypes.Area)
                    {
                        Vector2 selectedRange = Random.value >= 0.5f ? command.random : command.random2;
                        random += Random.Range(selectedRange.x, selectedRange.y);
                    }
                    else
                        random += command.offsetAngle;
                    random = Utils.ClampAngle(random, (int)directions);
                    SetFacing(random);
                    NextNode();
                    return;
                }
            }
            else if (command.recalculate && (command.move_type == MovementCommand.MoverTypes.To_transform || command.move_type == MovementCommand.MoverTypes.ObjName))
                target = (Vector3)command.transformTarget.position;
            if ((transform.position-target).magnitude > 0.1f)
                SetFacing(Utils.AngleBetween(transform.position, target));
            if (command.instant)
                transform.position = target;
            int result = StepTowards(target, Mathf.Max(nearestPixel, command.withinDistance, speed * 0.02f));
            if (result == 1 || (result == 2 && ignore_impossible)) {
                Stop();
                NextNode();
                return;
            }
            SetAnimation("speed", animation_rate);
            moving = true;
        }

        Vector3 _GetTarget(MovementCommand command) {
            Vector3 result = command.target;
            switch (command.move_type)
            {
            case MovementCommand.MoverTypes.Absolute:
                break;
            case MovementCommand.MoverTypes.Relative:
                result += transform.position;
                break;
            case MovementCommand.MoverTypes.ObjName:
            case MovementCommand.MoverTypes.To_transform:
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
                    int randomAngle = Utils.RandomAngleWithinArc(0, 360, 360 / (int)directions);
                    result += (Vector3)Utils.AngleMagnitudeToVector2(randomAngle, Random.Range(command.random.x, command.random.y));
                }
            }
            // Do not excced maxStep distance when moving.
            if (command.maxStep > 0 && command.move_type != MovementCommand.MoverTypes.Angle)
            {
                Vector2 dir = (Vector2)result - (Vector2)transform.position;
                if (dir.magnitude > command.maxStep)
                    dir = dir.normalized * command.maxStep;
                result = transform.position + (Vector3)dir;
            }
            result.z = transform.position.z;
            return result;
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
                SetAnimation("speed", alwaysAnimate ? animation_rate : 0);
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
            }
        }


        public void iTick()
        {
            if (compelete || pause|| commands.Count == 0)
				return;

            MovementCommand command = commands[currentCommandIndex];
            switch (command.commandType)
            {
            case MovementCommand.CommandTypes.Wait:
                waitTime += Time.deltaTime;
                if (waitTime >= commands[currentCommandIndex].time)
                    NextNode();
                break;
            case MovementCommand.CommandTypes.Move:
            case MovementCommand.CommandTypes.Face:
                _MoveCommands();
                break;
            case MovementCommand.CommandTypes.Boolean:
                _BoolCommands();
                // This command is a NoOp
                NextNode();
                iTick();
                break;
            case MovementCommand.CommandTypes.GoTo:
                currentCommandIndex = command.int_1;
                // This command is a NoOp
                iTick();
                break;
            case MovementCommand.CommandTypes.Script:
                command.scriptCalls.Invoke();
                // This command is a NoOp
                NextNode();
                iTick();
                break;
            case MovementCommand.CommandTypes.Remove:
                int start = Mathf.Max(currentCommandIndex - command.int_1, 0);
                int remove = command.Bool ? 1 : 0;
                int range = Mathf.Min(command.int_1 + remove, commands.Count);
                commands.RemoveRange(start, range);
                currentCommandIndex = start - remove;
                // This command is a NoOp
                NextNode();
                iTick();
                break;
            case MovementCommand.CommandTypes.Set:
                if (command.setType == MovementCommand.SetTypes.Speed)
                    move_speed = (Speed)command.int_1;
                else
                    animation_speed = (aSpeed)command.int_1;
                // This command is a NoOp
                NextNode();
                iTick();
                break;
            case MovementCommand.CommandTypes.Note:
                // This command is a NoOp
                NextNode();
                iTick();
                break;
            case MovementCommand.CommandTypes.Sync:
                if (command.int_1 == 0)
                {
                    awaitSync = true;
                    command.commandType = MovementCommand.CommandTypes.WaitSync;
                }
                else if (command.int_1 == 1)
                {
                    if (command.moverTarget == null)
                        command.moverTarget = this;
                    command.moverTarget.awaitSync = false;
                    NextNode();
                    iTick();
                }
                break;
            case MovementCommand.CommandTypes.WaitSync:
                if (awaitSync)
                    return;
                command.commandType = MovementCommand.CommandTypes.Sync;
                NextNode();
                iTick();
                break;
            // ---------------------------------------------
            // DEFINE COMMAND LOGIC HERE
            // ---------------------------------------------
            default:
                Debug.LogWarning("Unknown command type: " + command.commandType.ToString());
                return;
            }
        }

        public int StepTowards(Vector3 targetLocation, float within = 0)
        {
            Vector2 dir = targetLocation - transform.position;
            if (dir.magnitude <= within)
                return (int) results.Complete;
            dir = dir.normalized;
            return Step(dir) ? (int) results.Nil : (int)results.Hit;
        }

        public void Stop()
        {
            SetAnimation("speed", 0);
            moving = false;
            if (rigidBody == null)
                return;
            rigidBody.velocity = constantForces;
        }

        public bool Step(Vector3 dir)
        {
            if (pause)
                return false;
            RaycastHit2D hit = Utils.Raycast((transform.position), dir, radius, ray_density, self: transform, layers: 1);
            SetFacing(Utils.Vector2toAngle(dir));
            if (hit && !hit.collider.isTrigger)
            {
                entity.OnCollision(hit.transform);
                Stop();
                return false;
            }
            SetAnimation("speed", animation_rate);
            moving = true;
            rigidBody.velocity = dir * speed;
            if (forces != Vector2.zero)
            {
                float angle = Mathf.RoundToInt(Vector2.Angle(rigidBody.velocity, forces));
                angle = (angle - 90) / -90f;
                Vector2 result = forces * Mathf.Abs(angle);
                // If walking against forces
                if (angle < 0)
                {
                    // No bouncing against slopes
                    if (result.magnitude >= rigidBody.velocity.magnitude)
                    {
                        rigidBody.velocity = Vector2.zero;
                        result = Vector2.zero;
                    }
                    rigidBody.velocity += result;
                }
                else
                {
                    // Limit max speed increase to 2X event speed
                    if (result.magnitude * 2f >= rigidBody.velocity.magnitude)
                    {
                        result = rigidBody.velocity * 2;
                    }
                    rigidBody.velocity += result;
                }
            }
            rigidBody.velocity += constantForces;
            // TODO - Call to OnStep functionality? (show foot steps in snow, drit kickup, etc.)
            return true;
        }

        //end class
        }
}
