using UnityEngine;
using System.Collections;

namespace BKB_TEXT {
    public class Dialouge : MonoBehaviour {

        public string text;

        float _next;

        // Dictionary of text commands and thier start/end location

        // Use this for initialization
        void Start() {
            //index = 0;
            // TODO - Remove HTML codes
            //textLength = text.Length;
            //StartCoroutine(Output());
            S("123456789 cat <x000000>five n<xffffff>D ! hi there");
        }

        DialougeData data;

        public void S(string text) {
            data = new DialougeData(text);
            _next = Time.fixedTime + data.character_delay;
        }

        void Update() {
            ITick();
        }

        void ITick() {
            text = data.text;
            if (data._index >= data._length)
            {
                if (Input.anyKeyDown)
                    S("Time is " + Time.unscaledTime.ToString() + " and some bit. Average = 860 characters per minute, 15 characters per second, 0.25 per frame");
                return;
            }
            if (Time.unscaledTime >= _next)
                _next = data.Advance();
  


        }

        void OnGUI() {
            GUI.Label(new Rect(10, 10, 200, 120), text);
        }

    }
}