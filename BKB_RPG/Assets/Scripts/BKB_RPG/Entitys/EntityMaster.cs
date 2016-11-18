using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace BKB_RPG {
    public class EntityMaster : MonoBehaviour, ISetup, ITick {


        public Player playerEntity;
        public List<Entity> entities = new List<Entity>();

        private static EntityMaster _instance;

        private static Entity coroutineOwner;
        private static Coroutine activeCoroutine;
        private static Dictionary<Entity, Coroutine> parallelCoroutines;

        private void SetupSingleton()
        {
            if (_instance == null)
            {
                _instance = this;
                parallelCoroutines = new Dictionary<Entity, Coroutine>();
            }
            else if (_instance != this)
                Destroy(this);
        }

        public void iSetup(object o)
        {
            SetupSingleton();
            foreach (Entity entity in entities)
            {
                entity.iSetup(this);
            }
            // TODO - need to do this each map????
            playerEntity.iSetup(null);
        }

        public void iTick()
        {
            if (BKB_FSM.StateManager.GetState != BKB_FSM.FSMState.OnMap.ToString())
                return;
            foreach (Entity entity in entities)
            {
                entity.iTick();
            }
        }

        public static void DestoryEntity(Entity entity)
        {
            HaltCoroutine(entity);
            _instance.entities.Remove(entity);
            Destroy(entity.gameObject);
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        //  Coroutines
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        private static void StopActiveCoroutine()
        {
            _instance.StopCoroutine(activeCoroutine);
            activeCoroutine = null;
            coroutineOwner = null;
        }

        public static bool EntityCoroutineRunning(Entity entity, bool Parralel = false)
        {
            return Parralel ? parallelCoroutines.ContainsKey(entity) : coroutineOwner == entity;
        }

        public static Coroutine AddCoroutine(Entity entity, IEnumerator coroutine, bool parallel = false)
        {
            if (parallel)
            {
                if (parallelCoroutines.ContainsKey(entity))
                {
                    Debug.LogWarning(string.Format("2 - Failed to launch {0}'s Parallel Coroutine. Already in progress.", entity.name));
                    return null;
                }
                else
                {
                    Coroutine c = _instance.StartCoroutine(coroutine);
                    parallelCoroutines.Add(entity, c);
                    return c;
                }
            }
            else
            {
                if (activeCoroutine == null)
                {
                    Coroutine c = _instance.StartCoroutine(coroutine);
                    activeCoroutine = c;
                    coroutineOwner = entity;
                    return c;
                }
                else
                {
                    Debug.LogWarning(string.Format("1 - Failed to launch {0}'s Lone Coroutine. Another in progress.", entity.name));
                    return null;
                }
            }
        }

        public static void HaltCoroutine(Coroutine coroutine)
        {
            if (activeCoroutine == coroutine)
            {
                StopActiveCoroutine();
            }
            if (parallelCoroutines.ContainsValue(coroutine))
            {
                foreach(var item in parallelCoroutines.Where(x => x.Value == coroutine).ToList())
                {
                    _instance.StopCoroutine(item.Value);
                    parallelCoroutines.Remove(item.Key);
                }
            }
            else
                Debug.LogWarning("1 - Failed to Remove Coroutine. Not found");
        }

        public static void HaltCoroutine(Entity entity) {
            if (coroutineOwner == entity)
            {
                StopActiveCoroutine();
            }
            else if (parallelCoroutines.ContainsKey(entity))
            {
                foreach (var item in parallelCoroutines.Where(x => x.Key == entity).ToList())
                {
                    _instance.StopCoroutine(item.Value);
                    parallelCoroutines.Remove(item.Key);
                }
            }
            else
                Debug.LogWarning(string.Format("1 - Failed to Remove {0}'s Lone Coroutine. Not found", entity.name));
        }


        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        //  Pause / Resume entites
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        #region Pause + Resume
        public void PauseAll() {
            PauseNPC();
            PauseEnemies();
            PausePlayer();
        }

        public void PauseNPC() {
            foreach (Entity entity in entities)
            {
                if (entity.bkb_enemy == null)
                    entity.iPause();
            }
        }

        public void PauseEnemies() {
            foreach (Entity entity in entities)
            {
                if (entity.bkb_enemy != null)
                    entity.iPause();
            }
        }

        public void PausePlayer() {
            playerEntity.iPause();
        }


        public void ResumeAll() {
            ResumeNPC();
            ResumeEnemies();
            ResumePlayer();
        }

        public void ResumeNPC() {
            foreach (Entity entity in entities)
            {
                if (entity.bkb_enemy == null)
                    entity.iResume();
            }
        }

        public void ResumeEnemies() {
            foreach (Entity entity in entities)
            {
                if (entity.bkb_enemy != null)
                    entity.iResume();
            }
        }

        public void ResumePlayer() {
            playerEntity.iResume();
        }
        #endregion
    }
}