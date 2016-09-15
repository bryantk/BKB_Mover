using UnityEngine;
using System.Collections;

namespace BKB_RPG {
    public class Player : Entity {

        public float actionRate = 0.25f;

        private float _actionNext;
        private Mover bkb_mover;

        void Awake() {
            
        }

        public override void iSetup(object parent) {
            tag = "Player";
            bkb_mover = GetComponent<Mover>();
            InputMaster.moveEvent += Move;
            InputMaster.okButtonEvent += ActionPressed;
            base.iSetup(null);
            bkb_mover.iSetup(null);
        }

        public override void OnCollision(Transform hit) {
            if (Time.time < _next)
                return;
            _next = Time.time + actionRate;
            Entity other = hit.GetComponent<Entity>();
            if (other != null)
                other.OnPlayerTouched();
        }

        void OnTriggerEnter2D(Collider2D other) {
            // TODO - create 'on player touch' component that directs to cached events
            if (other.tag == "TP")
            {
                Teleporter t = other.GetComponent<Teleporter>();
                if (t != null)
                    t.Teleport();
            }
        }

        public void Move(object sender, InfoEventArgs<Vector2> e) {
            if (Paused)
                return;
            Vector2 dir = e.info;
            if (dir.magnitude != 0)
            {
                bkb_mover.moving = true;
                bkb_mover.SetAnimation("speed", bkb_mover.animation_rate);
                bkb_mover.SetFacing(Utils.Vector2toAngle(dir));
                bkb_mover.StepTowards((Vector3)dir.normalized + transform.position);
            }
            
        }

        void LateUpdate() {
            bkb_mover.SetAnimation("speed", 0);
            bkb_mover.moving = false;
        }

        public void ActionPressed() {
            print("action!");
            if (Paused)
                return;
            if (Time.time < _actionNext)
                return;
            _actionNext = Time.time + actionRate;
            Vector2 dir = Utils.AngleMagnitudeToVector2(bkb_mover.facing);
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
