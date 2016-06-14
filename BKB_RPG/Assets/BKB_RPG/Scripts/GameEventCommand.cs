﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace BKB_RPG {
    [System.Serializable]
    public class GameEventCommand {

        public enum CommandTypes { Teleport=1, Pause, UnPause, Wait, Script, Label, GoTo, If, Else, EndIf, Shake };

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
        public bool instant;            // TP

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
            case CommandTypes.Label:
                LabelCommand();
                break;
            case CommandTypes.GoTo:
                GoToCommand();
                break;
            case CommandTypes.Shake:
                ShakeCommand();
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
        public void TeleportCommand() {
            lines = 3;
            CommandID = CommandTypes.Teleport;
            Block = true;
            executionType = 0;
            string_1 = "";
            vector3_1 = Vector3.zero;
            instant = false;
        }

        public void TintCommand() {

        }

        public void TransitionCommand() {

        }

        public void LabelCommand() {
            CommandID = CommandTypes.Label;
            lines = 2;
            string_1 = "";
        }

        public void GoToCommand() {
            CommandID = CommandTypes.GoTo;
            int_1 = 0;
        }

        public void PauseCommand() {
            CommandID = CommandTypes.Pause;
            executionType = 0;
        }

        public void UnPauseCommand() {
            CommandID = CommandTypes.UnPause;
            executionType = 0;
        }

        public void WaitCommand() {
            CommandID = CommandTypes.Wait;
            Block = true;
            float_1 = 1;
        }

        public void ScriptCommand() {
            CommandID = CommandTypes.Script;
            lines = 5;
            scriptCalls = new UnityEvent();
        }

        public void ShakeCommand() {
            CommandID = CommandTypes.Shake;
            Block = true;
            lines = 5;
            float_1 = 1;
            int_1 = 1;
            vector3_1 = Vector3.one;    //translation
            vector3_1.z = 0;
            vector3_2 = Vector3.zero;    //rotation
        }

        public void TypeCommand(CommandTypes t) {
            CommandID = t;
        }

        // ----------------------------------------------------------------------

        public IEnumerator Execute() {
            if (Block)
                yield return Run();
            else
                GameMaster._instance.StartCoroutine(Run());
        }

        public IEnumerator Run() {
            switch (CommandID)
            {
            case CommandTypes.Pause:
                yield return RunPauase();
                break;
            case CommandTypes.UnPause:
                yield return RunUnPauase();
                break;
            case CommandTypes.Wait:
                float time = float_1;
                if (executionType == 1)
                    time = Random.Range(float_2, float_1);
                yield return new WaitForSeconds(time);
                break;
            case CommandTypes.Script:
                scriptCalls.Invoke();
                break;
            case CommandTypes.Teleport:
                yield return RunTeleport();
                break;
            case CommandTypes.Shake:
                yield return RunShakeCommand();
                break;
            case CommandTypes.Label:
            case CommandTypes.GoTo:
            default:
                yield break;
            }
            yield break;
        }

        private IEnumerator RunTeleport() {
            bool waiting = true;
            float fade = instant ? 0 : 0.25f;
            string_1 = string_1 != "" ? string_1 : null;
            if (executionType == 0)
                GameMaster.Teleport(string_1, () => { waiting = false; }, fade);
            else
                GameMaster.Teleport(vector3_1, string_1, () => { waiting = false; }, fade);
            while (waiting)
                yield return null;
        }

        private IEnumerator RunPauase() {
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

        private IEnumerator RunUnPauase() {
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

        private IEnumerator RunShakeCommand() {
            GameMaster.SetRotationScale(vector3_2);
            GameMaster.Shake(int_1, float_1, vector3_1);
            if (Block)
                yield return new WaitForSeconds(float_1);
            yield break;
        }

    }
}