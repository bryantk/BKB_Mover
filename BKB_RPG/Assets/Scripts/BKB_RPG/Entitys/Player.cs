using UnityEngine;
using System.Collections;

namespace BKB_RPG {
    public class Player : Entity {

        public float actionRate = 0.25f;

        private float _actionNext;
        private Mover bkb_mover;


        public override void iSetup(object parent) {
            tag = "Player";
            bkb_mover = GetComponent<Mover>();
            myCollider = GetComponent<Collider2D>();
            _animator = GetComponent<Animator>();
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
            bkb_mover.iSetup(this);
        }

        public override void iTick() { }

        public void EnterState()
        {
            InputMaster.moveEvent += Move;
            InputMaster.notMoving += NotMoving;
            InputMaster.okButtonEvent += ActionPressed;
        }

        public void ExitState()
        {
            InputMaster.moveEvent -= Move;
            InputMaster.notMoving -= NotMoving;
            InputMaster.okButtonEvent -= ActionPressed;
        }

        #region Pause + Resume
        public new void iPause() {
            base.iPause();
            bkb_mover.iPause();
        }

        public new void iResume() {
            base.iResume();
            bkb_mover.iResume();
        }
        #endregion

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
            bkb_mover.Step(dir.normalized);
        }

        void NotMoving() {
            bkb_mover.Stop();
        }

        public void ActionPressed() {
            if (Paused)
                return;
            if (Time.time < _actionNext)
                return;
            _actionNext = Time.time + actionRate;
            Vector2 dir = Utils.AngleMagnitudeToVector2(bkb_mover.facing);
            RaycastHit2D hit = Utils.Raycast(transform.position, dir, lookAhead: 0.2f, self: transform, spread: -0.5f);
            if (hit)
            {
                Entity other = hit.transform.GetComponent<Entity>();
                if (other != null)
                {
                    other.OnInputActivated(this);
                }
            }
        }

    }
}
