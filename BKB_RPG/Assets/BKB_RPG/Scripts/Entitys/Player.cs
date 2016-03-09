using UnityEngine;
using System.Collections;

namespace BKB_RPG {
    public class Player : Entity {

        public float actionRate = 0.25f;
        protected float _actionNext;

        public static Player _instance { get; private set; }
        public virtual void Awake() {
            tag = "Player";
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
                OnAwake();
            }
            else {
                Debug.LogWarning("MoverMaster already exists, deleting.");
                Destroy(this.gameObject);
                return;
            }
        }

        void OnDestroy() {
            if (_instance == this) _instance = null;
        }

        void OnAwake() {
            rate = 0.25f;
            InputMaster.moveEvent += Move;
        }

        public override void OnCollision(Transform hit) {
            if (Time.time < _next)
                return;
            _next = Time.time + rate;
            Entity other = hit.GetComponent<Entity>();
            if (other != null)
                other.OnPlayerTouched();
        }

        // TODO - move to input manager
        void Update() {
            if (bkb_mover.commands.Count > 0)
                return;

            if (Input.GetKey(KeyCode.E))
                ActionPressed();
            /*
            Vector2 dir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            bkb_mover.moving = false;
            if (dir.magnitude != 0)
            {
                bkb_mover.moving = true;
                bkb_mover.SetFacing(BKB_RPG.Utils.Vector2toAngle(dir));
                bkb_mover.StepTowards((Vector3)dir.normalized + transform.position);
            }
            */
        }

        public void Move(object sender, InfoEventArgs<Vector2> e) {
            Vector2 dir = e.info;
            bkb_mover.moving = false;
            if (dir.magnitude != 0)
            {
                bkb_mover.moving = true;
                bkb_mover.SetFacing(BKB_RPG.Utils.Vector2toAngle(dir));
                bkb_mover.StepTowards((Vector3)dir.normalized + transform.position);
            }
        }

        public void ActionPressed() {
            if (Time.time < _actionNext)
                return;
            _actionNext = Time.time + actionRate;
            Vector2 dir = Utils.AngleMagnitudeToVector2(bkb_mover.facing, 1);
            RaycastHit2D hit = Utils.Raycast(transform.position, dir, 2, 0.25f, 0.75f, this.transform);
            if (hit)
            {
                Entity other = hit.transform.GetComponent<Entity>();
                if (other != null)
                {
                    other.OnInputActivated();
                }
            }
        }

    }
}
