using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using BKB_TEXT;

namespace BKB_RPG {
    [System.Serializable]
    public class GameEventCommand {

        public enum CommandTypes { Teleport = 1, Pause, UnPause, Wait, Script, Label, GoTo, If, Else, EndIf,
            Shake, Tint, Transition, Debug, Letterbox, Message, Globals, Local, EntityEvent, ClearDialouge };

        public CommandTypes CommandID;
        public bool Block = false;
        // # of lines inspector should reserve
        public int lines = 1;
        public bool expanded = true;

        public int int_1;               // GoTO, shake
        public int int_2;
        public float float_1;           // wait, shake
        public float float_2;           // wait
        public string string_1;         // TP, label, message
        public string string_2;         // Global key
        public Entity entity;
        public Transform transform_1;
        public Vector3 vector3_1;       // TP, shake
        public Vector3 vector3_2;       // shake
        public Vector2 vector2_1;
        public Color color;
        public Gradient gradient;
        public Texture2D texture;
        public bool bool_1;            // TP, mesasge
        public Sprite sprite;
        public AnimatorOverrideController animationOverride;
        [SerializeField]
        public VoxData voxData;

        public bool bool_2;

        // SCRIPT
        public UnityEvent scriptCalls;

        public int executionType = 0;

        private bool _awaitingCallback;

        private void CallbackComplete()
        {
            _awaitingCallback = false;
        }

        public GameEventCommand(CommandTypes type) {
            SetEventCommand(type);
        }

        public void SetEventCommand(CommandTypes type) {
            lines = 1;
            switch (type)
            {
            case CommandTypes.Pause:
                PauseCommand();
                break;
            case CommandTypes.UnPause:
                UnPauseCommand();
                break;
            case CommandTypes.Wait:
                WaitCommand();
                break;
            case CommandTypes.If:
                IfCommand();
                break;
            case CommandTypes.Script:
                ScriptCommand();
                break;
            case CommandTypes.Teleport:
                TeleportCommand();
                break;
            case CommandTypes.Tint:
                TintCommand();
                break;
            case CommandTypes.Transition:
                TransitionCommand();
                break;
            case CommandTypes.Label:
                LabelCommand();
                break;
            case CommandTypes.Debug:
                LabelCommand();
                bool_1 = true;
                CommandID = CommandTypes.Debug;
                break;
            case CommandTypes.GoTo:
                GoToCommand();
                break;
            case CommandTypes.Shake:
                ShakeCommand();
                break;
            case CommandTypes.Letterbox:
                LetterboxCommand();
                break;
            case CommandTypes.Message:
                MessageCommand();
                break;
            case CommandTypes.Globals:
                GlobalCommand(true);
                break;
            case CommandTypes.Local:
                GlobalCommand(false);
                break;
            case CommandTypes.EntityEvent:
                EntityEventCommand();
                break;
            default:
                TypeCommand(type);
                break;
            }
        }

        public GameEventCommand Copy() {
            return (GameEventCommand)this.MemberwiseClone();
        }

        // ----------------------------------------------------------------------
        void TeleportCommand() {
            lines = 3;
            CommandID = CommandTypes.Teleport;
            Block = true;
            executionType = 0;
            string_1 = "";
            vector3_1 = Vector3.zero;
            bool_1 = false;
        }

        void TintCommand() {
            CommandID = CommandTypes.Tint;
            Block = true;
            lines = 3;
            float_1 = 1;    //time
            executionType = 0;      //type: toColor, gradient
            color = Color.black;
            gradient = new Gradient();
        }

        void TransitionCommand() {
            CommandID = CommandTypes.Transition;
            Block = true;
            bool_1 = false;
            lines = 5;
            color = Color.black;
            float_1 = 1;    //time
            executionType = 0;      //type: fade in, fade out
            texture = null;
            int_1 = 0;      //offset_type: none, player, explicit
            vector2_1 = Vector2.zero;
        }

        void IfCommand() {
            CommandID = CommandTypes.If;
            lines = 2;
            string_1 = "";
        }

        void LabelCommand() {
            CommandID = CommandTypes.Label;
            bool_1 = false;
            lines = 2;
            string_1 = "";
        }

        void GoToCommand() {
            CommandID = CommandTypes.GoTo;
            int_1 = 0;
        }

        void PauseCommand() {
            CommandID = CommandTypes.Pause;
            executionType = 0;
        }

        void UnPauseCommand() {
            CommandID = CommandTypes.UnPause;
            executionType = 0;
        }

        void WaitCommand() {
            CommandID = CommandTypes.Wait;
            Block = true;
            float_1 = 1;
        }

        void ScriptCommand() {
            CommandID = CommandTypes.Script;
            lines = 5;
            scriptCalls = new UnityEvent();
        }

        void ShakeCommand() {
            CommandID = CommandTypes.Shake;
            Block = true;
            lines = 5;
            float_1 = 1;
            int_1 = 1;
            vector3_1 = Vector3.one;    //translation
            vector3_1.z = 0;
            vector3_2 = Vector3.zero;    //rotation
        }

        void LetterboxCommand() {
            CommandID = CommandTypes.Letterbox;
            lines = 3;
            float_1 = 1;
            float_2 = 0.213f;
            executionType = 0;
        }

        void MessageCommand() {
            CommandID = CommandTypes.Message;
            lines = 7;
            voxData = new VoxData();
        }

        void GlobalCommand(bool global) {
            CommandID = global ? CommandTypes.Globals : CommandTypes.Local;
            lines = 2;
            executionType = 0; //Bool, float, string
            bool_1 = false;
            int_1 = 0;
            float_1 = 0;
            string_1 = "";
            string_2 = "";
        }

        void EntityEventCommand() {
            CommandID = CommandTypes.EntityEvent;
            lines = 2;
            executionType = 0; //change execution, erase
            entity = null;  // target, null = self
            int_1 = 0;  // page, -1 for current
            int_2 = 0; // executtion trigger type
            string_1 = ""; // JSON
            sprite = null;
            animationOverride = null;
        }

        void TypeCommand(CommandTypes t) {
            CommandID = t;
        }

        // ----------------------------------------------------------------------

        public IEnumerator Run(GameEvent gameEvent)
        {
            gEvent = gameEvent;
            switch (CommandID)
            {
            case CommandTypes.Wait:
                yield return RunWait();
                break;
            case CommandTypes.Script:
                scriptCalls.Invoke();
                break;
            case CommandTypes.Teleport:
                yield return RunTeleport();
                break;
            case CommandTypes.Shake:
                yield return GameMaster._instance.mainCamera.shaker.Shake(float_1, int_1, vector3_1, vector3_2);
                break;
            case CommandTypes.Tint:
                yield return RunTint();
                break;
            case CommandTypes.Transition:
                yield return RunTransition();
                break;
            case CommandTypes.Letterbox:
                GameMaster._instance.mainCamera.tintFader.LetterBox(executionType == 0, float_1, maxCutoff: float_2);
                yield return Wait(float_1);
                break;
            case CommandTypes.Message:
                yield return RunMessage();
                break;
            case CommandTypes.EntityEvent:
                yield return RunEntityEventCommand();
                break;
            case CommandTypes.Debug:
            case CommandTypes.Label:
            case CommandTypes.GoTo:
            default:
                yield break;
            }
            yield break;
        }

        // TO DO - allow transition to be prepared to use
        IEnumerator RunTeleport() {
            float fade = bool_1 ? 0 : 0.25f;
            string_1 = string_1 != "" ? string_1 : null;
            if (executionType == 0)
                GameMaster.Teleport(string_1, CallbackComplete, fade);
            else
                GameMaster.Teleport(vector3_1, string_1, CallbackComplete, fade);
            _awaitingCallback = true;
            while (_awaitingCallback)
                yield return null;
        }

        IEnumerator RunEntityEventCommand()
        {
            entity = entity != null ? entity : gEvent.parent;
            var targetPage = entity.eventPages[entity.activePage];
            switch (executionType)
            {
            case 0:     // Change execution type
                var behaviour = (Entity.TriggerBehaviour)int_2;
                if (int_1 == -2)
                {
                    foreach (var ep in entity.eventPages)
                    {
                        ep.trigger = behaviour;
                    }
                    yield return null;
                }
                int eventPage = int_1;
                if (int_1 == -1)
                    eventPage = entity.activePage;
                entity.eventPages[eventPage].trigger = behaviour;
                break;
            case 1:     // Erase
                entity.iDestroy();
                break;
            case 2:     // Set Move Route
                if (entity == gEvent.parent)
                {
                    entity.CachedFacing = -1;
                }
                var targetMover = targetPage.mover == null ? entity.gameObject.AddComponent<Mover>() : targetPage.mover;
                targetMover.iLoad(string_1);
                if (!Block)
                    yield break;
                targetMover.SetCallback(CallbackComplete);
                _awaitingCallback = true;
                while (_awaitingCallback)
                    yield return null;
                break;
            case 3:     // change sprite
                entity.SetupImage(animationOverride, sprite);
                break;
            case 4: // Disable
                entity.SetActive(false);
                break;
            case 5:  // Activate
                entity.SetActive(true);
                break;
            }
            yield return null;
        }

        IEnumerator RunWait() {
            float time = float_1;
            if (executionType == 1)
                time = Random.Range(float_2, float_1);
            yield return Wait(time);
        }

        IEnumerator RunTint() {
            if (executionType == 0)
                GameMaster._instance.mainCamera.tintFader.Tint(color, float_1);
            else
                GameMaster._instance.mainCamera.tintFader.Tint(gradient, float_1);
            yield return Wait(float_1);
        }

        IEnumerator RunTransition() {
            Vector2? vec2 = null;
            if (int_1 == 1)
                vec2 = GameMaster._instance.mainCamera.ScreenLocationVector(GameMaster._instance.playerData.gameObject.transform);
            else if (int_1 == 2)
                vec2 = vector2_1;
            if (executionType == 0)
                GameMaster._instance.mainCamera.tintFader.FadeOut(float_1, bool_1, color, texture, vec2);
            else
                GameMaster._instance.mainCamera.tintFader.FadeIn(float_1, bool_1, texture, vec2);
            yield return Wait(float_1);
        }

        IEnumerator RunMessage() {
            VoxBox.QueueMessage(voxData);
            VoxBox.PlayMessages(CallbackComplete);
            _awaitingCallback = true;
            while (_awaitingCallback)
                yield return null;
        }

        // Run by GameEvent
        public void RunGlobals(string key) {
            Debug.Log(key);
            switch(executionType)
            {
            case 0:
                GameVariables.SetBool(string_2, bool_1);
                break;
            case 1:
                GameVariables.SetFloat(string_2, float_1);
                break;
            case 2:
                GameVariables.SetString(string_2, string_1);
                break;
            case 3:
                GameVariables.SetBool(string_2, GameMaster._instance.stringParser.EvaluateBool(string_1));
                break;
            case 4:
                GameVariables.SetFloat(string_2, GameMaster._instance.stringParser.EvaluateFloat(string_1));
                break;
            case 5:
                Debug.Log(string_1);
                GameVariables.SetString(string_2, GameMaster._instance.stringParser.EvaluateString(string_1));
                break;
            }

        }

        // Run by GameEvent
        public void RunPause() {
            switch (executionType)
            {
            case 0:     // Pause all
                GameMaster.PauseAll();
                break;
            case 1:     // Pause NPC
                GameMaster.PauseNPCs();
                break;
            case 2:     // Pause Player
                GameMaster.PausePlayer();
                break;
            case 3:     // Pause Enemies
                GameMaster.PauseEnemies();
                break;
            }
        }

        // Run by GameEvent
        public void RunUnPause() {
            switch (executionType)
            {
            case 0:     // Pause all
                GameMaster.ResumeAll();
                break;
            case 1:     // Pause NPC
                GameMaster.ResumeNPCs();
                break;
            case 2:     // Pause Player
                GameMaster.ResumePlayer();
                break;
            case 3:     // Pause Enemies
                GameMaster.ResumeEnemies();
                break;
            }
        }

        private GameEvent gEvent;

        IEnumerator Wait(float seconds)
        {
            float lastTick = 0;
            while (true)
            {
                if (gEvent.parent.Paused)
                {
                    lastTick = Time.time;
                    yield return null;
                }
                float now = Time.time;
                if (lastTick != 0)
                    seconds -= now - lastTick;
                if (seconds <= 0)
                    yield break;
                lastTick = now;
                yield return null;
            }
        }






    }
}