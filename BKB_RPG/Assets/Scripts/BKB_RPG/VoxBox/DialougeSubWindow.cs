using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using BKB_RPG;

namespace BKB_TEXT {
    public class DialougeSubWindow : UIWindow
    {
        public Button button;


        public void Highlight() {
            button.image.color = VoxBox._this.HighlightColor;
        }

        public void UnHighlight() {
            button.image.color = VoxBox._this.NormalColor;
        }

        public void Disable() {
            // TODO - Should disable button
            button.image.color = VoxBox._this.DisabledColor;
        }

        // TODO - add 'enable' method
    }
}