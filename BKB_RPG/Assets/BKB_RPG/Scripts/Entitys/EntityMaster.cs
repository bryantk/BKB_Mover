﻿using UnityEngine;
using System.Collections.Generic;

namespace BKB_RPG {
    public class EntityMaster : MonoBehaviour, ITick {


        public Entity playerEntity;
        public List<Entity> entities = new List<Entity>();


        public void OnSceneReady() {
            foreach (Entity entity in entities)
            {
                entity.Setup();
            }
        }


        public void iTick() {
            foreach (Entity entity in entities)
            {
                entity.iTick();
            }
        }



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