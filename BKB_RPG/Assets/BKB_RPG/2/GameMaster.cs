using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public delegate void Callback();

namespace BKB_RPG {
    public class GameMaster : MonoBehaviour {

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

        // callbacks
        static Callback TPCallback;

        static bool isBlocking = false;


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
            playerData.playerEntity.Setup();
            if (entityMaster == null)
                entityMaster = GetComponent<EntityMaster>();
            entityMaster.playerEntity = playerData.playerEntity;
            levelData.name = "main";
            levelData.currentLevel = FindObjectOfType<LevelMaster>();
            if (levelData.currentLevel != null)
                levelData.currentLevel.SetupLevel();
            playerData.playerEntity.Setup();
            mainCamera.ReParent(playerData.gameObject.transform);
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


        public static void Teleport(string levelLabel, float time = 0.25f, Callback callback = null) {
            TPCallback = callback;
            string[] strArray = levelLabel.Split('.');
            if (strArray.Length == 1)
                Teleport(strArray[0], null, time, callback);
            else if (strArray.Length == 2)
                Teleport(strArray[1], strArray[0], time, callback);
            else
                Debug.LogWarning(string.Format("Level Label '{}' malformed.", levelLabel));
        }

        public static void Teleport(string label, string scene, float time = 0.25f, Callback callback=null) {
            TPCallback = callback;
            _instance.StartCoroutine(_Teleport(Vector3.zero, scene, label, time, time));
        }

        public static void Teleport(Vector3 position, string scene = null, float time = 0.25f, Callback callback = null) {
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

        // Reparent Camera
        public static void ReParentCamera(Transform target) {
            _instance.mainCamera.ReParent(target);
        }

        // Shaker
        public static void SetRotationScale(Vector3 rotation_Scale) {
            _instance.mainCamera.SetRotationScale(rotation_Scale);
        }

        public static void SetShakeScale(Vector3 scale) {
            _instance.mainCamera.SetShakeScale(scale);
        }

        public static void Shake(int power, float duration, Callback callback=null) {
            _instance.mainCamera.Shake(power, duration, callback);
        }

        public static void Shake(int power, float duration, Vector3 scale, Callback callback=null) {
            _instance.mainCamera.SetShakeScale(scale);
            Shake(power, duration, callback);
        }

        public static void Shake(int power, float duration, Vector3 scale, Vector3 rotation_Scale, Callback callback=null) {
            _instance.mainCamera.SetShakeScale(scale);
            _instance.mainCamera.SetRotationScale(rotation_Scale);
            Shake(power, duration, callback);
        }

        // Tint
        public static void Tint(Color toColor, float time = 0, Callback callback=null) {
            _instance.mainCamera.tintFader.Tint(toColor, time, callback);
        }

        public static void Tint(Color fromColor, Color toColor, float time = 0, Callback callback=null) {
            _instance.mainCamera.tintFader.Tint(fromColor, toColor, time, callback);
        }

        public static void TintFromCurrent(Color toColor, float time = 0, Callback callback=null) {
            _instance.mainCamera.tintFader.TintFromCurrent(toColor, time, callback);
        }

        public static void FadeOut(float time, Callback callback=null) {
            Tint(Color.black, time, callback);
        }

        public static void FadeIn(float time, Callback callback = null) {
            TintFromCurrent(Color.clear, time, callback);
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        // Helpers
        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - 
        static IEnumerator LoadLevel(string levelNameToLoad) {
            ReParentCamera(_instance.playerData.gameObject.transform);
            var loading = SceneManager.LoadSceneAsync(levelNameToLoad);
            // Set FSM = Loading
            yield return loading;
            SceneManager.UnloadScene(levelData.name);
            levelData.name = levelNameToLoad;
            levelData.currentLevel = FindObjectOfType<LevelMaster>();
            levelData.currentLevel.SetupLevel();
        }

        static IEnumerator _Teleport(Vector3 position, string levelNameToLoad, string label, float timeOut=0.25f, float timeIn=0.25f) {
            PauseAll();
            // Set state 'teleport'

            FadeOut(timeOut, () => { Complete(); } );
            yield return _Block();

            if (levelNameToLoad != null && levelNameToLoad != levelData.name)
                yield return LoadLevel(levelNameToLoad);
            if (label != null)
                position = levelData.currentLevel.GetLabel(label);
            SetPartyPosition(position);

            FadeIn(timeIn, () => { Complete(); });
            yield return _Block();

            ResumeAll();
            if (TPCallback != null)
                TPCallback();
        }
 

        static IEnumerator _Block(bool shouldBlock=true) {
            isBlocking = shouldBlock;
            while (isBlocking)
                yield return null;
            isBlocking = false;
        }

        static void Complete() {
            isBlocking = false;
        }

    }
}