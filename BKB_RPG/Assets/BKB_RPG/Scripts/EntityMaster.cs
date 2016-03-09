using UnityEngine;
using System.Collections;

namespace BKB_RPG {
    public class EntityMaster : MonoBehaviour {

        public static EntityMaster _instance { get; private set; }
        public enum dirs { Free, Four, Eight };
        public dirs directions = dirs.Free;
        public float unitDistance = 1f;

        private Entity playerEntity;

        private Entity[] entities;

        public virtual void Awake() {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
                OnAwake();
            }
            else {
                Debug.LogWarning("EntityMaster already exists, deleting.");
                Destroy(this.gameObject);
                return;
            }
        }

        void OnDestroy() {
            if (_instance == this) _instance = null;
        }

        void OnAwake() {
            OnSceneLoad();
        }

        public void OnSceneLoad() {
            entities = FindObjectsOfType<Entity>();
            foreach (Entity entity in entities)
            {
                entity.Setup();
            }
            playerEntity = FindObjectOfType<Player>();
        }



        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            foreach (Entity entity in entities)
            {
                entity.iTick();
            }
        }



        #region Pause + Resume
        public void PauseAll() {
            PauseNPC();
            PausePlayer();
        }

        public void PauseNPC() {
            foreach (Entity entity in entities)
            {
                if (entity == playerEntity)
                    continue;
                entity.iPause();
            }
        }

        public void PausePlayer() {
            playerEntity.iPause();
        }


        public void ResumeAll() {
            ResumeNPC();
            ResumePlayer();
        }

        public void ResumeNPC() {
            foreach (Entity entity in entities)
            {
                if (entity == playerEntity)
                    continue;
                entity.iResume();
            }
        }

        public void ResumePlayer() {
            playerEntity.iResume();
        }
        #endregion
    }
}