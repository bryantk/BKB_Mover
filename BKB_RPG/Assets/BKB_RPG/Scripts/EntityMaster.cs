using UnityEngine;
using System.Collections.Generic;

namespace BKB_RPG {
    public class EntityMaster : MonoBehaviour {

        public static EntityMaster _instance { get; private set; }
        public enum dirs { Free, Four, Eight };
        public dirs directions = dirs.Free;
        public float unitDistance = 1f;

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
            }
        }

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            foreach (Entity entity in entities)
            {
                entity.Tick();
            }
        }
    }
}