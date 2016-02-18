using UnityEngine;
using System.Collections;

namespace BKB_TEXT {
    public class Dialouge : MonoBehaviour {

        public string speakerName;
        public string text;
        public string displayText;

        public int zDepth;
        public float rate = 0;
        // TODO - Group1: ',' '-' with space on right   light pauase
        //          Group2: '.', '?', '!'               pause
        public float punctuationModifier = 1;
        public float newPageModifier = 1;

        public enum TextLocationEnum{Top, Middle, Bottom, Auto, Location}
        public TextLocationEnum textLocation;
        public Vector2 location;

        private int textLength;
        private int index;
        // Dictionary of text commands and thier start/end location

        // Use this for initialization
        void Start() {
            index = 0;
            // TODO - Remove HTML codes
            textLength = text.Length;
            StartCoroutine(Output());
        }

        public void Setup(string text, TextLocationEnum textLocation = TextLocationEnum.Auto, string name="", float rate=0, Vector2? at = null, int zDepth=0) {

            index = 0;
            // TODO - Remove HTML codes
            textLength = text.Length;

            this.textLocation = textLocation;
            speakerName = name;
            this.rate = rate;
            this.zDepth = zDepth;
        }

        IEnumerator Output() {
            while (true)
            {
                if (rate == 0 || index >= textLength)
                {
                    displayText = text;
                    yield break;
                }
                float wait = rate;
                string next = text[index++].ToString();
                displayText += next;
                if (".?!,".Contains(next))
                    wait *= punctuationModifier;
                if ("\n".Contains(next))
                    wait *= newPageModifier;
                yield return new WaitForSeconds(wait);
            }
        }
    }
}