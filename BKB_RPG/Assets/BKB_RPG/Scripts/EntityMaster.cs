using UnityEngine;
using System.Collections;

namespace BKB_RPG {
    public class EntityMaster : MonoBehaviour {

        public static EntityMaster _instance { get; private set; }
        public enum dirs { Free, Four, Eight };
        public dirs directions = dirs.Free;
        public float unitDistance = 1f;

        public GameObject player;
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
                Debug.LogWarning("MoverMaster already exists, deleting.");
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
                if (entity.gameObject.tag == "Player")
                {
                    player = entity.gameObject;
                    playerEntity = entity;
                }
            }
        }



        // Use this for initialization
        void Start() {
            StartCoroutine(ts());
        }

        // Update is called once per frame
        void Update() {
            foreach (Entity entity in entities)
            {
                entity.Tick();
            }
        }


        IEnumerator ts() {
            print("start");
            yield return new WaitForSeconds(2);
            print("pause");
            PauseAll();
            yield return new WaitForSeconds(2);
            print("player");
            ResumePlayer();
            yield return new WaitForSeconds(5);
            print("all");
            ResumeAll();
        }


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

    }
}