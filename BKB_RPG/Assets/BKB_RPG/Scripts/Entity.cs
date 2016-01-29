using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace BKB_RPG {
    public class Entity : MonoBehaviour {

        public delegate void NullDelegate();
        NullDelegate tickDelegate;

        public float rate = 0.25f;
        float _next;
        public UnityEvent onCollision;


        private Mover bkb_mover;


        /// <summary>
        /// Poll self for all atached scripts 'entity' manages
        /// </summary>
        public void Setup() {
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

        public void OnCollision(Transform hit) {
            if (Time.time > _next)
                _next = Time.time + rate;
            else
                return;

            Entity other = hit.GetComponent<Entity>();

            onCollision.Invoke();
            print("I, " + this.name + ", hit a thing");
        }

    }
}