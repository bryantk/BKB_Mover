using UnityEngine;
using System.Collections.Generic;

namespace BKB_TEXT
{
    [System.Serializable]
    public class VoxData
    {
        public string message;
        public List<string> choices;
        public bool hasChoices;
        public int defaultChoice;
        public MessageLocation position;
        public string name;
        public HorizontalAlignment nameLocation;
        public bool useTexture;
        public bool useSound;
        public bool noTearDown;
        public string JSON;


        public VoxData()
        {
            message = "";
            choices = new List<string>();
            defaultChoice = -1;
            hasChoices = false;
            position = MessageLocation.Auto;
            name = null;
            nameLocation = HorizontalAlignment.Left;
            useSound = true;
            useTexture = true;
            noTearDown = false;
            JSON = null;
        }

        public VoxData(string message, MessageLocation position = MessageLocation.Auto, string name = null, HorizontalAlignment nameLocation = HorizontalAlignment.Left,
                        List<string> choices = null,  bool useTexture = true, bool useSound = true, bool doNotTearDown = false, string JSON = "")
        {
            this.message = message;
            this.position = position;
            this.name = name;
            this.nameLocation = nameLocation;
            this.choices = choices;
            this.useTexture = useTexture;
            this.useSound = useSound;
            noTearDown = doNotTearDown;
            this.JSON = JSON;
        }

    }
}
