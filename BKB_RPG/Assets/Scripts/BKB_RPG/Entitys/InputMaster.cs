using UnityEngine;
using System;

public class InputMaster : MonoBehaviour {

    public static event EventHandler<InfoEventArgs<Vector2>> moveEvent;
    // TODO - think this one out better
    public static ButtonDelegate notMoving;
    public delegate void ButtonDelegate();
    public static  ButtonDelegate okButtonEvent;
    public static ButtonDelegate cancleButtonEvent;

    public static Repeater _hor = new Repeater("Horizontal");
    public static Repeater verticalRepeater = new Repeater("Vertical");

    string[] _buttons = new string[] { "Fire1", "Fire2", "Fire3" };

    private static bool isEnabled = true;

    public static void EnablePlayerInput() {
        isEnabled = true;
    }

    public static void DisablePlayerInput() {
        isEnabled = false;
    }

    // Update is called once per frame
    void Update () {
        if (!isEnabled)
            return;
        int x = _hor.Update();
        int y = verticalRepeater.Update();
        if (x != 0 || y != 0)
        {
            //if (x != 0 && y != 0)
            //    y = 0;
            if (moveEvent != null)
                moveEvent(this, new InfoEventArgs<Vector2>(new Vector2(x, y)));
        }
        else
        {
            if (notMoving != null)
                notMoving();
        }

        for (int i = 0; i < 3; ++i)
        {
            if (Input.GetButtonUp(_buttons[i]))
            {
                if (i == 0 && okButtonEvent != null)
                {
                    okButtonEvent();
                }
                else if (i == 1 && cancleButtonEvent != null)
                {
                    cancleButtonEvent();
                }
            }
                
        }
    }

}
