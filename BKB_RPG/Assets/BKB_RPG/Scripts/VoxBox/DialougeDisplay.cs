using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialougeDisplay : MonoBehaviour {

    public bool activated = false;
    public Text UIText;

    float nextTime;
    const float minReadTime = 0.25f;
    string text;
    Rect location = new Rect(10, 10, 500, 200);

    void Awake() {
        VoxBox.onEnter += onEnable;
        VoxBox.onBuildWindow += onBuildWindow;
        VoxBox.onTextUpdate += onTextUpdate;
        VoxBox.onWindowTeardown += onWindowTearDown;
        VoxBox.onExit += onExit;
        // Input
        InputMaster.okButtonEvent += OKButton;
    }

    void OKButton() {
        if (BKB_FSM.StateManager.GetState != "Vox")
            return;
        if (Time.time >= nextTime)
            VoxBox.ContinueMessages();
    }

    void onEnable() {
        BKB_FSM.StateManager.Push("Vox");
        nextTime = Time.time + minReadTime;
        activated = true;
        text = "";
        UIText.text = text;
       
    }

    IEnumerator onBuildWindow(object d) {
        UIText.gameObject.SetActive(true);
        yield return null;
    }

    void onTextUpdate(object d) {
        text = (string)d;
        UIText.text = text;
        nextTime = Time.time + minReadTime;
        //VoxBox.ContinueMessages();
    }

    IEnumerator onWindowTearDown(bool instant) {
        UIText.gameObject.SetActive(false);
        yield return null;
    }

    void onExit() {
        activated = false;
        BKB_FSM.StateManager.Pop();
    }



}
