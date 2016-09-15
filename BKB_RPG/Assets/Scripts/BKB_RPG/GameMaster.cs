using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public delegate void Callback();

namespace BKB_RPG {
    public class GameMaster : MonoBehaviour, ISetup {

        public static GameMaster _instance;
        public EntityMaster entityMaster;

        [System.Serializable]
        public struct LevelData {
            public string name;
            public LevelMaster currentLevel;
        }
        static public LevelData levelData;

        [System.Serializable]
        public struct PlayerStruct {
            public GameObject gameObject;
            public Player playerEntity;
            public Mover mover;
        }
        public PlayerStruct playerData;

        public CameraMaster mainCamera;

        public StringParser stringParser;

        // callbacks
        static Callback TPCallback;


#if UNITY_EDITOR
        void OnGUI() {
            GUI.Label(new Rect(10, Screen.height - 20, 100, 20), Time.time.ToString());
        }
#endif

        public virtual void Awake() {
            if (_instance == null)
            {
                _instance = this;
                
                DontDestroyOnLoad(this);
                OnAwake();
            }
            else {
                Debug.LogWarning("MASTER already exists, deleting.");
                Destroy(gameObject);
                return;
            }
        }

        // Update is called once per frame
        void Update() {
            entityMaster.iTick();
        }


        void OnDestroy() {
            if (_instance == this) _instance = null;
        }


        void OnAwake() {
            iSetup();
            // TODO - TEMP
            BKB_FSM.StateManager.iSetup(null);
        }

        public void iSetup(object parent=null) {
            stringParser = GetComponent<StringParser>();
            stringParser.iSetup(this);
            GameVariables.iSetup(null);
            if (entityMaster == null)
                entityMaster = GetComponent<EntityMaster>();
            entityMaster.playerEntity = playerData.playerEntity;
            levelData.name = "main";
            levelData.currentLevel = FindObjectOfType<LevelMaster>();
            if (levelData.currentLevel != null)
                levelData.currentLevel.SetupLevel();
            

            playerData.playerEntity.iSetup(this);
            mainCamera.ReParent(playerData.gameObject.transform);
            // TODO - load from save
            mainCamera.tintFader.FadeIn(0);
            mainCamera.tintFader.Tint(Color.clear, 0);
            mainCamera.tintFader.LetterBox(false);
            // TODO - TESTing
            GameVariables.SetBool("setup", true);
            GameVariables.SetFloat("playerHP", 9001);
            //print(GameVariables.iSave());
        }

        static public Coroutine RunCoroutine(IEnumerator co) {
            return GameMaster._instance.StartCoroutine(co);
        }

        static public Transform GetPlayerTransform(int position = 0) {
            // TODO - offset position for other party memebers
            return _instance.playerData.gameObject.transform;
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        // Commands
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        static void SetPartyPosition(Vector3 position) {
            // Sets party position on current map
            _instance.playerData.gameObject.transform.position = position;
        }


        public static void Teleport(string levelLabel, Callback callback = null, float time = 0.25f) {
            TPCallback = callback;
            string[] strArray = levelLabel.Split('.');
            if (strArray.Length == 1)
                Teleport(strArray[0], null, callback, time);
            else if (strArray.Length == 2)
                Teleport(strArray[1], strArray[0], callback, time);
            else
                Debug.LogWarning(string.Format("Level Label '{}' malformed.", levelLabel));
        }

        public static void Teleport(string label, string scene, Callback callback=null, float time = 0.25f) {
            TPCallback = callback;
            _instance.StartCoroutine(_Teleport(Vector3.zero, scene, label, time, time));
        }

        public static void Teleport(Vector3 position, string scene = null, Callback callback = null, float time = 0.25f) {
            TPCallback = callback;
            _instance.StartCoroutine(_Teleport(position, scene, null, time, time));
        }

        // Pause + Resume
        public static void PauseAll() {
            _instance.entityMaster.PauseAll();
        }

        public static void PausePlayer() {
            _instance.entityMaster.PausePlayer();
        }

        public static void PauseNPCs() {
            _instance.entityMaster.PauseNPC();
        }

        public static void PauseEnemies() {
            _instance.entityMaster.PauseEnemies();
        }

        public static void ResumeAll() {
            _instance.entityMaster.ResumeAll();
        }

        public static void ResumePlayer() {
            _instance.entityMaster.ResumePlayer();
        }

        public static void ResumeNPCs() {
            _instance.entityMaster.ResumeNPC();
        }

        public static void ResumeEnemies() {
            _instance.entityMaster.ResumeEnemies();
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        // Helpers
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        static IEnumerator LoadLevel(string levelNameToLoad) {
            _instance.mainCamera.ReParent(_instance.playerData.gameObject.transform);
            var loading = SceneManager.LoadSceneAsync(levelNameToLoad);
            // Set FSM = Loading
            yield return loading;
            SceneManager.UnloadScene(levelData.name);
            levelData.name = levelNameToLoad;
            levelData.currentLevel = FindObjectOfType<LevelMaster>();
            levelData.currentLevel.SetupLevel();
        }

        static IEnumerator _Teleport(Vector3 position, string levelNameToLoad, string label, float timeOut = 0.25f, float timeIn = 0.25f) {
            BKB_FSM.StateManager.Push("TP");
            if (timeOut > 0)
                yield return _instance.mainCamera.tintFader.FadeOut(timeOut);

            if (levelNameToLoad != null && levelNameToLoad != levelData.name)
                yield return LoadLevel(levelNameToLoad);
            if (label != null)
                position = levelData.currentLevel.GetLabel(label);
            SetPartyPosition(position);

            if (timeIn > 0)
                yield return _instance.mainCamera.tintFader.FadeIn(timeIn);
            BKB_FSM.StateManager.Pop();
            if (TPCallback != null)
                TPCallback();
        }
 

    }
}