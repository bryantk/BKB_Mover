using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace BKB_RPG {
    public class Entity : MonoBehaviour, IPauseable, ITick, ISetup {

        public delegate void NullDelegate();
        NullDelegate tickDelegate;

        // TODO - Each and Each with seperate lists?
        public enum TriggerBehaviour { OnButtonPress, PlayerTouch, EventTouch, Always, Once, Each, None};

        /// <summary>
        /// 
        /// </summary>
        [System.Serializable]
        public class EntityPageData {
            public string condition;
            public TriggerBehaviour trigger;
            public float rate = 0.25f;
            public GameEvent gameEvent;
            public Mover mover;

            public EntityPageData(GameEvent ge=null, Mover m=null) {
                condition = "";
                trigger = TriggerBehaviour.OnButtonPress;
                rate = 0.25f;
                gameEvent = ge;
                mover = m;
            }

            public bool IsValidCondition() {
                if (string.IsNullOrEmpty(condition))
                    return true;
                return GameMaster._instance.stringParser.EvaluateBool(condition);
            }

        }

        public List<EntityPageData> eventPages;
        public int activePage = 0;
        protected float _next = 0;

        // TODO - Am I running an event? Don't run more if I am.
        public bool Processing = false;

        // readonly
        public bool Paused;

        [HideInInspector]
        public Enemy bkb_enemy;

        Counter frame60Counter;


        /// <summary>
        /// Poll self for all atached scripts 'entity' manages
        /// </summary>
        public virtual void iSetup(object parent) {
            bkb_enemy = GetComponent<Enemy>();
            frame60Counter = new Counter(60);
            activePage = DetermineEventPage();
        }

        public void iTick() {
            if (frame60Counter.Tick())
                activePage = DetermineEventPage();
            if (activePage >= eventPages.Count)
                return;
            var activeEvent = eventPages[activePage];
            if (!Paused && activeEvent.mover != null)
                activeEvent.mover.iTick();
            if (!Paused && (activeEvent.trigger == TriggerBehaviour.Always || activeEvent.trigger == TriggerBehaviour.Once))
            {
                RunEvent();
                if (activeEvent.trigger == TriggerBehaviour.Once)
                    activeEvent.trigger = TriggerBehaviour.None;
            }
        }

        #region Pause + Resume
        public void iPause() {
            Paused = true;
        }

        public void iResume() {
            Paused = false;
        }
        #endregion

        protected int DetermineEventPage() {
            int page = eventPages.Count;
            for (int i = 0; i < eventPages.Count; i++)
            {
                if (eventPages[i].IsValidCondition())
                {
                    if (i == page)
                        return page;
                    page = i;
                    break;
                }
            }
            if (page < eventPages.Count)
                eventPages[page].mover.iSetup(this);
            return page;
        }

        protected void RunEvent() {
            if (activePage < eventPages.Count && eventPages[activePage].gameEvent != null)
                eventPages[activePage].gameEvent.Run();
        }

        public virtual void OnCollision(Transform hit) {
            if (Paused)
                return;
            if (!(eventPages[activePage].trigger == TriggerBehaviour.EventTouch || eventPages[activePage].trigger == TriggerBehaviour.Each))
                return;
            if (Time.time < _next)
                return;
            _next = Time.time + eventPages[activePage].rate;
            //Entity other = hit.GetComponent<Entity>();

            RunEvent();
            // TODO - Always do this?
            if (bkb_enemy != null && hit.GetComponent<Player>() != null)
                bkb_enemy.Battle(false);
            print("I, " + this.name + ", hit a thing");
        }

        public void OnPlayerTouched() {
            if (Paused)
                return;
            if (!(eventPages[activePage].trigger == TriggerBehaviour.PlayerTouch || eventPages[activePage].trigger == TriggerBehaviour.Each))
                return;
            RunEvent();
            // TODO - Always do this?
            if (bkb_enemy != null)
                bkb_enemy.Battle(true);
            print("Player hit me, " + this.name);
        }

        public void OnInputActivated() {
            if (Paused)
                return;
            if (!(eventPages[activePage].trigger == TriggerBehaviour.OnButtonPress || eventPages[activePage].trigger == TriggerBehaviour.Each))
                return;
            RunEvent();
            print("Player activated me, " + this.name);
        }

        // TODO - add page index
        // TODO - Determine need and functionality
        public void SetTriggerbehaviour(string type="NONE") {
            switch (type.ToUpper())
            {
            default:
            case "NONE":
                eventPages[activePage].trigger = TriggerBehaviour.None;
                break;
            case "EVENTTOUCH":
                eventPages[activePage].trigger = TriggerBehaviour.EventTouch;
                break;
            case "PLAYERTOUCH":
                eventPages[activePage].trigger = TriggerBehaviour.PlayerTouch;
                break;
            case "ALWAYS":
                eventPages[activePage].trigger = TriggerBehaviour.Always;
                break;
            case "ONBUTTONPRESS":
                eventPages[activePage].trigger = TriggerBehaviour.OnButtonPress;
                break;
            case "ONCE":
                eventPages[activePage].trigger = TriggerBehaviour.Once;
                break;
            case "EACH":
                eventPages[activePage].trigger = TriggerBehaviour.Each;
                break;
            }
        }
    }
}