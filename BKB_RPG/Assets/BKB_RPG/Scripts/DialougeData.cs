using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using SimpleJSON;

namespace BKB_TEXT {
    public class DialougeData {
        const int charPerLine = 20 -1;
        const int linesPerWindow = 4 -1;

        public string text;
        public string speakerName;
        public float character_delay;
        public string _raw;
        public string _cleaned;
        public int _index;
        public int _length;

        public Dictionary<int, List<object>> cues;



        // name
        // z

        public DialougeData(string newText, string name="", float speed=-1, string extraJSON="") {
            cues = new Dictionary<int, List<object>>();
            _raw = newText;
            _index = 0;
            text = "";
            character_delay = speed;
            if (character_delay == -1)      // Get options
                character_delay = 0.05f;    // Average = 860 characters per minute, 15 characters per second, 0.25 per frame
            _parseText();
            if (extraJSON != "")
                _parseJSON(extraJSON);
            // if _cleaned > size, split and push all cues to next

        }

        void _parseText() {
            // parse cue commands

            string toParse = _raw;
            toParse.Replace("\t", "    ");
            string temp = "";
            while (toParse.Length > charPerLine)
            {
                // Handle '\n's
                int newLine = toParse.IndexOf('\n');
                if (newLine == -1)
                    newLine = 0;
                else if (newLine <= charPerLine)
                {
                    newLine += 1;
                    temp += toParse.Substring(0, newLine);
                    toParse = toParse.Substring(newLine, toParse.Length - newLine);
                    continue;
                }
                // Add '\n's as needed
                string part = toParse.Substring(0, charPerLine);
                int spaceID = part.LastIndexOf(' ');
                if (spaceID == -1)
                    spaceID = charPerLine;
                toParse = toParse.Insert(spaceID+1, "\n");
                // increment all cues after this?
            }
            temp += toParse;
            // if > lines \n, break it up
            _cleaned = temp;
            _length = _cleaned.Length;
            // parse out cues
            // split to lines

            // pad
            // pass extra to new data
            // set 0-limit
        }

        void _parseCues(string str) {

        }

        void _parseJSON(string jsonString) {
            JSONNode json = JSON.Parse(jsonString);

        }

        void _AddCue(int position, object command) {
            if (!cues.ContainsKey(position))
                cues[position] = new List<object>();
            cues[position].Add(command);
        }

        public float Advance() {
            if (_index >= _length || character_delay <= 0)
            {
                text = _cleaned;
                _index = _length;
                return -1;
            }
            char c = _cleaned[_index];
            // check Cue state, apply cue
            text += c;
            // and here
            _index++;
            float rate = 1f;
            if (c == ' ')
                rate = 0.1f;
            else if (".?!,".IndexOf(c) != -1)
                rate = 1.5f;
            else if ("\n".IndexOf(c) != -1)
                rate = 4;
            // if wait cue?
            if (Input.anyKey)
                rate /= 5;

            return Time.unscaledTime + character_delay * rate;
        }

        string _printDict() {
            string r = "{";
            foreach(var key in cues.Keys)
            {
                r += key.ToString() + ": [";
                List<object> l = cues[key];
                for (int j = 0; j < l.Count; j++)
                {
                    r += l[j].ToString() + ",";
                }
                r += "],";
            }
            return r + "}";
        }
    }
}