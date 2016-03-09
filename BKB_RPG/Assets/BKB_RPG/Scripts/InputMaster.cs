using UnityEngine;
using System;

public class InputMaster : MonoBehaviour {

    public static event EventHandler<InfoEventArgs<Vector2>> moveEvent;
    public delegate void ButtonDelegate();
    public static  ButtonDelegate okButtonEvent;

    Repeater _hor = new Repeater("Horizontal");
    Repeater _ver = new Repeater("Vertical");

    string[] _buttons = new string[] { "Fire1", "Fire2", "Fire3" };


    // Use this for initialization
    void Start () {
        okButtonEvent += t;
    }
	
    void t() {
        print("done");
    }


	// Update is called once per frame
	void Update () {
        int x = _hor.Update();
        int y = _ver.Update();
        if (x != 0 || y != 0)
        {
            //if (x != 0 && y != 0)
            //    y = 0;
            if (moveEvent != null)
                moveEvent(this, new InfoEventArgs<Vector2>(new Vector2(x, y)));
        }

        for (int i = 0; i < 3; ++i)
        {
            if (Input.GetButtonUp(_buttons[i]))
            {
                if (okButtonEvent != null)
                    okButtonEvent();
            }
        }
    }

}
