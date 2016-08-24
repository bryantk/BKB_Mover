using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace BKB_RPG {
    [System.Serializable]
    public class GameEventCommand {

        public enum CommandTypes { Teleport = 1, Pause, UnPause, Wait, Script, Label, GoTo, If, Else, EndIf, Shake, Tint, Transition, Debug, Letterbox };

        public CommandTypes CommandID;
        public bool Block = false;
        // # of lines inspector should reserve
        public int lines = 1;
        public bool expanded = true;

        public int int_1;               // GoTO, shake
        public float float_1;           // wait, shake
        public float float_2;           // wait
        public string string_1;         // TP, label
        public Transform transform_1;
        public Vector3 vector3_1;       // TP, shake
        public Vector3 vector3_2;       // shake
        public Vector2 vector2_1;
        public Color color;
        public Gradient gradient;
        public Texture2D texture;
        public bool bool_1;            // TP

        // SCRIPT
        public UnityEvent scriptCalls;

        public int executionType = 0;


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

        void TypeCommand(CommandTypes t) {
            CommandID = t;
        }

        // ----------------------------------------------------------------------

        public IEnumerator Execute(GameEvent geo = null) {
            if (Block)
                yield return Run();
            else
            {
                if (geo != null)
                    geo.StartCoroutine(Run());
                else
                    GameMaster._instance.StartCoroutine(Run());
            }
        }

        public IEnumerator Run() {
            switch (CommandID)
            {
            case CommandTypes.Pause:
                yield return RunPause();
                break;
            case CommandTypes.UnPause:
                yield return RunUnPause();
                break;
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
                yield return GameMaster._instance.mainCamera.tintFader.LetterBox(executionType == 0, float_1, maxCutoff: float_2);
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
            bool waiting = true;
            float fade = bool_1 ? 0 : 0.25f;
            string_1 = string_1 != "" ? string_1 : null;
            if (executionType == 0)
                GameMaster.Teleport(string_1, () => { waiting = false; }, fade);
            else
                GameMaster.Teleport(vector3_1, string_1, () => { waiting = false; }, fade);
            while (waiting)
                yield return null;
        }

        IEnumerator RunWait() {
            float time = float_1;
            if (executionType == 1)
                time = Random.Range(float_2, float_1);
            yield return new WaitForSeconds(time);
        }

        IEnumerator RunTint() {
            if (executionType == 0)
                return GameMaster._instance.mainCamera.tintFader.Tint(color, float_1);
            else
                return GameMaster._instance.mainCamera.tintFader.Tint(gradient, float_1);
        }

        IEnumerator RunTransition() {
            Vector2? vec2 = null;
            if (int_1 == 1)
                vec2 = GameMaster._instance.mainCamera.ScreenLocationVector(GameMaster._instance.playerData.gameObject.transform);
            else if (int_1 == 2)
                vec2 = vector2_1;
            if (executionType == 0)
                return GameMaster._instance.mainCamera.tintFader.FadeOut(float_1, bool_1, color, texture, vec2);
            else
                return GameMaster._instance.mainCamera.tintFader.FadeIn(float_1, bool_1, texture, vec2);
        }

        IEnumerator RunPause() {
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
            yield break;
        }

        IEnumerator RunUnPause() {
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
            yield break;
        }


    }
}