using UnityEngine;
using System.Collections;

namespace BKB_RPG {
    [RequireComponent(typeof(Entity))]
    public class Enemy : MonoBehaviour {


        bool Initiative(float facing) {
            if (facing > 180)
                facing -= 180;
            // 180 = Facing
            // 90 = side
            // 0 = Back
            if (facing <= 45)
                return true; // back attack
            return false;

        }

        public void Battle(bool playerInstigated=false) {
            EntityMaster._instance.PauseAll();
            float playerFacing = Player._instance.GetComponent<Mover>().facing;
            float myFacing = this.GetComponent<Mover>().facing;
            if (playerInstigated)
            {
                float temp = myFacing;
                myFacing = playerFacing;
                playerFacing = temp;
            }
            float diff = Utils.TouchedAt(myFacing, playerFacing);
            if (Initiative(diff))
            {
                if (playerInstigated)
                    print("Player's Favor");
                else
                    print("Enemy's Favor");
            }
            // TODO - Call battle
            EntityMaster._instance.ResumeAll();
        }
    }
}
