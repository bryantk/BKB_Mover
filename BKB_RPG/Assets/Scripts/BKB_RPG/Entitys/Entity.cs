using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace BKB_RPG {
    public class Entity : MonoBehaviour, IPauseable, ITick, ISetup, IDestroy
    {
        [System.Serializable]
        public class EntityPageData {
            public string condition;
            public TriggerBehaviour trigger;
            public GameEvent gameEvent;
            public Mover mover;
            public bool useCollider;
            public int facing;
            public AnimatorOverrideController controller;
            public Sprite sprite;
            public string savedMoverJSON;

            public EntityPageData(GameEvent ge=null, Mover m=null) {
                condition = "";
                trigger = TriggerBehaviour.OnButtonPress;
                gameEvent = ge;
                mover = m;
                useCollider = true;
                facing = 180;
            }

            public bool IsValidCondition() {
                if (string.IsNullOrEmpty(condition))
                    return true;
                return GameMaster._instance.stringParser.EvaluateBool(condition, gameEvent == null ? null : gameEvent.gameObject);
            }

        }

        public float CachedFacing;


        private const float RATE = 0.25f;

        // TODO - Each and Each with seperate lists?
        public enum TriggerBehaviour { OnButtonPress, PlayerTouch, EventTouch, Always, Once, Each, None };

        public List<EntityPageData> eventPages;
        public int activePage = -1;
        protected float _next = 0;

        // TODO - Am I running an event? Don't run more if I am.
        public bool Processing = false;

        // readonly
        protected bool _paused;

        public bool Paused
        {
            get { return _paused;}
            set
            {
                if (value)
                    iResume();
                else
                {
                    iPause();
                }
            }
        }

        public int myGUID = 0;

        [HideInInspector]
        public Enemy bkb_enemy;

        private TickCounter frameCounter;
        protected Collider2D myCollider;
        public Animator _animator;
        public SpriteRenderer _sprite;

        void OnEnable()
        {
            iResume();
        }

        void OnDisable()
        {
            iPause();
        }

        public void ShouldEvaluateConditions(bool active) {
            if (active)
                frameCounter.Resume();
            else
                frameCounter.Pause();
        }

        /// <summary>
        /// Poll self for all atached scripts 'entity' manages
        /// </summary>
        public virtual void iSetup(object parent)
        {
            activePage = -1;
            _animator = GetComponent<Animator>();
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
            _sprite = GetComponent<SpriteRenderer>();
            if (_sprite == null)
                _sprite = GetComponentInChildren<SpriteRenderer>();
            var myuGUID = GetComponent<uGUID>();
            myGUID = myuGUID != null ? myuGUID.persistentID : 0;
            bkb_enemy = GetComponent<Enemy>();
            frameCounter = new TickCounter(30);
            myCollider = GetComponent<Collider2D>();
            for (int i = 0; i < eventPages.Count; i++)
            {
                var page = eventPages[i];
                if (page.gameEvent != null)
                    page.gameEvent.iSetup(this);
                if (page.mover != null)
                    page.savedMoverJSON = page.mover.Save("savePosition");
            }
            activePage = DetermineEventPage();
        }

        public virtual void iTick() {
            if (frameCounter.Tick)
                activePage = DetermineEventPage();
            if (!gameObject.activeSelf || _paused)
                return;
            var activeEvent = eventPages[activePage];
            if (activeEvent.mover != null)
                activeEvent.mover.iTick();
            if (activeEvent.trigger == TriggerBehaviour.Always || activeEvent.trigger == TriggerBehaviour.Once)
            {
                RunEvent();
                if (activeEvent.trigger == TriggerBehaviour.Once)
                    activeEvent.trigger = TriggerBehaviour.None;
            }
        }

        private bool HasActivePage
        {
            get { return activePage < eventPages.Count && Application.isPlaying; }
        }

        public void iDestroy()
        {
            EntityMaster.DisableEntity(this, true);
        }

        public void SetActive(bool active)
        {
            if (active)
            {
                EntityMaster.AddEntity(this);
            }
            else
            {
                EntityMaster.DisableEntity(this);
            }
            gameObject.SetActive(active);
        }

        #region Pause + Resume
        public void iPause() {
            if (_paused)
                return;
            _paused = true;
            if (HasActivePage)
            {
                var activeEvent = eventPages[activePage];
                if (activeEvent.mover != null)
                    activeEvent.mover.iPause();
            }
        }

        public void iResume() {
            if (!_paused)
                return;
            _paused = false;
            if (HasActivePage)
            {
                var activeEvent = eventPages[activePage];
                if (activeEvent.mover != null)
                    activeEvent.mover.iResume();
            }
        }
        #endregion

        protected int DetermineEventPage() {
            int numPages = eventPages.Count;
            for (int i = 0; i < numPages; i++)
            {
                if (eventPages[i].IsValidCondition())
                {
                    gameObject.SetActive(true);
                    frameCounter.ResetInterval(60);
                    SetupPage(i);
                    return i;
                }
            }
            // No valid event found
            gameObject.SetActive(false);
            frameCounter.ResetInterval(30);
            return numPages;
        }

        protected void SetupPage(int page)
        {
            if (activePage == page)
                return;
            // New page selected. Zero out old page, set up new.
            var previousFoces = Vector2.zero;
            var previousPage = activePage == -1 ? null : eventPages[activePage];
            if (previousPage != null)
            {
                if (previousPage.mover != null)
                {
                    previousFoces = previousPage.mover.constantForces;
                    previousPage.mover.Stop();
                }
            }
            var selectedPage = eventPages[page];
            SetupImage(selectedPage.controller, selectedPage.sprite);
            if (myCollider != null)
            {
                myCollider.isTrigger = !selectedPage.useCollider;
            }
            // set up new mover if exists
            if (selectedPage.mover != null)
            {
                selectedPage.mover.iLoad(selectedPage.savedMoverJSON);
                selectedPage.mover.iSetup(this);
                selectedPage.mover.SetFacing(selectedPage.facing);
                selectedPage.mover.constantForces = previousFoces;
            }
        }

        public void SetupImage(AnimatorOverrideController animController = null, Sprite sprite = null)
        {
            print("Setting up " + name);
            if (animController != null && _animator != null)
            {
                _animator.enabled = true;
                _animator.runtimeAnimatorController = animController;
            }
            else if (sprite != null)
            {
                if (_animator != null)
                    _animator.enabled = false;
                // TODO - if no sprite, add component?
                _sprite.enabled = true;
                _sprite.sprite = sprite;
            }
            else
            {
                if (_sprite != null)
                    _sprite.enabled = false;
            }
        }

        protected void RunEvent(Callback onComplete = null) {
            if (activePage < eventPages.Count && eventPages[activePage].gameEvent != null)
                eventPages[activePage].gameEvent.RunWithCallback(onComplete);
        }



        // Reactive Event Triggering

        public virtual void OnCollision(Transform hit) {
            if (_paused)
                return;
            if (!(eventPages[activePage].trigger == TriggerBehaviour.EventTouch || eventPages[activePage].trigger == TriggerBehaviour.Each))
                return;
            if (Time.time < _next)
                return;
            _next = Time.time + RATE;
            //Entity other = hit.GetComponent<Entity>();

            RunEvent();
            // TODO - Always do this?
            if (bkb_enemy != null && hit.GetComponent<Player>() != null)
                bkb_enemy.Battle(false);
            print("I, " + this.name + ", hit a thing");
        }

        public void OnPlayerTouched() {
            if (_paused)
                return;
            if (!(eventPages[activePage].trigger == TriggerBehaviour.PlayerTouch || eventPages[activePage].trigger == TriggerBehaviour.Each))
                return;
            RunEvent();
            // TODO - Always do this?
            if (bkb_enemy != null)
                bkb_enemy.Battle(true);
            print("Player hit me, " + this.name);
        }

        public void OnInputActivated(Entity activator) {
            if (_paused)
                return;
            if (!(eventPages[activePage].trigger == TriggerBehaviour.OnButtonPress || eventPages[activePage].trigger == TriggerBehaviour.Each))
                return;
            Mover m = MyMover;
            CachedFacing = m != null ? m.facing : -1;
            if (m)
            {
                Vector2 dir = activator.gameObject.transform.position - gameObject.transform.position;
                m.SetFacing(Utils.Vector2toAngle(dir));
            }
            RunEvent(ResetFacing);
            print("Player activated me, " + this.name);
        }

        private void ResetFacing()
        {
            Mover m = MyMover;
            if (CachedFacing < 0 || m == null)
                return;
            m.SetFacing(CachedFacing);
        }

        private Mover MyMover
        {
            get
            {
                if (HasActivePage && eventPages[activePage].mover != null)
                    return eventPages[activePage].mover;
                return null;
            }
        }

        private GameEvent MyEvent
        {
            get
            {
                if (HasActivePage && eventPages[activePage].gameEvent != null)
                    return eventPages[activePage].gameEvent;
                return null;
            }
        }


    }
}