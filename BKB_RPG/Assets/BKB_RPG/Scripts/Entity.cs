﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace BKB_RPG {
    public class Entity : MonoBehaviour, IPauseable {

        public delegate void NullDelegate();
        NullDelegate tickDelegate;

        // TODO - Each and Each with seperate lists?
        public enum TriggerBehaviour { OnButtonPress, PlayerTouch, EventTouch, Always, Each};
        public TriggerBehaviour behaviour = TriggerBehaviour.OnButtonPress;

        public float rate = 0.25f;
        protected float _next;
        public UnityEvent onActivate;

        // TODO - Am I running an event? Don't run more if I am.
        public bool Processing = false;

        // readonly
        public bool Paused;

        protected Mover bkb_mover;
        private Enemy bkb_enemy;

        /// <summary>
        /// Poll self for all atached scripts 'entity' manages
        /// </summary>
        public void Setup() {
            bkb_enemy = GetComponent<Enemy>();
            bkb_mover = GetComponent<Mover>();
            if (bkb_mover != null)
            {
                bkb_mover.Setup(this);
                tickDelegate += bkb_mover.Tick;
            }
        }

        public void Tick() {
            if (tickDelegate != null)
                tickDelegate();
        }

        #region Pause + Resume
        public void iPause() {
            Paused = true;
            if (bkb_mover != null)
                bkb_mover.iPause();
        }

        public void iResume() {
            Paused = false;
            if (bkb_mover != null)
                bkb_mover.iResume();
        }
        #endregion

        public virtual void OnCollision(Transform hit) {
            if (!(behaviour == TriggerBehaviour.EventTouch || behaviour == TriggerBehaviour.Each))
                return;
            if (Time.time < _next)
                return;
            _next = Time.time + rate;
            Entity other = hit.GetComponent<Entity>();

            onActivate.Invoke();
            // TODO - Always do this?
            if (bkb_enemy != null)
                bkb_enemy.Battle(false);
            print("I, " + this.name + ", hit a thing");
        }

        public void OnPlayerTouched() {
            if (!(behaviour == TriggerBehaviour.PlayerTouch || behaviour == TriggerBehaviour.Each))
                return;
            onActivate.Invoke();
            // TODO - Always do this?
            if (bkb_enemy != null)
                bkb_enemy.Battle(true);
            print("Player hit me, " + this.name);
        }

        public void OnInputActivated() {
            if (!(behaviour == TriggerBehaviour.OnButtonPress || behaviour == TriggerBehaviour.Each))
                return;
            onActivate.Invoke();
            print("Player activated me, " + this.name);
        }


    }
}